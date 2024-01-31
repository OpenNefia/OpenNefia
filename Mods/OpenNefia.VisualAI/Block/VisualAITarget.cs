using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.VisualAI.Block
{
    [DataDefinition]
    public sealed class VisualAITarget
    {
        [DataField]
        public IVisualAICondition Filter { get; } = new AcceptAllCondition();

        [DataField]
        public IVisualAITargetOrdering? Ordering { get; }

        [DataField]
        public IVisualAITargetSource? Source { get; }
    }
}