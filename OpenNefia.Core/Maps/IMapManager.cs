using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.SaveGames;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Core.Maps
{
    public enum MapLoadType
    {
        InitializeOnly,
        GameLoaded,
        Traveled,
        Full
    }

    public delegate void ActiveMapChangedDelegate(IMap newMap, IMap? oldMap, MapLoadType loadType);

    public interface IMapManager
    {
        IMap? ActiveMap { get; }

        /// <summary>
        /// All maps that are currently loaded in memory.
        /// </summary>
        IReadOnlyDictionary<MapId, IMap> LoadedMaps { get; }

        event ActiveMapChangedDelegate? OnActiveMapChanged;

        void SetActiveMap(MapId mapId, MapLoadType loadType = MapLoadType.Full);

        bool IsMapInitialized(MapId mapId);

        bool MapIsLoaded(MapId mapId);
        IMap CreateMap(int width, int height, PrototypeId<EntityPrototype>? mapEntityProto = null);
        IMap CreateMap(int width, int height, MapId mapId, PrototypeId<EntityPrototype>? mapEntityProto = null);

        /// <summary>
        /// Sets the MapEntity(root node) for a given map. If an entity is already set, it will be deleted
        /// before the new one is set.
        /// </summary>
        void SetMapEntity(MapId mapId, EntityUid newMapEntityId);

        IMap GetMap(MapId mapId);
        IMap GetMapOfEntity(EntityUid entity);

        bool TryGetMap(MapId mapId, [NotNullWhen(true)] out IMap? map);
        bool TryGetMapOfEntity(EntityUid entity, [NotNullWhen(true)] out IMap? map);

        void UnloadMap(MapId mapId);

        void RefreshVisibility(IMap map);

        /// <summary>
        /// Allocates a new MapID, incrementing the highest ID counter.
        /// </summary>
        MapId GenerateMapId();
    }
}