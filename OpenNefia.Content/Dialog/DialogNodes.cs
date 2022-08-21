using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.UI;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Log;

namespace OpenNefia.Content.Dialog
{
    public delegate void DialogActionDelegate(IDialogEngine engine, IDialogNode node);
    public delegate QualifiedDialogNode? DialogNodeDelegate(IDialogEngine engine, IDialogNode node);

    [ImplicitDataDefinitionForInheritors]
    public interface IDialogNode
    {
        QualifiedDialogNode? Invoke(IDialogEngine engine);
        QualifiedDialogNode? GetDefaultNode(IDialogEngine engine);
    }

    [DataDefinition]
    public sealed class DialogChoiceEntry
    {
        [DataField]
        public QualifiedDialogNodeID? NextNode { get; set; }

        [DataField]
        public DialogTextEntry Text { get; set; } = DialogTextEntry.FromString("");

        [DataField]
        public bool IsDefault { get; set; } = false;
    }

    public sealed class DialogTextOverride : IDialogExtraData
    {
        /// <summary>
        /// List of texts to override the next text node with. This is used for allowing a node to
        /// inherit the behavior/choices of another node.
        /// </summary>
        /// <remarks>
        /// See chat2.hsp:*chat_default in the HSP source, which checks if the chat buffer wasn't
        /// previously set. (buff="")
        /// </remarks>
        public IReadOnlyList<DialogTextEntry> Texts { get; }

        public DialogTextOverride(IReadOnlyList<DialogTextEntry> texts)
        {
            Texts = texts;
        }
    }

    public sealed class DialogJumpNode : IDialogNode
    {
        [DataField("texts", required: true)]
        private List<DialogTextEntry> _texts { get; } = new();

        public IReadOnlyList<DialogTextEntry> Texts => _texts;

        [DataField(required: true)]
        public QualifiedDialogNodeID NextNode { get; } = QualifiedDialogNodeID.Empty;

        [DataField]
        public DialogActionDelegate? BeforeEnter { get; }

        public QualifiedDialogNode? Invoke(IDialogEngine engine)
        {
            BeforeEnter?.Invoke(engine, this);

            if (_texts.Count == 0)
                return engine.GetNodeByID(NextNode);

            engine.Data.Add(new DialogTextOverride(Texts));
            return engine.GetNodeByID(NextNode);
        }

        public QualifiedDialogNode? GetDefaultNode(IDialogEngine engine) => engine.GetNodeByID(NextNode);
    }

    [DataDefinition]
    public sealed class DialogTextEntry
    {
        public DialogTextEntry() {}

        public static DialogTextEntry FromString(string text)
        {
            return new() { Text = text };
        }

        /// <summary>
        /// Creates a new text entry from a locale key.
        /// 
        /// This will autoformat the text with parameters from the dialog engine, such as the
        /// speaker entity, so you should use this instead of <see cref="FromString(string)"/>
        /// unless you require special formatting/parameters.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static DialogTextEntry FromLocaleKey(LocaleKey key)
        {
            return new() { Key = key };
        }

        [DataField]
        public string? Text { get; set; }

        [DataField]
        public LocaleKey? Key { get; set; }
    }

    public sealed class DialogTextNode : IDialogNode
    {
        public DialogTextNode() { }

        public DialogTextNode(List<DialogTextEntry> texts, List<DialogChoiceEntry> choices,
            DialogActionDelegate? beforeEnter = null, DialogActionDelegate? afterEnter = null)
        {
            _texts = texts;
            _choices = choices;
            BeforeEnter = beforeEnter;
            AfterEnter = afterEnter;
        }

        [DataField("texts", required: true)]
        private List<DialogTextEntry> _texts { get; } = new();

        public IReadOnlyList<DialogTextEntry> Texts => _texts;

        [DataField("choices", required: true)]
        private List<DialogChoiceEntry> _choices { get; } = new();

        public IReadOnlyList<DialogChoiceEntry> Choices => _choices;

        [DataField]
        public DialogActionDelegate? BeforeEnter { get; }

        [DataField]
        public DialogActionDelegate? AfterEnter { get; }

        [DataField]
        public LocaleKey ByeChoice { get; set; } = "Elona.Dialog.Common.Choices.Bye";

        private string GetLocalizedText(DialogTextEntry text, IDialogEngine engine)
        {
            if (text.Text != null)
                return text.Text;

            return Loc.GetString($"{text.Key}", ("speaker", engine.Speaker), ("player", engine.Player));
        }

        public QualifiedDialogNode? Invoke(IDialogEngine engine)
        {
            if (_texts.Count == 0)
                return null;

            var uiMan = IoCManager.Resolve<IUserInterfaceManager>();
            var entityMan = IoCManager.Resolve<IEntityManager>();
            var dialog = EntitySystem.Get<IDialogSystem>();

            IReadOnlyList<DialogTextEntry> entries = _texts;
            if (engine.Data.TryGet<DialogTextOverride>(out var textOverride))
            {
                entries = textOverride.Texts;
                engine.Data.Remove<DialogTextOverride>();
            }

            int defaultChoiceIndex;
            if (_choices.Count == 1)
                defaultChoiceIndex = 0;
            else
                defaultChoiceIndex = _choices.FindIndex(c => c.IsDefault);

            BeforeEnter?.Invoke(engine, this);

            UiResult<DialogResult>? result = null;

            for (var i = 0; i < _texts.Count; i++)
            {
                var entry = entries[i];

                List<DialogChoice> choices = new();
                if (i == _texts.Count - 1)
                {
                    if (_choices.Count == 0)
                    {
                        choices.Add(new DialogChoice()
                        {
                            Text = Loc.GetString(ByeChoice)
                        });
                    }
                    else
                    {
                        foreach (var choice in _choices)
                        {
                            choices.Add(new DialogChoice()
                            {
                                Text = GetLocalizedText(choice.Text, engine)
                            });
                        }
                    }
                }
                else
                {
                    choices.Add(new DialogChoice()
                    {
                        Text = Loc.GetString("Elona.Dialog.Common.Choices.More")
                    });
                }

                var speakerName = "";
                if (entityMan.IsAlive(engine.Speaker))
                    speakerName = dialog.GetDefaultSpeakerName(engine.Speaker.Value);

                var step = new DialogStepData()
                {
                    Target = engine.Speaker,
                    SpeakerName = speakerName,
                    Text = GetLocalizedText(entry, engine),
                    Choices = choices,
                    CanCancel = defaultChoiceIndex != -1
                };

                engine.DialogLayer.UpdateFromStepData(step);
                result = uiMan.Query(engine.DialogLayer);
            }

            AfterEnter?.Invoke(engine, this);

            if (result == null)
                return null;

            int choiceIndex = 0;
            if (result is UiResult<DialogResult>.Cancelled)
            {
                if (defaultChoiceIndex == -1)
                {
                    Logger.ErrorS("dialog.node", "Dialog menu was cancelled, but no default choice was found.");
                    return null;
                }

                choiceIndex = defaultChoiceIndex;
            }
            else if (result is UiResult<DialogResult>.Finished resultFinished)
            {
                choiceIndex = resultFinished.Value.SelectedChoiceIndex;
            }

            var nextNodeID = _choices.ElementAtOrDefault(choiceIndex)?.NextNode;
            if (nextNodeID == null)
            {
                return null;
            }

            return engine.GetNodeByID(nextNodeID.Value);
        }

        public QualifiedDialogNode? GetDefaultNode(IDialogEngine engine)
        {
            int defaultChoiceIndex = 0; // TODO

            var nextNodeID = _choices.ElementAtOrDefault(defaultChoiceIndex)?.NextNode;
            if (nextNodeID == null)
            {
                return null;
            }

            return engine.GetNodeByID(nextNodeID.Value);
        }
    }

    public sealed class DialogCallbackNode : IDialogNode
    {
        [DataField(required: true)]
        public DialogNodeDelegate Callback { get; } = default!;

        public QualifiedDialogNode? Invoke(IDialogEngine engine)
        {
            return Callback(engine, this);
        }

        public QualifiedDialogNode? GetDefaultNode(IDialogEngine engine) => Callback(engine, this);
    }
}
