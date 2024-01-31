using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.VisualAI.Block;

namespace OpenNefia.VisualAI.Engine
{
    [DataDefinition]
    public sealed class VisualAIBlock
    {
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

        // TODO
        [DataField]
        public IVisualAICondition? Condition { get; }

        // TODO
        [DataField]
        public IVisualAIAction? Action { get; }

        // TODO
        [DataField]
        public VisualAITarget? Target { get; }
    }
}