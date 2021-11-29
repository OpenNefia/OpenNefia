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
        public IEntity Player { get => _player!; }

        public ICoords Coords { get; set; } = new OrthographicCoords();

        public FieldLayer Field { get; } = default!;
    }

    public static class GameSession
    {
        public static IEntity Player { get => IoCManager.Resolve<IGameSessionManager>().Player; }
        public static ICoords Coords { get => IoCManager.Resolve<IGameSessionManager>().Coords; }
        public static FieldLayer Field { get => IoCManager.Resolve<IGameSessionManager>().Field; }

        public static IMap ActiveMap { get => IoCManager.Resolve<IMapManager>().ActiveMap; }
    }
}
