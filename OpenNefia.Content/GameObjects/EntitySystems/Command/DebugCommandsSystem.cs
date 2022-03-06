using OpenNefia.Content.UI.Hud;
using OpenNefia.Core.DebugView;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Input;
using OpenNefia.Core.Input.Binding;
using OpenNefia.Core.IoC;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.UserInterface;

namespace OpenNefia.Content.GameObjects
{
    public class DebugCommandsSystem : EntitySystem
    {
        [Dependency] private readonly IDebugViewLayer _debugViewLayer = default!;
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

        public override void Initialize()
        {
            CommandBinds.Builder
                .Bind(EngineKeyFunctions.ShowDebugView, InputCmdHandler.FromDelegate(ShowDebugView))
                .Register<CommonCommandsSystem>();
        }

        private TurnResult? ShowDebugView(IGameSessionManager? session)
        {
            if (session?.Player == null)
                return null;

            _debugViewLayer.ZOrder = HudLayer.HudZOrder + 10;
            _uiManager.PushLayer((UiLayer)_debugViewLayer);

            return TurnResult.NoResult;
        }
    }
}
