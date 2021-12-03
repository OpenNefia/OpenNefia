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
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Hud;
using OpenNefia.Content.Logic;
using OpenNefia.Content.UI.Layer.Repl;

namespace OpenNefia.Content.UI.Layer
{
    public class FieldLayer : BaseUiLayer<UiNoResult>, IFieldLayer
    {
        [Dependency] private readonly IMapRenderer _mapRenderer = default!;
        [Dependency] private readonly IHudLayer _hud = default!;
        [Dependency] private readonly ICoords _coords = default!;
        [Dependency] private readonly IPlayerQuery _playerQuery = default!;
        [Dependency] private readonly IReplLayer _repl = default!;

        public static FieldLayer? Instance = null;

        public IMap Map { get; private set; } = default!;

        private UiScroller _scroller;

        public Camera Camera { get; private set; }
        private UiFpsCounter FpsCounter;

        private FontSpec FontText = new(14, 12);

        public string Message { get; private set; }
        private string MouseText;
        private TilePrototype? PlacingTile = null;

        public FieldLayer()
        {
            _scroller = new UiScroller();
            Camera = new Camera(this);

            var result = PrintMessage("dood");
            Console.WriteLine($"Got back: {result}");
            Message = result;
            MouseText = "";

            FpsCounter = new UiFpsCounter();

            BindKeys();
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
            Keybinds[CoreKeybinds.Repl] += QueryRepl;
            //Keybinds[Keys.G] += (_) => GetItem();
            //Keybinds[Keys.D] += (_) => DropItem();
            //Keybinds[Keys.C] += (_) => CastSpell();
            //Keybinds[Keys.Q] += (_) => DrinkItem();
            //Keybinds[Keys.T] += (_) => ThrowItem();
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

            var player = GameSession.Player;

            if (player != null)
            {
                Camera.CenterOnTilePos(player);
            }
        }

        private void MovePlayer(Direction dir)
        {
            var player = GameSession.Player;

            if (player != null)
            {
                var ev = new MoveEventArgs()
                {
                    OldPosition = player.Spatial.Coords,
                    NewPosition = player.Spatial.Coords.Offset(dir.ToIntVec())
                };
                player.EntityManager.EventBus.RaiseLocalEvent(player.Uid, ev);
            }
        }

        /*
                private void GetItem()
                {
                    var player = GameSession.Player;

                    if (player != null)
                    {
                        var item = player.Coords.GetEntities().FirstOrDefault();

                        if (item != null && CharaAction.PickUpItem(player, item))
                        {
                            Sounds.Play(SoundPrototypeOf.Get1, pos);

                            if (item.StackAll())
                            {
                                Sounds.Play(SoundPrototypeOf.Heal1);
                            }
                        }

                        var drawable = new BasicAnimMapDrawable(BasicAnimDefOf.AnimSmoke);
                        drawable.SetPosition(Rand.NextInt(Love.Graphics.GetWidth()), Rand.NextInt(Love.Graphics.GetHeight()));
                        MapDrawables.Enqueue(drawable, player.GetTilePos());

                        RefreshScreen();
                    }
                }

                private void DropItem()
                {
                    var player = GameSession.Player;

                    if (player != null)
                    {
                        var item = player.Inventory.FirstOrDefault();

                        if (item != null && CharaAction.DropItem(player, item))
                        {
                            Sounds.Play(SoundPrototypeOf.Drop1, player.X, player.Y);

                            if (item.StackAll())
                            {
                                Sounds.Play(SoundPrototypeOf.AtkChaos);
                            }
                        }

                        RefreshScreen();
                    }
                }

                private void DrinkItem()
                {
                    var player = GameSession.Player;

                    if (player != null)
                    {
                        var item = player.Inventory.Where(i => i.CanDrink(player)).FirstOrDefault();

                        if (item != null)
                        {
                            CharaAction.Drink(player, item);
                            Sounds.Play(SoundPrototypeOf.Drink1, player.X, player.Y);
                        }

                        RefreshScreen();
                    }
                }

                private void ThrowItem()
                {
                    var player = GameSession.Player;

                    if (player != null)
                    {
                        var item = player.Inventory.Where(i => i.CanThrow(player)).FirstOrDefault();

                        if (item != null)
                        {
                            var posResult = new PositionPrompt(player).Query();
                            if (!posResult.HasValue)
                                return;

                            var targetPos = posResult.Value.Pos;
                            CharaAction.Throw(player, item, targetPos);
                        }

                        RefreshScreen();
                    }
                }

                private void CastSpell()
                {
                    var prompt = new Prompt<CastableDef>(DefStore<CastableDef>.Enumerate());
                    var result = prompt.Query();
                    if (result.HasValue)
                    {
                        Spell.CastSpell(result.Value.ChoiceData, GameSession.Player!);
                        RefreshScreen();
                    }
                }*/

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
                    PlacingTile = TilePrototypeOf.Dirt.ResolvePrototype();
                }
                else if (evt.Button == MouseButton.Mouse2)
                {
                    PlacingTile = TilePrototypeOf.WallBrick.ResolvePrototype();
                }
                else
                {
                    PlacingTile = TilePrototypeOf.Flooring1.ResolvePrototype();
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
            FpsCounter.SetSize(400, 500);
            _hud.SetSize(width, height);

            var player = GameSession.Player;
            if (player != null)
            {
                Camera.CenterOnTilePos(player);
            }
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            _mapRenderer.SetPosition(x, y);
            FpsCounter.SetPosition(Width - FpsCounter.Text.Width - 5, 5);
            _hud.SetPosition(0, 0);
        }

        public override void OnQuery()
        {
            Music.Play(MusicPrototypeOf.March1);
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
                var tileCoords = Map.AtPos(tiledPos);

                if (tileCoords.GetTile()?.Prototype != PlacingTile)
                {
                    if (PlacingTile.IsSolid)
                    {
                        Sounds.Play(SoundPrototypeOf.Offer1, tileCoords);
                    }
                    tileCoords.SetTile(PlacingTile.GetStrongID());
                    tileCoords.MemorizeTile();
                }
            }

            _hud.Update(dt);
            _mapRenderer.Update(dt);
            FpsCounter.Update(dt);
        }

        public override void Draw()
        {
            Love.Graphics.SetColor(255, 255, 255);

            _mapRenderer.Draw();

            Love.Graphics.SetColor(255, 0, 0);

            var player = GameSession.Player!;
            player.GetScreenPos(out var screenPos);
            Love.Graphics.Rectangle(Love.DrawMode.Line, X + screenPos.X, Y + screenPos.Y, _coords.TileSize.X, _coords.TileSize.Y);

            GraphicsEx.SetFont(FontText);
            GraphicsEx.SetColor(Color.White);
            Love.Graphics.Print(Message, 5, 5);
            Love.Graphics.Print(MouseText, 5, 20);
            Love.Graphics.Print($"Player: ({player.Spatial.Coords})", 5, 35);

            _hud.Draw();
            FpsCounter.Draw();
        }

        public override void Dispose()
        {
            FpsCounter.Dispose();
        }
    }
}
