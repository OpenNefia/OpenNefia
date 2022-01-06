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
        public IReadOnlyDictionary<MapId, IMap> LoadedMaps { get; }

        public event ActiveMapChangedDelegate? ActiveMapChanged;

        void SetActiveMap(MapId mapId);

        bool IsMapInitialized(MapId mapId);

        bool MapIsLoaded(MapId mapId);
        IMap CreateMap(int width, int height);

        /// <summary>
        /// Sets the MapEntity(root node) for a given map. If an entity is already set, it will be deleted
        /// before the new one is set.
        /// </summary>
        void SetMapEntity(MapId mapId, EntityUid newMapEntityId);

        IMap GetMap(MapId mapId);

        bool TryGetMap(MapId mapId, [NotNullWhen(true)] out IMap? map);
        bool TryGetMap(EntityUid mapEntityUid, [NotNullWhen(true)] out IMap? map);

        void UnloadMap(MapId mapId);
    }
}