using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maths;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Core.Maps
{
    public interface IMapManager
    {
        IMap? ActiveMap { get; }

        // TODO: Whatever map the current player is in == active map.
        // That way you can just do player.Coords.MapId = mapId and everything
        // is supposed to magically work.
        // It never really made sense to have the active and player maps be different
        // anyway, it was a source of bugs and general annoyance.
        void ChangeActiveMap(MapId mapId);

        bool IsMapInitialized(MapId mapId);

        bool MapExists(MapId mapId);
        IMap CreateMap(int width, int height, MapId? mapId = null);

        IMap GetMap(MapId mapId);
        Entity GetMapEntity(MapId mapId);

        bool TryGetMap(MapId mapId, [NotNullWhen(true)] out IMap? map);
        bool TryGetMapEntity(MapId mapId, [NotNullWhen(true)] out Entity? mapEntity);

        void UnloadMap(MapId mapId);
    }
}