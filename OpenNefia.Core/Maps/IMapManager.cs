using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maths;
using OpenNefia.Core.SaveGames;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Core.Maps
{
    public delegate void ActiveMapChangedDelegate(IMap newMap, IMap? oldMap);

    public interface IMapManager
    {
        IMap? ActiveMap { get; }

        /// <summary>
        /// All maps that are currently loaded in memory.
        /// </summary>
        IReadOnlyDictionary<MapId, IMap> LoadedMaps { get; }

        event ActiveMapChangedDelegate? OnActiveMapChanged;

        void SetActiveMap(MapId mapId);

        bool IsMapInitialized(MapId mapId);

        bool MapIsLoaded(MapId mapId);
        IMap CreateMap(int width, int height);
        IMap CreateMap(int width, int height, MapId mapId);

        /// <summary>
        /// Sets the MapEntity(root node) for a given map. If an entity is already set, it will be deleted
        /// before the new one is set.
        /// </summary>
        void SetMapEntity(MapId mapId, EntityUid newMapEntityId);

        IMap GetMap(MapId mapId);

        bool TryGetMap(MapId mapId, [NotNullWhen(true)] out IMap? map);

        void UnloadMap(MapId mapId);

        void RefreshVisibility(IMap map);

        /// <summary>
        /// Allocates a new MapID, incrementing the highest ID counter.
        /// </summary>
        MapId GenerateMapId();
    }
}