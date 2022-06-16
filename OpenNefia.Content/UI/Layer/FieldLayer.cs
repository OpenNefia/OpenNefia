using OpenNefia.Core.Rendering;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.Input;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Core.Audio;
using OpenNefia.Content.TitleScreen;

namespace OpenNefia.Content.UI.Layer
{
    public partial class FieldLayer : UiLayerWithResult<UINone, UINone>, IFieldLayer
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IMapRenderer _mapRenderer = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IGraphics _graphics = default!;
        [Dependency] private readonly IInputManager _inputManager = default!;
        [Dependency] private readonly IMusicManager _music = default!;

        public override int? DefaultZOrder => 100000;

        public static FieldLayer? Instance = null;

        public IMap Map { get; private set; } = default!;

        private UiScroller _scroller;

        public Camera Camera { get; private set; }

        public event IFieldLayer.ScreenRefreshDelegate OnScreenRefresh = default!;

        public FieldLayer()
        {
            _scroller = new UiScroller();
            Camera = new Camera(this);
        }

        public void Startup()
        {
            _mapManager.OnActiveMapChanged += OnActiveMapChanged;
            _graphics.OnWindowResized += (_) => RefreshScreen();

            Camera.Initialize();
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            _inputManager.Contexts.SetActiveContext("field");
        }

        /// <inheritdoc/>
        public bool IsInGame()
        {
            return this.IsInActiveLayerList() && _mapManager.ActiveMap != null;
        }

        private void OnActiveMapChanged(IMap newMap, IMap? oldMap)
        {
            SetMap(newMap);
        }

        public void SetMap(IMap map)
        {
            if (map == Map)
                return;

            Map = map;

            Camera.SetMapSize(map.Size);
            _mapRenderer.SetMap(Map);

            RefreshScreen();
        }

        public void RefreshScreen()
        {
            if (Map == null)
                return;

            _mapManager.RefreshVisibility(Map);
            OnScreenRefresh?.Invoke();

            var player = _gameSession.Player;

            if (_entityManager.IsAlive(player))
            {
                Camera.CenterOnTilePos(player);
            }
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            _mapRenderer.SetSize(width, height);

            var player = _gameSession.Player;
            if (_entityManager.IsAlive(player))
            {
                Camera.CenterOnTilePos(player);
            }
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            _mapRenderer.SetPosition(x, y);
        }

        public override void OnQuery()
        {
            _music.Play(Protos.Music.Field1);

            EntitySystem.Get<ITurnOrderSystem>().AdvanceState();
        }

        public override void OnQueryFinish()
        {
        }

        public override void Update(float dt)
        {
            if (Map.NeedsRedraw)
            {
                RefreshScreen();
                _mapRenderer.RefreshAllLayers();
            }

            _scroller.GetPositionDiff(dt, out var dx, out var dy);

            SetPosition((int)Camera.ScreenPos.X, (int)Camera.ScreenPos.Y);

            _mapRenderer.Update(dt);
        }

        public override void Draw()
        {
            Love.Graphics.SetColor(255, 255, 255);

            _mapRenderer.Draw();

            Love.Graphics.SetColor(255, 0, 0);

            var player = _gameSession.Player!;
            var playerSpatial = _entityManager.GetComponent<SpatialComponent>(player);
        }

        public override void Dispose()
        {
        }
    }
}
