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
using OpenNefia.Core.Random;

namespace OpenNefia.Content.Dialog
{
    /*
     * Some designs for the node types here were inspired by Skyrim's dialogue editor.
     */

    /// <summary>
    /// Represents code that can run before/after a dialog node, or similar, but does not change the
    /// dialog's control flow.
    /// </summary>
    /// <param name="engine">Current dialog engine.</param>
    /// <param name="node">Current dialog node.</param>
    public delegate void DialogActionDelegate(IDialogEngine engine, IDialogNode node);

    /// <summary>
    /// Represents code that can run in a dialog node that can influence its control flow.
    /// </summary>
    /// <param name="engine">Current dialog engine.</param>
    /// <param name="node">Current dialog node.</param>
    /// <returns>The next node in a dialog to jump to.</returns>
    public delegate QualifiedDialogNode? DialogNodeDelegate(IDialogEngine engine, IDialogNode node);

    /// <summary>
    /// Represents a node in the dialog graph.
    /// </summary>
    [ImplicitDataDefinitionForInheritors]
    public interface IDialogNode
    {
        /// <summary>
        /// Runs this node's logic.
        /// </summary>
        /// <param name="engine">Current dialog engine.</param>
        /// <returns>The next node to traverse to, or <c>null</c> to end the dialog.</returns>
        QualifiedDialogNode? Invoke(IDialogEngine engine);

        /// <summary>
        /// Gets the default node to traverse to.
        /// </summary>
        /// <remarks>
        /// This would be used in non-interactive testing scenarios.
        /// </remarks>
        /// <param name="engine">Current dialog engine.</param>
        /// <returns>The next node to traverse to, or <c>null</c> to end the dialog.</returns>
        QualifiedDialogNode? SelectDefaultNode(IDialogEngine engine);
    }

    public sealed class DialogTextOverride : IDialogExtraData
    {
        [DataField("texts")]
        private List<DialogTextEntry> _texts { get; } = new();

        /// <summary>
        /// List of texts to override the next text node with. This is used for allowing a node to
        /// inherit the behavior/choices of another node.
        /// </summary>
        /// <remarks>
        /// See chat2.hsp:*chat_default in the HSP source, which checks if the chat buffer wasn't
        /// previously set. (buff="")
        /// </remarks>
        public IReadOnlyList<DialogTextEntry> Texts => _texts;

        public DialogTextOverride() { }

        public DialogTextOverride(IEnumerable<DialogTextEntry> texts)
        {
            _texts = texts.ToList();
        }
    }

    public sealed class DialogJumpNode : IDialogNode
    {
        public DialogJumpNode() { }

        public DialogJumpNode(List<DialogTextEntry> texts, QualifiedDialogNodeID nextNode, List<IDialogAction>? beforeEnter = null)
        {
            _texts = texts;
            NextNode = nextNode;
            _beforeEnter = beforeEnter ?? new();
        }

        [DataField("texts")]
        private List<DialogTextEntry> _texts { get; } = new();

        public IReadOnlyList<DialogTextEntry> Texts => _texts;

        /// <summary>
        /// ID of the node to jump to.
        /// </summary>
        [DataField(required: true)]
        public QualifiedDialogNodeID NextNode { get; } = QualifiedDialogNodeID.Empty;

        /// <summary>
        /// Actions to execute before this node is entered.
        /// </summary>
        [DataField("beforeEnter")]
        public List<IDialogAction> _beforeEnter { get; } = new();
        public IReadOnlyList<IDialogAction> BeforeEnter => _beforeEnter;

        public QualifiedDialogNode? Invoke(IDialogEngine engine)
        {
            foreach (var action in BeforeEnter)
            {
                EntitySystem.InjectDependencies(action); // TODO auto-injection
                action.Invoke(engine, this);
            }

            if (_texts.Count == 0)
                return engine.GetNodeByID(NextNode);

            engine.Data.Add(new DialogTextOverride(Texts));
            return engine.GetNodeByID(NextNode);
        }

        public QualifiedDialogNode? SelectDefaultNode(IDialogEngine engine) => engine.GetNodeByID(NextNode);
    }

    /// <summary>
    /// Represents one text entry in a <see cref="DialogTextNode"/>. This can either contain a raw
    /// string or a locale key referencing the text to display.
    /// </summary>
    [DataDefinition]
    public sealed class DialogTextEntry
    {
        public DialogTextEntry() { }

        public static DialogTextEntry FromString(string text)
        {
            return new DialogTextEntry() { Text = text };
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
            return new DialogTextEntry() { Key = key };
        }

        /// <summary>
        /// Raw text to display.
        /// </summary>
        /// <remarks>
        /// Mutually exclusive with <see cref="Key"/>.
        /// </remarks>
        [DataField]
        public string? Text { get; internal set; }

        /// <summary>
        /// Locale key referencing the text to display. This can either be a string or list in the
        /// locale environment. In the list case, each entry in the list will be displayed one after
        /// the other.
        /// </summary>
        /// <remarks>
        /// Mutually exclusive with <see cref="Text"/>.
        /// </remarks>
        [DataField]
        public LocaleKey? Key { get; internal set; }

        /// <summary>
        /// If true, pick one text randomly from the locale list referenced by <see cref="Key"/>.
        /// Otherwise, treat the list as multiple sequental text entries.
        /// </summary>
        [DataField]
        public bool PickRandomly { get; set; } = false;

        [DataField("args")]
        private List<EntitySystemPropertyRef> _args { get; set; } = new();

        /// <summary>
        /// List of entity system properties to inject into the locale fetching.
        /// </summary>
        public IReadOnlyList<EntitySystemPropertyRef> Args => _args;

        /// <summary>
        /// Actions to execute before this text is displayed.
        /// </summary>
        [DataField("beforeEnter")]
        public List<IDialogAction> _beforeEnter { get; } = new();
        public IReadOnlyList<IDialogAction> BeforeEnter => _beforeEnter;

        /// <summary>
        /// Actions to execute after this text is displayed.
        /// </summary>
        [DataField("afterEnter")]
        public List<IDialogAction> _afterEnter { get; } = new();
        public IReadOnlyList<IDialogAction> AfterEnter => _afterEnter;
    }

    /// <summary>
    /// Represents a single choice in a <see cref="DialogTextNode"/>.
    /// </summary>
    [DataDefinition]
    public sealed class DialogChoiceEntry
    {
        /// <summary>
        /// The node to jump to when this choice is picked.
        /// </summary>
        [DataField]
        public QualifiedDialogNodeID? NextNode { get; set; }

        /// <summary>
        /// Text of this node.
        /// </summary>
        [DataField]
        public DialogTextEntry Text { get; set; } = DialogTextEntry.FromString("");

        /// <summary>
        /// If true, this choice can be automatically chosen by pressing the Cancel key when it is
        /// being displayed.
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        /// Only one item in a list of <see cref="DialogChoiceEntry"/> should have this set to
        /// <c>true</c>.
        /// </item>
        /// <item>
        /// A list containing only one choice automatically treats that choice as having <see
        /// cref="IsDefault"/> set to <c>true</c>.
        /// </item>
        /// </list>
        /// </remarks>
        [DataField]
        public bool IsDefault { get; set; } = false;

        /// <summary>
        /// Extra data to set if this choice is selected.
        /// </summary>
        [DataField("extraData")]
        public List<IDialogExtraData> ExtraData { get; set; } = new();
    }

    /// <summary>
    /// Dialog node for displaying text and choices. This is the usual node for scripting dialogs.
    /// </summary>
    public sealed class DialogTextNode : IDialogNode
    {
        public DialogTextNode() { }

        public DialogTextNode(List<DialogTextEntry> texts, List<DialogChoiceEntry> choices,
            List<IDialogAction>? beforeEnter = null, List<IDialogAction>? afterEnter = null)
        {
            _texts = texts;
            _choices = choices;
            if (beforeEnter != null)
                _beforeEnter.AddRange(beforeEnter);
            if (afterEnter != null)
                _afterEnter.AddRange(afterEnter);
        }

        [DataField("texts", required: true)]
        private List<DialogTextEntry> _texts { get; } = new();

        /// <summary>
        /// Texts to display.
        /// </summary>
        public IReadOnlyList<DialogTextEntry> Texts => _texts;

        [DataField("choices")]
        private List<DialogChoiceEntry> _choices { get; } = new();

        /// <summary>
        /// Choices to display for the last text entry.
        /// </summary>
        public IReadOnlyList<DialogChoiceEntry> Choices => _choices;

        /// <summary>
        /// Actions to execute before this node is entered.
        /// </summary>
        [DataField("beforeEnter")]
        public List<IDialogAction> _beforeEnter { get; } = new();
        public IReadOnlyList<IDialogAction> BeforeEnter => _beforeEnter;

        /// <summary>
        /// Actions to execute after this node is exited.
        /// </summary>
        [DataField("afterEnter")]
        public List<IDialogAction> _afterEnter { get; } = new();
        public IReadOnlyList<IDialogAction> AfterEnter => _afterEnter;

        [DataField]
        public LocaleKey ByeChoice { get; set; } = "Elona.Dialog.Common.Choices.Bye";

        private IReadOnlyList<string> GetLocalizedText(DialogTextEntry text, IDialogEngine engine)
        {
            // Use the raw string if it's available.
            if (text.Text != null)
                return new List<string>() { text.Text };

            // Pass default locale arguments.
            var args = new List<LocaleArg>()
            {
                ("speaker", engine.Speaker),
                ("player", engine.Player)
            };

            // Inject extra locale arguments
            if (text.Args.Count > 0)
            {
                var props = IoCManager.Resolve<IEntitySystemPropertiesManager>();
                object value;
                foreach (var (arg, i) in text.Args.WithIndex())
                {
                    try
                    {
                        var prop = props.GetProperty(arg);
                        value = prop.GetValue();
                    }
                    catch (Exception ex)
                    {
                        Logger.ErrorS("dialog.node", ex, "Error fetching locale argument");
                        value = "(error fetching locale argument)";
                    }

                    args.Add(($"arg{i}", value));
                }
            }

            var argsArray = args.ToArray();

            // Check lists. In Lua:
            //
            // Dialog.Text = { "Text 1.", "Text 2.", "Text 3" }
            if (Loc.TryGetList(text.Key!.Value, out var list, argsArray))
            {
                if (text.PickRandomly)
                {
                    // One line of text picked randomly.
                    return new List<string>() { IoCManager.Resolve<IRandom>().Pick(list) };
                }
                else
                {
                    // Sequential text.
                    return list;
                }
            }

            // Check single strings. In Lua:
            //
            // Dialog.Text = "Some text."
            return new List<string>() { Loc.GetString(text.Key!.Value, argsArray) };
        }

        /// <summary>
        /// Used for when only a single string is expected (choices).
        /// </summary>
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

            // Check if a previous dialog node set any text overrides. This can happen if the dialog
            // scriptor wants to inherit the choices of a specific node, but wants differing text.
            //
            // An example is showing "You kidding?" and returning the player to the list of default
            // "villager talk" choices.
            if (engine.Data.TryGet<DialogTextOverride>(out var textOverride))
            {
                entries = textOverride.Texts;
                engine.Data.Remove<DialogTextOverride>();
            }

            int defaultChoiceIndex = -1;

            foreach (var action in BeforeEnter)
            {
                EntitySystem.InjectDependencies(action); // TODO auto-injection
                action.Invoke(engine, this);
            }

            var queryLayerArgs = new QueryLayerArgs(/* NoHaltInput: true */); // BUG: reentrancy causes infinite loop
            UiResult<DialogResult>? result = null;

            for (var i = 0; i < _texts.Count; i++)
            {
                // Each entry could either reference a single string or multiple. This is because
                // the necessary amount of localized text could vary between languages, so it
                // doesn't always make sense to have a 1:1 mapping. Hence, the "list" form of the
                // localized text allows for the number of lines to vary.
                var entry = entries[i];
                var localizedTexts = GetLocalizedText(entry, engine);

                foreach (var action in entry.BeforeEnter)
                {
                    EntitySystem.InjectDependencies(action); // TODO auto-injection
                    action.Invoke(engine, this);
                }

                for (var j = 0; j < localizedTexts.Count; j++)
                {
                    var localizedText = localizedTexts[j];
                    defaultChoiceIndex = -1;

                    var isAtEndOfAllTexts = i == _texts.Count - 1 && j == localizedTexts.Count - 1;

                    List<DialogChoice> choices = new();
                    if (isAtEndOfAllTexts)
                    {
                        if (_choices.Count == 0)
                        {
                            // No choices were specified, default to a "Bye bye" choice that ends
                            // the dialog.
                            choices.Add(new DialogChoice()
                            {
                                Text = Loc.GetString(ByeChoice)
                            });
                            defaultChoiceIndex = 0;
                        }
                        else
                        {
                            // Show the standard list of choices after all text has been advanced.
                            foreach (var choice in _choices)
                            {
                                choices.Add(new DialogChoice()
                                {
                                    Text = GetSingleLocalizedText(choice.Text, engine)
                                });
                            }

                            // Canceling with only one choice is the same as selecting that choice.
                            // Else, the selected choice is the first choice that's set as the default.
                            if (_choices.Count == 1)
                                defaultChoiceIndex = 0;
                            else
                                defaultChoiceIndex = _choices.FindIndex(c => c.IsDefault);
                        }
                    }
                    else
                    {
                        // Still more text to show, so display "(More)".
                        choices.Add(new DialogChoice()
                        {
                            Text = Loc.GetString("Elona.Dialog.Common.Choices.More")
                        });
                        defaultChoiceIndex = 0;
                    }

                    var speakerName = "";
                    if (entityMan.IsAlive(engine.Speaker))
                        speakerName = dialog.GetDefaultSpeakerName(engine.Speaker.Value);

                    // Now we will ask the provided IDialogLayer to update its state and present the
                    // text/choices.
                    var step = new DialogStepData()
                    {
                        Target = engine.Speaker,
                        SpeakerName = speakerName,
                        Text = localizedText,
                        Choices = choices,
                        CanCancel = defaultChoiceIndex != -1
                    };

                    engine.DialogLayer.UpdateFromStepData(step);

                    // Intermediate results from the dialog layer are ignored; the final result will
                    // be used to determine the node to jump to.
                    result = uiMan.Query(engine.DialogLayer, queryLayerArgs);
                }

                foreach (var action in entry.AfterEnter)
                {
                    EntitySystem.InjectDependencies(action); // TODO auto-injection
                    action.Invoke(engine, this);
                }
            }

            foreach (var action in AfterEnter)
            {
                EntitySystem.InjectDependencies(action); // TODO auto-injection
                action.Invoke(engine, this);
            }

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

            var selectedChoice = _choices.ElementAtOrDefault(choiceIndex);
            if (selectedChoice == null)
                return null;

            foreach (var extraData in selectedChoice.ExtraData)
            {
                engine.Data.Add(extraData);
            }

            if (selectedChoice.NextNode == null)
                return null;

            return engine.GetNodeByID(selectedChoice.NextNode.Value);
        }

        public QualifiedDialogNode? SelectDefaultNode(IDialogEngine engine)
        {
            int defaultChoiceIndex = 0; // TODO

            var choice = _choices.ElementAtOrDefault(defaultChoiceIndex);
            if (choice == null)
                return null;

            foreach (var extraData in choice.ExtraData)
            {
                engine.Data.Add(extraData);
            }

            if (choice.NextNode == null)
                return null;

            return engine.GetNodeByID(choice.NextNode.Value);
        }
    }

    /// <summary>
    /// Dialog node that runs custom code and returns the next node to jump to.
    /// </summary>
    public sealed class DialogCallbackNode : IDialogNode
    {
        /// <summary>
        /// Code to run, which returns the next node.
        /// </summary>
        [DataField(required: true)]
        public DialogNodeDelegate Callback { get; } = default!;

        public QualifiedDialogNode? Invoke(IDialogEngine engine)
        {
            return Callback(engine, this);
        }

        public QualifiedDialogNode? SelectDefaultNode(IDialogEngine engine) => Callback(engine, this);
    }

    /// <summary>
    /// A testable condition for a <see cref="DialogBranchNode"/>.
    /// </summary>
    [ImplicitDataDefinitionForInheritors]
    public interface IDialogCondition
    {
        /// <summary>
        /// Gets the value of this condition. This can then be tested using a <see cref="ComparisonType"/>.
        /// </summary>
        /// <param name="engine"></param>
        /// <returns></returns>
        int GetValue(IDialogEngine engine);
    }

    /// <summary>
    /// Represents one branch arm of a <see cref="DialogBranchNode"/>.
    /// </summary>
    [DataDefinition]
    public sealed class DialogBranchCondition
    {
        /// <summary>
        /// Condition that will provide an integer value to test.
        /// </summary>
        [DataField(required: true)]
        public IDialogCondition Condition { get; set; } = default!;

        /// <summary>
        /// Comparison to be made on the condition's returned value.
        /// </summary>
        [DataField]
        public ComparisonType Comparison { get; set; } = ComparisonType.Equal;

        /// <summary>
        /// Value to compare the condition's with. For true/false conditions, <c>1</c> is true and
        /// <c>0</c> is false.
        /// </summary>
        [DataField(required: true)]
        public int Value { get; set; } = 0;

        /// <summary>
        /// Node to jump to if the conditional test passes.
        /// </summary>
        [DataField(required: true)]
        public QualifiedDialogNodeID? Node { get; set; }

        public bool Test(IDialogEngine engine)
        {
            EntitySystem.InjectDependencies(Condition); // TODO auto-injection
            var value = Condition.GetValue(engine);
            return ComparisonUtils.EvaluateComparison(value, Value, Comparison);
        }
    }

    /// <summary>
    /// Dialog node that tests one or more conditions and jumps to a node based on the result.
    /// </summary>
    public sealed class DialogBranchNode : IDialogNode
    {
        /// <summary>
        /// Default node to jump to if no conditions match.
        /// </summary>
        [DataField(required: true)]
        public QualifiedDialogNodeID? DefaultNode { get; }

        [DataField("conditions")]
        private List<DialogBranchCondition> _conditions { get; set; } = new();

        /// <summary>
        /// List of conditions. They will be evaluated in order, and the first one that matches will
        /// be chosen, if any.
        /// </summary>
        public IReadOnlyList<DialogBranchCondition> Conditions => _conditions;

        public QualifiedDialogNode? Invoke(IDialogEngine engine)
        {
            foreach (var condition in Conditions)
            {
                if (condition.Test(engine))
                {
                    if (condition.Node == null) // "end the dialog" case
                        return null;

                    return engine.GetNodeByID(condition.Node.Value);
                }
            }

            return SelectDefaultNode(engine);
        }

        public QualifiedDialogNode? SelectDefaultNode(IDialogEngine engine) => DefaultNode != null ? engine.GetNodeByID(DefaultNode.Value) : null;
    }
}