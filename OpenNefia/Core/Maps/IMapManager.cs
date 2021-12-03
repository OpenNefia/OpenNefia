using OpenNefia.Core.GameObjects;

namespace OpenNefia.Core.Maps
{
    public interface IMapManager
    {
        IMap? ActiveMap { get; }

        void ChangeActiveMap(MapId id);
        MapId RegisterMap(IMap map);

        bool IsMapInitialized(MapId mapId);

        IMap LoadMap(MapId id);
        void SaveMap(MapId id);
        IMap GetMap(MapId id);
        void UnloadMap(MapId id);
    }
}