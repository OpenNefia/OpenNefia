using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maths;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Core.Maps
{
    public interface IMapManager
    {
        IMap? ActiveMap { get; }

        void ChangeActiveMap(MapId mapId);
        MapId RegisterMap(IMap map);

        bool IsMapInitialized(MapId mapId);

        bool MapExists(MapId mapId);
        MapId CreateMap(MapId? mapId, int width, int height);
        IMap LoadMap(MapId mapId);
        void SaveMap(MapId mapId);
        IMap GetMap(MapId mapId);
        bool TryGetMap(MapId mapId, [NotNullWhen(true)] out IMap? map);
        void UnloadMap(MapId mapId);
    }
}