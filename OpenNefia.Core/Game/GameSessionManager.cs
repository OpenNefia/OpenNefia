using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Layer;

namespace OpenNefia.Core.Game
{
    public class GameSessionManager : IGameSessionManager
    {
        public EntityUid Player { get; set; } = EntityUid.Invalid;

        public bool IsPlayer(EntityUid ent)
        {
            return ent == Player;
        }
    }

    [Obsolete("Move to dependency injection wherever possible")]
    public static class GameSession
    {
        public static EntityUid Player { get => IoCManager.Resolve<IGameSessionManager>().Player; }

        public static IMap? ActiveMap { get => IoCManager.Resolve<IMapManager>().ActiveMap; }

        public static ICoords Coords { get => IoCManager.Resolve<ICoords>(); }
    }
}
