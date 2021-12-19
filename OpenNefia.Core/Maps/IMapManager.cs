using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maths;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Core.Maps
{
    public interface IMapManager
    {
        IMap? ActiveMap { get; }

        public event Action<IMap>? ActiveMapChanged;

        void SetActiveMap(MapId mapId);

        bool IsMapInitialized(MapId mapId);

        bool MapExists(MapId mapId);
        IMap CreateMap(int width, int height, MapId? mapId = null);
        MapId RegisterMap(IMap map, MapId? mapId = null, EntityUid? mapEntityUid = null);

        /// <summary>
        /// Sets the MapEntity(root node) for a given map. If an entity is already set, it will be deleted
        /// before the new one is set.
        /// </summary>
        void SetMapEntity(MapId mapId, EntityUid newMapEntityId);

        MapId GetFreeMapId();
        IMap GetMap(MapId mapId);
        Entity GetMapEntity(MapId mapId);

        bool TryGetMap(MapId mapId, [NotNullWhen(true)] out IMap? map);
        bool TryGetMapEntity(MapId mapId, [NotNullWhen(true)] out Entity? mapEntity);

        void UnloadMap(MapId mapId);
    }
}