using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.VisualAI.Block;

namespace OpenNefia.VisualAI.Engine
{
    /// <summary>
    /// Defines a block in the visual AI system.
    /// Blocks can have one of three types:
    /// - Target: Refines the AI's current target and/or prioritizes the target list.
    /// - Action: Makes the AI invoke an action in the game world.
    /// - Condition: Evaluates a condition and runs one of two subplans.
    /// </summary>
    [Prototype("VisualAI.Block")]
    public sealed class VisualAIBlockPrototype : IPrototype, ISerializationHooks, IVisualAIVariableTargets
    {
        [IdDataField]
        public string ID { get; set; } = default!;

        /// <summary>
        /// Type of this block. Inferred automatically from the
        /// properties you set.
        /// </summary>
        public VisualAIBlockType Type { get; private set; }

        /// <summary>
        /// Color of the block in the editor.
        /// </summary>
        [DataField]
        public Color Color { get; private set; } = Color.White;

        /// <summary>
        /// Icon for the block in the editor.
        /// </summary>
        [DataField]
        public PrototypeId<AssetPrototype>? Icon { get; private set; }

        private VisualAIVariableSet? _variables;
        public VisualAIVariableSet Variables
        {
            get
            {
                _variables ??= VisualAIHelpers.GetBlockVariables(this);
                return _variables;
            }
        }

        /// <summary>
        /// Define this to declare this block as a Target block.
        /// </summary>
        [DataField]
        public VisualAITarget? Target { get; }

        /// <summary>
        /// Define this to declare this block as a Condition block.
        /// </summary>
        [DataField]
        public IVisualAICondition? Condition { get; }

        /// <summary>
        /// Define this to declare this block as an Action block.
        /// </summary>
        [DataField]
        public IVisualAIAction? Action { get; }

        /// <summary>
        /// For internal use only.
        /// </summary>
        [DataField]
        public bool IsSpecial { get; }

        [DataField("isTerminal")]
        private bool _isTerminal { get; }
        /// <summary>
        /// If <c>true</c> and the block is an Action block, prevent running
        /// any more blocks past this one. Use this if the action should pass
        /// the AI's turn after invocation.
        /// </summary>
        public bool IsTerminal => _isTerminal && (Type == VisualAIBlockType.Action || Type == VisualAIBlockType.Special);

        /// <summary>
        /// If <c>false</c>, the values for this condition specified in the prototype
        /// will be used to define the block logic, instead of allowing the user
        /// to specify the values in-game for those properties annotated with
        /// <see cref="VisualAIVariableAttribute"/> or similar.
        /// </summary>
        [DataField]
        public bool CanConfigure { get; } = true;

        public void AfterDeserialization()
        {
            var found = 0;
            if (Target != null)
            {
                found++;
                Type = VisualAIBlockType.Target;
                if (Color == Color.White)
                    Color = new(234, 200, 140);
                Icon ??= new("VisualAI.IconSingleplayer");
            }
            if (Action != null)
            {
                found++;
                Type = VisualAIBlockType.Action;
                if (Color == Color.White)
                    Color = new(140, 180, 210);
                Icon ??= new("VisualAI.IconFlag");
            }
            if (Condition != null)
            {
                found++;
                Type = VisualAIBlockType.Condition;
                if (Color == Color.White)
                    Color = new(128, 118, 118);
                Icon ??= new("VisualAI.IconDownRight");
            }
            if (IsSpecial)
            {
                found++;
                Type = VisualAIBlockType.Special;
            }

            if (found == 0)
            {
                throw new InvalidDataException($"Visual AI block must have one of {nameof(Condition)}, {nameof(Action)} or {nameof(Target)} defined");
            }
            else if (found != 1)
            {
                throw new InvalidDataException($"Visual AI block has more than one of {nameof(Condition)}, {nameof(Action)} or {nameof(Target)} defined");
            }
        }

        internal void InjectDependencies()
        {
            if (Condition != null)
                EntitySystem.InjectDependencies(Condition);

            if (Action != null)
                EntitySystem.InjectDependencies(Action);

            if (Target != null)
            {
                if (Target.Source != null)
                    EntitySystem.InjectDependencies(Target.Source);

                if (Target.Filter != null)
                    EntitySystem.InjectDependencies(Target.Filter);

                if (Target.Ordering != null)
                    EntitySystem.InjectDependencies(Target.Ordering);
            }
        }
    }
}