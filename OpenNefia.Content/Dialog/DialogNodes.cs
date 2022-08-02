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

namespace OpenNefia.Content.Dialog
{
    public delegate QualifiedDialogNode? DialogNodeDelegate(IDialogEngine engine, IDialogNode node);

    [ImplicitDataDefinitionForInheritors]
    public interface IDialogNode
    {
        QualifiedDialogNode? Invoke(IDialogEngine engine);
        QualifiedDialogNode? GetDefaultNode(IDialogEngine engine);
    }

    [DataDefinition]
    public sealed class TextNodeChoice
    {
        [DataField]
        public string? NextNode { get; set; }

        [DataField]
        public LocaleKey Text { get; set; }
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
        public IReadOnlyList<LocaleKey> Texts { get; }

        public DialogTextOverride(IReadOnlyList<LocaleKey> texts)
        {
            Texts = texts;
        }
    }

    public sealed class DialogJumpNode : IDialogNode
    {
        [DataField("texts", required: true)]
        private List<LocaleKey> _texts { get; } = new();

        public IReadOnlyList<LocaleKey> Texts => _texts;

        [DataField(required: true)]
        public string NextNode { get; } = string.Empty;

        public QualifiedDialogNode? Invoke(IDialogEngine engine)
        {
            if (_texts.Count == 0)
                return engine.GetNodeByID(NextNode);

            engine.Data.Add(new DialogTextOverride(Texts));
            return engine.GetNodeByID(NextNode);
        }

        public QualifiedDialogNode? GetDefaultNode(IDialogEngine engine) => engine.GetNodeByID(NextNode);
    }

    public sealed class DialogTextNode : IDialogNode
    {
        public DialogTextNode() {}

        public DialogTextNode(List<LocaleKey> texts, List<TextNodeChoice> choices,
            DialogNodeDelegate? beforeEnter = null, DialogNodeDelegate? afterEnter = null)
        {
            _texts = texts;
            _choices = choices;
            BeforeEnter = beforeEnter;
            AfterEnter = afterEnter;
        }

        [DataField("texts", required: true)]
        private List<LocaleKey> _texts { get; } = new();

        public IReadOnlyList<LocaleKey> Texts => _texts;

        [DataField("choices", required: true)]
        private List<TextNodeChoice> _choices { get; } = new();

        public IReadOnlyList<TextNodeChoice> Choices => _choices;

        [DataField]
        public DialogNodeDelegate? BeforeEnter { get; } = default!;

        [DataField]
        public DialogNodeDelegate? AfterEnter { get; } = default!;

        public QualifiedDialogNode? Invoke(IDialogEngine engine)
        {
            if (_texts.Count == 0)
                return null;

            var uiMan = IoCManager.Resolve<IUserInterfaceManager>();
            var entityMan = IoCManager.Resolve<IEntityManager>();
            var dialog = EntitySystem.Get<IDialogSystem>();

            IReadOnlyList<LocaleKey> texts = _texts;
            if (engine.Data.TryGet<DialogTextOverride>(out var textOverride))
            {
                texts = textOverride.Texts;
                engine.Data.Remove<DialogTextOverride>();
            }

            UiResult<DialogResult>? result = null;

            for (var i = 0; i < _texts.Count; i++)
            {
                var text = texts[i];

                List<DialogChoice> choices = new();
                if (i == _texts.Count - 1)
                {
                    if (_choices.Count == 0)
                    {
                        choices.Add(new DialogChoice()
                        {
                            Text = Loc.GetString("Elona.Dialog.Common.Choices.Bye")
                        });
                    }
                    else
                    {
                        foreach (var choice in _choices)
                        {
                            choices.Add(new DialogChoice()
                            {
                                // TODO pass blackboard data
                                Text = Loc.GetString(choice.Text, ("speaker", engine.Target))
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
                if (entityMan.IsAlive(engine.Target))
                    speakerName = dialog.GetDefaultSpeakerName(engine.Target.Value);

                var step = new DialogStepData()
                {
                    Target = engine.Target,
                    SpeakerName = speakerName,
                    // TODO pass blackboard data
                    Text = Loc.GetString(text, ("speaker", engine.Target)),
                    Choices = choices
                };

                engine.DialogLayer.UpdateFromStepData(step);
                result = uiMan.Query(engine.DialogLayer);
            }

            if (result == null)
                return null;

            int defaultChoiceIndex = 0; // TODO
            int choiceIndex = 0;
            if (result is UiResult<DialogResult>.Cancelled)
            {
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

            return engine.GetNodeByID(nextNodeID);
        }

        public QualifiedDialogNode? GetDefaultNode(IDialogEngine engine)
        {
            int defaultChoiceIndex = 0; // TODO

            var nextNodeID = _choices.ElementAtOrDefault(defaultChoiceIndex)?.NextNode;
            if (nextNodeID == null)
            {
                return null;
            }

            return engine.GetNodeByID(nextNodeID);
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
