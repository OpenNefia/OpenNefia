using OpenNefia.Core.Areas;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;

namespace OpenNefia.Content.Nefia.Generator
{
    [ImplicitDataDefinitionForInheritors]
    public interface INefiaFloorType
    {
        public IMap? Generate(IArea area, MapId mapId, int generationAttempt, int floorNumber, Blackboard<NefiaGenParams> data);
    }
}
