using OpenNefia.Content.Levels;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Content.Nefia
{
    /// <summary>
    /// Single nefia floor layout type (rogue, big room, maze, etc.).
    /// </summary>
    [ImplicitDataDefinitionForInheritors]
    public interface IVanillaNefiaLayout
    {
        IMap? Generate(IArea area, MapId mapId, int generationAttempt, int floorNumber, Blackboard<NefiaGenParams> data);

        void AfterGenerateMap(IArea area, IMap map, int floorNumber, Blackboard<NefiaGenParams> data) { }
    }
}
