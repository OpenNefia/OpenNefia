using OpenNefia.Core.Rendering;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Game;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Maths;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Content.UI.Hud;
using OpenNefia.Content.Logic;
using OpenNefia.Content.UI.Layer.Repl;
using OpenNefia.Core.Logic;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Graphics;

namespace OpenNefia.Content.UI.Layer
{
    public class FieldLayer : BaseUiLayer<UiNoResult>, IFieldLayer
    {
        [Dependency] private readonly IMapRenderer _mapRenderer = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IHudLayer _hud = default!;
        [Dependency] private readonly ICoords _coords = default!;
        [Dependency] private readonly IPlayerQuery _playerQuery = default!;
        [Dependency] private readonly IReplLayer _repl = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IGraphics _graphics = default!;

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

        public void Startup()
        {
            _mapManager.ActiveMapChanged += SetMap;
            _graphics.OnWindowResized += (_) => RefreshScreen();
        }

        public void SetMap(IMap map)
        {
            if (map == Map)
                return;

            Map = (Map)map;

            Camera.SetMapSize(map.Size);
            _mapRenderer.SetMap(Map);

            RefreshScreen();
        }

        protected virtual void BindKeys()
        {
            Keybinds[CoreKeybinds.Identify] += QueryLayer;
            Keybinds[CoreKeybinds.Escape] += PromptToCancel;
            //Keybinds[Keys.Ctrl | Keys.S] += (_) => Save();
            //Keybinds[Keys.Ctrl | Keys.O] += (_) => Load();
            Keybinds[Keys.Ctrl | Keys.T] += QueryAtlas;
            Keybinds[CoreKeybinds.North] += (_) => MovePlayer(Direction.North);
            Keybinds[CoreKeybinds.South] += (_) => MovePlayer(Direction.South);
            Keybinds[CoreKeybinds.West] += (_) => MovePlayer(Direction.West);
            Keybinds[CoreKeybinds.East] += (_) => MovePlayer(Direction.East);
            Keybinds[CoreKeybinds.Ascend] += Ascend;
            Keybinds[CoreKeybinds.Descend] += Descend;
            Keybinds[CoreKeybinds.Enter] += Activate;
            Keybinds[CoreKeybinds.Repl] += QueryRepl;
            Keybinds[Keys.G] += PickUpItem;
            Keybinds[Keys.D] += DropItem;
            //Keybinds[Keys.C] += (_) => CastSpell();
            Keybinds[Keys.Q] += DrinkItem;
            Keybinds[Keys.T] += ThrowItem;
            //Keybinds[Keys.Ctrl | Keys.B] += (_) => ActivateBeautify();
            //Keybinds[Keys.Period] += (_) => MovePlayer(0, 0);

            _scroller.BindKeys(this);

            MouseMoved.Callback += (evt) =>
            {
                MouseText = $"{evt.Pos}";
            };

            MouseButtons[MouseButton.Mouse1].Bind((evt) => PlaceTile(evt), trackReleased: true);
            MouseButtons[MouseButton.Mouse2].Bind((evt) => PlaceTile(evt), trackReleased: true);
            MouseButtons[MouseButton.Mouse3].Bind((evt) => PlaceTile(evt), trackReleased: true);
        }

        private void QueryAtlas(UiKeyInputEventArgs args)
        {
            var prompt = new Prompt<string>(new List<string>() { AtlasNames.Tile, AtlasNames.Chip });
            var result = prompt.Query();
            if (result.HasValue)
            {
                var atlas = IoCManager.Resolve<ITileAtlasManager>().GetAtlas(result.Value.ChoiceData);
                new PicViewLayer(atlas.Image).Query();
            }
        }

        private void QueryRepl(UiKeyInputEventArgs args)
        {
            _repl.Query();
        }

        public void RefreshScreen()
        {
            Map.RefreshVisibility();

            var player = _gameSession.Player;

            if (player != null)
            {
                Camera.CenterOnTilePos(player);
            }
        }

        private void MovePlayer(Direction dir)
        {
            var player = _gameSession.Player;

            if (player != null)
            {
                var oldPosition = player.Spatial.MapPosition;
                var newPosition = player.Spatial.MapPosition.Offset(dir.ToIntVec());
                var ev = new MoveEventArgs(oldPosition, newPosition);
                player.EntityManager.EventBus.RaiseLocalEvent(player.Uid, ev);
            }
        }

        private void RunVerbCommand(Verb verb, IEnumerable<Entity> ents)
        {
            var player = _gameSession.Player!;

            var verbSystem = EntitySystem.Get<VerbSystem>();

            foreach (var target in ents)
            {
                if (target.Uid != player.Uid)
                {
                    var verbs = verbSystem.GetLocalVerbs(target.Uid, player.Uid);
                    if (verbs.Contains(verb))
                    {
                        verbSystem.ExecuteVerb(player.Uid, target.Uid, verb);
                        break;
                    }
                }
            }

            RefreshScreen();
        }

        private IEnumerable<Entity> EntitiesUnderneath()
        {
            var player = _gameSession.Player!;
            var lookup = EntitySystem.Get<IEntityLookup>();
            return lookup.GetLiveEntitiesAtCoords(player.Spatial.MapPosition);
        }

        private IEnumerable<Entity> EntitiesInInventory()
        {
            var player = _gameSession.Player!;
            var inv = _entityManager.EnsureComponent<InventoryComponent>(player.Uid);
            return inv.Container.ContainedEntities.Select(uid => _entityManager.GetEntity(uid));
        }

        private void DrinkItem(UiKeyInputEventArgs args)
        {
            RunVerbCommand(new Verb(DrinkableSystem.VerbIDDrink), EntitiesUnderneath());
        }

        private void ThrowItem(UiKeyInputEventArgs args)
        {
            RunVerbCommand(new Verb(ThrowableSystem.VerbIDThrow), EntitiesUnderneath());
        }

        public void Ascend(UiKeyInputEventArgs args)
        {
            RunVerbCommand(new Verb(StairsSystem.VerbIDAscend), EntitiesUnderneath());
        }

        public void Descend(UiKeyInputEventArgs args)
        {
            RunVerbCommand(new Verb(StairsSystem.VerbIDDescend), EntitiesUnderneath());
        }

        public void Activate(UiKeyInputEventArgs args)
        {
            RunVerbCommand(new Verb(StairsSystem.VerbIDActivate), EntitiesUnderneath());
        }

        private void PickUpItem(UiKeyInputEventArgs args)
        {
            RunVerbCommand(new Verb(PickableSystem.VerbIDPickUp), EntitiesUnderneath());

            var stackSystem = EntitySystem.Get<IStackSystem>();

            var inv = _entityManager.GetComponent<InventoryComponent>(_gameSession.Player!.Uid);
            foreach (var ent in inv.Container.ContainedEntities.ToList())
            {
                if (!_entityManager.IsAlive(ent))
                    continue;

                stackSystem.TryStackAtSamePos(ent);
            }
        }

        private void DropItem(UiKeyInputEventArgs args)
        {
            RunVerbCommand(new Verb(PickableSystem.VerbIDDrop), EntitiesInInventory());

            var stackSystem = EntitySystem.Get<IStackSystem>();

            foreach (var ent in EntitiesUnderneath().ToList())
            {
                if (!_entityManager.IsAlive(ent.Uid))
                    continue;

                stackSystem.TryStackAtSamePos(ent.Uid);
            }
        }

        public void PromptToCancel(UiKeyInputEventArgs args)
        {
            if (_playerQuery.YesOrNo("Quit to title screen?"))
                Cancel();
        }

        private void PlaceTile(UiMousePressedEventArgs evt)
        {
            if (evt.State == KeyPressState.Pressed)
            {
                if (evt.Button == MouseButton.Mouse1)
                {
                    PlacingTile = Protos.Tile.Dirt;
                }
                else if (evt.Button == MouseButton.Mouse2)
                {
                    PlacingTile = Protos.Tile.WallBrick;
                }
                else
                {
                    PlacingTile = Protos.Tile.Flooring1;
                }
            }
            else
            {
                PlacingTile = null;
            }
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
            if (player != null)
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
            Music.Play(Protos.Music.March1);
        }

        public override void OnQueryFinish()
        {
        }

        private void QueryLayer(UiKeyInputEventArgs args)
        {
            using (var layer = new TestLayer())
            {
                Console.WriteLine("Query layer!");
                var result = layer.Query();
                Console.WriteLine($"Get result: {result}");
            }
        }

        public override void Update(float dt)
        {
            if (Map.NeedsRedraw)
            {
                RefreshScreen();
                _mapRenderer.RefreshAllLayers();
            }

            _scroller.GetPositionDiff(dt, out var dx, out var dy);

            SetPosition(Camera.ScreenPos.X, Camera.ScreenPos.Y);

            if (PlacingTile != null)
            {
                var mouse = Love.Mouse.GetPosition();
                _coords.ScreenToTile(new Vector2i((int)mouse.X - X, (int)mouse.Y - Y), out var tiledPos);

                if (Map.IsInBounds(tiledPos) && Map.Tiles[tiledPos.X, tiledPos.Y].ResolvePrototype().GetStrongID() != PlacingTile)
                {
                    var proto = PlacingTile.Value.ResolvePrototype();
                    if (proto.IsSolid)
                    {
                        Sounds.Play(Protos.Sound.Offer1, Map.AtPos(tiledPos));
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
            player.GetScreenPos(out var screenPos);
            Love.Graphics.Rectangle(Love.DrawMode.Line, X + screenPos.X, Y + screenPos.Y, _coords.TileSize.X, _coords.TileSize.Y);

            GraphicsEx.SetFont(FontText);
            GraphicsEx.SetColor(Color.White);
            Love.Graphics.Print(Message, 5, 5);
            Love.Graphics.Print(MouseText, 5, 20);
            Love.Graphics.Print($"Player: ({player.Spatial.MapPosition})", 5, 35);

            _hud.Draw();
        }

        public override void Dispose()
        {
        }
    }
}
