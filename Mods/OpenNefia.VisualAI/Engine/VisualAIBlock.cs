using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.VisualAI.Block;

namespace OpenNefia.VisualAI.Engine
{
    public interface IVisualAIVariableTargets
    {
        public VisualAIVariableSet Variables { get; }

        public VisualAITarget? Target { get; }
        public IVisualAICondition? Condition { get; }
        public IVisualAIAction? Action { get; }
    }

    [DataDefinition]
    public sealed class VisualAIBlock : IVisualAIVariableTargets
    {
        public VisualAIBlock() { }

        public VisualAIBlock(PrototypeId<VisualAIBlockPrototype> protoID, VisualAITarget? target, IVisualAICondition? condition, IVisualAIAction? action)
        {
            ProtoID = protoID;
            Target = target;
            Condition = condition;
            Action = action;
        }

        [DataField]
        public PrototypeId<VisualAIBlockPrototype> ProtoID { get; set; }

        private VisualAIBlockPrototype? _proto;
        public VisualAIBlockPrototype Proto
        {
            get
            {
                _proto ??= IoCManager.Resolve<IPrototypeManager>().Index(ProtoID);
                return _proto;
            }
        }

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
        /// Target with any set variables.
        /// </summary>
        [DataField]
        public VisualAITarget? Target { get; }

        /// <summary>
        /// Condition with any set variables.
        /// </summary>
        [DataField]
        public IVisualAICondition? Condition { get; }

        /// <summary>
        /// Action with any set variables.
        /// </summary>
        [DataField]
        public IVisualAIAction? Action { get; }

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