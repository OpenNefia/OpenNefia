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
using OpenNefia.Content.GameObjects;
using System.Diagnostics.Metrics;
using OpenNefia.Core.Utility;
using OpenNefia.Content.Sidequests;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;

namespace OpenNefia.Content.Dialog
{
    public delegate void DialogActionDelegate(IDialogEngine engine, IScriptableDialogNode node);
    public delegate QualifiedDialogNode? DialogNodeDelegate(IDialogEngine engine, IScriptableDialogNode node);

    [ImplicitDataDefinitionForInheritors]
    public interface IDialogNode
    {
        QualifiedDialogNode? Invoke(IDialogEngine engine);
        QualifiedDialogNode? GetDefaultNode(IDialogEngine engine);
    }

    public sealed record DialogScriptTarget(Type DelegateType, string Code);
    public sealed record DialogScriptResult(Dictionary<string, Dictionary<string, Delegate>> Callbacks);

    public interface IScriptableDialogNode : IDialogNode
    {
        /// <summary>
        /// Asks the dialog node to give a set of script code/target delegate types for compilation.
        /// </summary>
        void GetCodeToCompile(ref Dictionary<string, DialogScriptTarget> targets);

        /// <summary>
        /// Given the compiled code, asks the dialog node to update its delegate fields with the
        /// results.
        /// </summary>
        void AddCompiledCode(IReadOnlyDictionary<string, Delegate> compiled);
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

    public sealed class DialogJumpNode : IScriptableDialogNode
    {
        [DataField("texts", required: true)]
        private List<DialogTextEntry> _texts { get; } = new();

        public IReadOnlyList<DialogTextEntry> Texts => _texts;

        [DataField(required: true)]
        public QualifiedDialogNodeID NextNode { get; } = QualifiedDialogNodeID.Empty;

        /// <summary>
        /// Code to execute before this node is entered.
        /// </summary>
        [DataField]
        public string? BeforeEnter { get; }
        internal DialogActionDelegate? BeforeEnterCompiled { get; set; }

        public QualifiedDialogNode? Invoke(IDialogEngine engine)
        {
            BeforeEnterCompiled?.Invoke(engine, this);

            if (_texts.Count == 0)
                return engine.GetNodeByID(NextNode);

            engine.Data.Add(new DialogTextOverride(Texts));
            return engine.GetNodeByID(NextNode);
        }

        public QualifiedDialogNode? GetDefaultNode(IDialogEngine engine) => engine.GetNodeByID(NextNode);

        public void GetCodeToCompile(ref Dictionary<string, DialogScriptTarget> targets)
        {
            if (BeforeEnter != null)
                targets["BeforeEnter"] = new(typeof(DialogActionDelegate), BeforeEnter);
        }

        public void AddCompiledCode(IReadOnlyDictionary<string, Delegate> compiled)
        {
            if (compiled.TryGetValue("BeforeEnter", out var before))
                BeforeEnterCompiled = (DialogActionDelegate)before;
        }
    }

    [DataDefinition]
    public sealed class DialogTextEntry
    {
        public DialogTextEntry() { }

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
        public string? Text { get; internal set; }

        [DataField]
        public LocaleKey? Key { get; internal set; }
    }

    public sealed class DialogTextNode : IScriptableDialogNode
    {
        public DialogTextNode() { }

        public DialogTextNode(List<DialogTextEntry> texts, List<DialogChoiceEntry> choices,
            DialogActionDelegate? beforeEnter = null, DialogActionDelegate? afterEnter = null)
        {
            _texts = texts;
            _choices = choices;
            BeforeEnterCompiled = beforeEnter;
            AfterEnterCompiled = afterEnter;
        }

        [DataField("texts", required: true)]
        private List<DialogTextEntry> _texts { get; } = new();

        public IReadOnlyList<DialogTextEntry> Texts => _texts;

        [DataField("choices", required: true)]
        private List<DialogChoiceEntry> _choices { get; } = new();

        public IReadOnlyList<DialogChoiceEntry> Choices => _choices;

        [DataField]
        public string? BeforeEnter { get; }
        internal DialogActionDelegate? BeforeEnterCompiled { get; set; }

        [DataField]
        public string? AfterEnter { get; }
        internal DialogActionDelegate? AfterEnterCompiled { get; set; }

        [DataField]
        public LocaleKey ByeChoice { get; set; } = "Elona.Dialog.Common.Choices.Bye";

        private IReadOnlyList<string> GetLocalizedText(DialogTextEntry text, IDialogEngine engine)
        {
            if (text.Text != null)
                return new List<string>() { text.Text };

            var args = new LocaleArg[]
            {
                ("speaker", engine.Speaker),
                ("player", engine.Player)
            };

            // Check lists.
            //
            // Dialog.Text = { "Text 1.", "Text 2.", "Text 3" }
            if (Loc.TryGetList(text.Key!.Value, out var list, args))
                return list;

            // Check single strings.
            //
            // Dialog.Text = "Some text."
            return new List<string>() { Loc.GetString(text.Key!.Value, args) };
        }

        private string GetSingleLocalizedText(DialogTextEntry text, IDialogEngine engine)
        {
            if (text.Text != null)
                return text.Text;

            return Loc.GetString(text.Key!.Value, ("speaker", engine.Speaker), ("player", engine.Player));
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

            int defaultChoiceIndex = -1;

            BeforeEnterCompiled?.Invoke(engine, this);

            UiResult<DialogResult>? result = null;

            for (var i = 0; i < _texts.Count; i++)
            {
                var entry = entries[i];
                var localizedTexts = GetLocalizedText(entry, engine);

                for (var j = 0; j < localizedTexts.Count; j++)
                {
                    var localizedText = localizedTexts[j];
                    defaultChoiceIndex = -1;

                    List<DialogChoice> choices = new();
                    if (i == _texts.Count - 1 && j == localizedTexts.Count - 1)
                    {
                        if (_choices.Count == 0)
                        {
                            // "Bye bye."
                            choices.Add(new DialogChoice()
                            {
                                Text = Loc.GetString(ByeChoice)
                            });
                            defaultChoiceIndex = 0;
                        }
                        else
                        {
                            // Standard list of choices after all text has been advanced.
                            foreach (var choice in _choices)
                            {
                                choices.Add(new DialogChoice()
                                {
                                    Text = GetSingleLocalizedText(choice.Text, engine)
                                });
                            }
                            
                            // Canceling with only one choice is the same as selecting that choice.
                            // Else, it's the first choice that's set as the default.
                            if (_choices.Count == 1)
                                defaultChoiceIndex = 0;
                            else
                                defaultChoiceIndex = _choices.FindIndex(c => c.IsDefault);
                        }
                    }
                    else
                    {
                        // "(More)"
                        choices.Add(new DialogChoice()
                        {
                            Text = Loc.GetString("Elona.Dialog.Common.Choices.More")
                        });
                        defaultChoiceIndex = 0;
                    }

                    var speakerName = "";
                    if (entityMan.IsAlive(engine.Speaker))
                        speakerName = dialog.GetDefaultSpeakerName(engine.Speaker.Value);

                    var step = new DialogStepData()
                    {
                        Target = engine.Speaker,
                        SpeakerName = speakerName,
                        Text = localizedText,
                        Choices = choices,
                        CanCancel = defaultChoiceIndex != -1
                    };

                    engine.DialogLayer.UpdateFromStepData(step);
                    result = uiMan.Query(engine.DialogLayer);
                }
            }

            AfterEnterCompiled?.Invoke(engine, this);

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

        public void GetCodeToCompile(ref Dictionary<string, DialogScriptTarget> targets)
        {
            if (BeforeEnter != null)
                targets["BeforeEnter"] = new(typeof(DialogActionDelegate), BeforeEnter);
            if (AfterEnter != null)
                targets["AfterEnter"] = new(typeof(DialogActionDelegate), AfterEnter);
        }

        public void AddCompiledCode(IReadOnlyDictionary<string, Delegate> compiled)
        {
            if (compiled.TryGetValue("BeforeEnter", out var before))
                BeforeEnterCompiled = (DialogActionDelegate)before;
            if (compiled.TryGetValue("AfterEnter", out var after))
                AfterEnterCompiled = (DialogActionDelegate)after;
        }
    }

    public sealed class DialogCallbackNode : IScriptableDialogNode
    {
        [DataField(required: true)]
        public string Callback { get; } = default!;
        internal DialogNodeDelegate CallbackCompiled { get; set; } = default!;

        public QualifiedDialogNode? Invoke(IDialogEngine engine)
        {
            return CallbackCompiled(engine, this);
        }

        public QualifiedDialogNode? GetDefaultNode(IDialogEngine engine) => CallbackCompiled(engine, this);

        public void GetCodeToCompile(ref Dictionary<string, DialogScriptTarget> targets)
        {
            targets["Callback"] = new(typeof(DialogNodeDelegate), Callback);
        }

        public void AddCompiledCode(IReadOnlyDictionary<string, Delegate> compiled)
        {
            CallbackCompiled = (DialogNodeDelegate)compiled["Callback"];
        }
    }

    [ImplicitDataDefinitionForInheritors]
    public interface IDialogCondition
    {
        int GetValue(IDialogEngine engine);
    }

    public sealed class SidequestStateCondition : IDialogCondition
    {
        [Dependency] private readonly ISidequestSystem _sidequests = default!;

        [DataField]
        public PrototypeId<SidequestPrototype> SidequestID { get; set; }

        public int GetValue(IDialogEngine engine)
        {
            EntitySystem.InjectDependencies(this);

            return _sidequests.GetState(SidequestID);
        }
    }

    public sealed class FindEntitiesWithTagCondition : IDialogCondition
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly ITagSystem _tags = default!;

        [DataField]
        public PrototypeId<TagPrototype> Tag { get; set; }

        public int GetValue(IDialogEngine engine)
        {
            EntitySystem.InjectDependencies(this);

            var spatial = _entityManager.GetComponent<SpatialComponent>(engine.Player);
            return _tags.EntitiesWithTagInMap(spatial.MapID, Tag).Count();
        }
    }

    public sealed class FindEntitiesWithPrototypeCondition : IDialogCondition
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;

        [DataField]
        public PrototypeId<EntityPrototype> ProtoID { get; set; }

        public int GetValue(IDialogEngine engine)
        {
            EntitySystem.InjectDependencies(this);

            var spatial = _entityManager.GetComponent<SpatialComponent>(engine.Player);
            return _lookup.EntityQueryInMap<MetaDataComponent>(spatial.MapID)
                .Where(metadata => metadata.EntityPrototype?.GetStrongID() == ProtoID)
                .Count();
        }
    }

    [DataDefinition]
    public sealed class DialogBranchCondition
    {
        [DataField(required: true)]
        public IDialogCondition Condition { get; set; } = default!;

        [DataField]
        public ComparisonType Comparison { get; set; } = ComparisonType.Equal;

        [DataField(required: true)]
        public int Value { get; set; } = 0;

        [DataField(required: true)]
        public QualifiedDialogNodeID Node { get; set; }

        public bool Test(IDialogEngine engine)
        {
            var value = Condition.GetValue(engine);
            return ComparisonUtils.EvaluateComparison(value, Value, Comparison);
        }
    }

    public sealed class DialogBranchNode : IDialogNode
    {
        [DataField(required: true)]
        public QualifiedDialogNodeID? DefaultNode { get; }

        [DataField("conditions")]
        private List<DialogBranchCondition> _conditions { get; set; } = new();
        public IReadOnlyList<DialogBranchCondition> Conditions => _conditions;

        public QualifiedDialogNode? Invoke(IDialogEngine engine)
        {
            foreach (var condition in Conditions)
            {
                if (condition.Test(engine))
                    return engine.GetNodeByID(condition.Node);
            }

            return GetDefaultNode(engine);
        }

        public QualifiedDialogNode? GetDefaultNode(IDialogEngine engine) => DefaultNode != null ? engine.GetNodeByID(DefaultNode.Value) : null;
    }
}
