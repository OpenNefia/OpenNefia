namespace Why.Core.Maps
{
    public interface IMapManager
    {
        void ChangeCurrentMap(MapId id);
        IMap LoadMap(MapId id);
        MapId RegisterMap(IMap map);
        void SaveMap(MapId id);
        bool IsMapInitialized(MapId mapId);
    }
}