using OpenNefia.Content.DebugView;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Input;
using OpenNefia.Core.Input.Binding;
using OpenNefia.Core.IoC;
using OpenNefia.Core.UserInterface;

namespace OpenNefia.Content.GameObjects
{
    public class DebugCommandsSystem : EntitySystem
    {
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IDebugViewLayer _debugView = default!;

        public override void Initialize()
        {
            CommandBinds.Builder
                .Bind(EngineKeyFunctions.ShowDebugView, InputCmdHandler.FromDelegate(ShowDebugView))
                .Register<DebugCommandsSystem>();
        }

        private TurnResult? ShowDebugView(IGameSessionManager? session)
        {
            if (session?.Player == null)
                return null;

            _uiManager.Query(_debugView);

            return TurnResult.NoResult;
        }
    }
}
