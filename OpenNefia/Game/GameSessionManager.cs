using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Layer;

namespace OpenNefia.Core.Game
{
    public class GameSessionManager : IGameSessionManager
    {
        public IEntity? _player;
        public IEntity Player { get => _player!; set => _player = value; }
    }

    public static class GameSession
    {
        public static IEntity Player { get => IoCManager.Resolve<IGameSessionManager>().Player; }

        public static IMap? ActiveMap { get => IoCManager.Resolve<IMapManager>().ActiveMap; }

        public static ICoords Coords { get => IoCManager.Resolve<ICoords>(); }
    }
}
