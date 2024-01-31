using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.VisualAI.Block;

namespace OpenNefia.VisualAI.Engine
{
    [Prototype("VisualAI.Block")]
    public sealed class VisualAIBlockPrototype : IPrototype, ISerializationHooks
    {
        [IdDataField]
        public string ID { get; set; } = default!;

        public VisualAIBlockType Type { get; private set; }

        [DataField]
        public Color Color { get; } = Color.White;

        [DataField]
        public PrototypeId<AssetPrototype>? Icon { get; }

        [DataField]
        public IVisualAICondition? Condition { get; }

        [DataField]
        public IVisualAIAction? Action { get; }

        [DataField]
        public VisualAITarget? Target { get; }

        [DataField]
        public bool IsSpecial { get; }

        [DataField("isTerminal")]
        private bool _isTerminal { get; }
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
            if (Condition != null)
            {
                found++;
                Type = VisualAIBlockType.Condition;
            }
            if (Action != null)
            {
                found++;
                Type = VisualAIBlockType.Action;
            }
            if (Target != null)
            {
                found++;
                Type = VisualAIBlockType.Target;
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