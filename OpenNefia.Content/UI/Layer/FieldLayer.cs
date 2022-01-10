using OpenNefia.Core.Rendering;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Game;
using OpenNefia.Core.Maths;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Content.UI.Hud;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Input;
using OpenNefia.Content.TurnOrder;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Core.Audio;

namespace OpenNefia.Content.UI.Layer
{
    public partial class FieldLayer : UiLayerWithResult<UINone, UINone>, IFieldLayer
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IMapRenderer _mapRenderer = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IHudLayer _hud = default!;
        [Dependency] private readonly ICoords _coords = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IGraphics _graphics = default!;
        [Dependency] private readonly IInputManager _inputManager = default!;
        [Dependency] private readonly IMusicManager _music = default!;

        public static FieldLayer? Instance = null;

        public IMap Map { get; private set; } = default!;

        private UiScroller _scroller;

        public Camera Camera { get; private set; }

        private FontSpec FontText = new(14, 12);

        public string Message { get; private set; }
        private string MouseText;
        private PrototypeId<TilePrototype>? PlacingTile = null;

        public FieldLayer()
        {
            _scroller = new UiScroller();
            Camera = new Camera(this);

            var result = PrintMessage("dood");
            Console.WriteLine($"Got back: {result}");
            Message = result;
            MouseText = "";

            BindKeys();
        }

        protected virtual void BindKeys()
        {
            //Keybinds[CoreKeybinds.Identify] += Examine;
            //Keybinds[CoreKeybinds.Escape] += PromptToQuit;
            //Keybinds[Keys.Ctrl | Keys.T] += QueryAtlas;
            //Keybinds[CoreKeybinds.North] += (_) => MovePlayer(Direction.North);
            //Keybinds[CoreKeybinds.South] += (_) => MovePlayer(Direction.South);
            //Keybinds[CoreKeybinds.West] += (_) => MovePlayer(Direction.West);
            //Keybinds[CoreKeybinds.East] += (_) => MovePlayer(Direction.East);
            //Keybinds[CoreKeybinds.Ascend] += Ascend;
            //Keybinds[CoreKeybinds.Descend] += Descend;
            //Keybinds[CoreKeybinds.Enter] += Activate;
            //Keybinds[CoreKeybinds.Repl] += QueryRepl;
            //Keybinds[Keys.G] += PickUpItem;
            //Keybinds[Keys.D] += DropItem;
            //Keybinds[Keys.Q] += DrinkItem;
            //Keybinds[Keys.T] += ThrowItem;
            //Keybinds[Keys.W] += QueryLayer;

            //_scroller.BindKeys(this);

            //MouseMoved.Callback += (evt) =>
            //{
            //    MouseText = $"{evt.Pos}";
            //};

            //MouseButtons[MouseButton.Mouse1].Bind((evt) => PlaceTile(evt), trackReleased: true);
            //MouseButtons[MouseButton.Mouse2].Bind((evt) => PlaceTile(evt), trackReleased: true);
            //MouseButtons[MouseButton.Mouse3].Bind((evt) => PlaceTile(evt), trackReleased: true);
        }


        public void Startup()
        {
            _mapManager.OnActiveMapChanged += OnActiveMapChanged;
            _graphics.OnWindowResized += (_) => RefreshScreen();

            Camera.Initialize();
        }

        public override void OnFocused()
        {
            _inputManager.Contexts.SetActiveContext("field");
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

            Map.RefreshVisibility();

            var player = _gameSession.Player;

            if (_entityManager.IsAlive(player))
            {
                Camera.CenterOnTilePos(player);
            }
        }

        private void PlaceTile(GUIBoundKeyEventArgs evt)
        {
            //if (evt.State == BoundKeyState.Down)
            //{
            //    if (evt.Button == MouseButton.Mouse1)
            //    {
            //        PlacingTile = Protos.Tile.Dirt;
            //    }
            //    else if (evt.Button == MouseButton.Mouse2)
            //    {
            //        PlacingTile = Protos.Tile.WallBrick;
            //    }
            //    else
            //    {
            //        PlacingTile = Protos.Tile.Flooring1;
            //    }
            //}
            //else
            //{
            //    PlacingTile = null;
            //}
        }

        public string PrintMessage(string dood)
        {
            Console.WriteLine($"Hi, I'm {dood}.");
            return dood + "?";
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            _mapRenderer.SetSize(width, height);
            _hud.SetSize(width, height);

            var player = _gameSession.Player;
            if (_entityManager.IsAlive(player))
            {
                Camera.CenterOnTilePos(player);
            }
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            _mapRenderer.SetPosition(x, y);
            _hud.SetPosition(0, 0);
        }

        public override void OnQuery()
        {
            _music.Play(Protos.Music.March1);

            EntitySystem.Get<ITurnOrderSystem>().AdvanceState();
        }

        public override void OnQueryFinish()
        {
        }

        private void QueryLayer(GUIBoundKeyEventArgs args)
        {
            //using (var layer = new TestLayer())
            //{
            //    Console.WriteLine("Query layer!");
            //    var result = layer.Query();
            //    Console.WriteLine($"Get result: {result}");
            //}
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

            if (PlacingTile != null)
            {
                var mouse = Love.Mouse.GetPosition();
                var tiledPos = _coords.ScreenToTile(new Vector2i((int)mouse.X - X, (int)mouse.Y - Y));

                if (Map.IsInBounds(tiledPos) && Map.Tiles[tiledPos.X, tiledPos.Y].ResolvePrototype().GetStrongID() != PlacingTile)
                {
                    var proto = PlacingTile.Value.ResolvePrototype();
                    if (proto.IsSolid)
                    {
                        Sounds.Play(Sound.Offer1, Map.AtPos(tiledPos));
                    }
                    Map.SetTile(tiledPos, PlacingTile.Value);
                    Map.MemorizeTile(tiledPos);
                }
            }

            _hud.Update(dt);
            _mapRenderer.Update(dt);
        }

        public override void Draw()
        {
            Love.Graphics.SetColor(255, 255, 255);

            _mapRenderer.Draw();

            Love.Graphics.SetColor(255, 0, 0);

            var player = _gameSession.Player!;
            var playerSpatial = _entityManager.GetComponent<SpatialComponent>(player);
            var screenPos = playerSpatial.GetScreenPos();
            Love.Graphics.Rectangle(Love.DrawMode.Line, X + screenPos.X, Y + screenPos.Y, _coords.TileSize.X, _coords.TileSize.Y);

            GraphicsEx.SetFont(FontText);
            GraphicsEx.SetColor(Color.White);
            Love.Graphics.Print(Message, 5, 5);
            Love.Graphics.Print(MouseText, 5, 20);
            Love.Graphics.Print($"Player: ({playerSpatial.MapPosition})", 5, 35);

            _hud.Draw();
        }

        public override void Dispose()
        {
        }
    }
}
