using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Maps
{
    public delegate void BlueprintEntityLoadedDelegate(EntityUid entity);

    /// <summary>
    /// Interface for loading and saving map blueprints, which are human-readable
    /// YAML files containing map and entity data.
    /// </summary>
    public interface IMapBlueprintLoader
    {
        public event BlueprintEntityLoadedDelegate OnBlueprintEntityLoaded;

        IMap LoadBlueprint(MapId? mapId, ResourcePath filepath);
        IMap LoadBlueprint(MapId? mapId, TextReader yamlStream);
        void SaveBlueprint(MapId? mapId, ResourcePath filepath);
    }
}
