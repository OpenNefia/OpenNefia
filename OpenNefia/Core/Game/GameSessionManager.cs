using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;

namespace OpenNefia.Core.Game
{
    public class GameSessionManager : IGameSessionManager
    {
        public IEntity? _player;
        public IEntity Player { get => _player!; }
    }

    public static class GameSession
    {
        public static IEntity Player { get => IoCManager.Resolve<IGameSessionManager>().Player; }
    }
}
