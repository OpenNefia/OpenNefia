using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Maps
{
    /// <summary>
    /// Interface for loading and saving map blueprints, which are human-readable
    /// YAML files containing map and entity data.
    /// </summary>
    public interface IMapBlueprintLoader
    {
        IMap? LoadBlueprint(MapId mapId, ResourcePath filepath);
        void SaveBlueprint(MapId mapId, ResourcePath filepath);
    }
}
