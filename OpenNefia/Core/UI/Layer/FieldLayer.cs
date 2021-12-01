using OpenNefia.Core.Data;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Hud;
using OpenNefia.Core.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Game;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Maths;
using OpenNefia.Core.GameController;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Logic;
using OpenNefia.Core.GameObjects.EntitySystems;

namespace OpenNefia.Core.UI.Layer
{
    public class FieldLayer : BaseUiLayer<UiNoResult>, IFieldLayer
    {
        [Dependency] private readonly ITileAtlasManager _atlasManager = default!;
        [Dependency] private readonly IAssetManager _assetManager = default!;
        [Dependency] private readonly IGameController _gameController = default!;
        [Dependency] private readonly IMapRenderer _mapRenderer = default!;

        public static FieldLayer? Instance = null;

        public IMap Map { get; private set; } = default!;

        private UiScroller _scroller;

        public Camera Camera { get; private set; }
        private UiFpsCounter FpsCounter;
        public HudLayer Hud { get; }

        private FontSpec FontText = new(14, 12);

        public string Message { get; private set; }
        private string MouseText;
        private TilePrototype? PlacingTile = null;

        public FieldLayer()
        {
            _scroller = new UiScroller();
            Camera = new Camera(this.Map, this);

            var result = PrintMessage("dood");
            Console.WriteLine($"Got back: {result}");
            Message = result;
            this.MouseText = "";

            FpsCounter = new UiFpsCounter();
            Hud = new HudLayer();

            this.BindKeys();
        }

        public void SetMap(IMap map)
        {
            if (map == this.Map)
                return;

            Map = (Map)map;

            Camera = new Camera(this.Map, this);
            _mapRenderer.SetMap(this.Map);

            RefreshScreen();
        }

        protected virtual void BindKeys()
        {
            this.Keybinds[CoreKeybinds.Identify] += QueryLayer;
            this.Keybinds[CoreKeybinds.Escape] += PromptToCancel;
            //this.Keybinds[Keys.Ctrl | Keys.S] += (_) => this.Save();
            //this.Keybinds[Keys.Ctrl | Keys.O] += (_) => this.Load();
            this.Keybinds[Keys.Ctrl | Keys.T] += QueryAtlas;
            this.Keybinds[CoreKeybinds.North] += (_) => this.MovePlayer(Direction.North);
            this.Keybinds[CoreKeybinds.South] += (_) => this.MovePlayer(Direction.South);
            this.Keybinds[CoreKeybinds.West] += (_) => this.MovePlayer(Direction.West);
            this.Keybinds[CoreKeybinds.East] += (_) => this.MovePlayer(Direction.East);
            //this.Keybinds[Keys.G] += (_) => this.GetItem();
            //this.Keybinds[Keys.D] += (_) => this.DropItem();
            //this.Keybinds[Keys.C] += (_) => this.CastSpell();
            //this.Keybinds[Keys.Q] += (_) => this.DrinkItem();
            //this.Keybinds[Keys.T] += (_) => this.ThrowItem();
            //this.Keybinds[Keys.Ctrl | Keys.B] += (_) => this.ActivateBeautify();
            //this.Keybinds[Keys.Period] += (_) => this.MovePlayer(0, 0);

            this._scroller.BindKeys(this);

            this.MouseMoved.Callback += (evt) =>
            {
                this.MouseText = $"{evt.Pos}";
            };

            this.MouseButtons[UI.MouseButtons.Mouse1].Bind((evt) => PlaceTile(evt), trackReleased: true);
            this.MouseButtons[UI.MouseButtons.Mouse2].Bind((evt) => PlaceTile(evt), trackReleased: true);
            this.MouseButtons[UI.MouseButtons.Mouse3].Bind((evt) => PlaceTile(evt), trackReleased: true);
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

        public void RefreshScreen()
        {
            Map.RefreshVisibility();

            var player = GameSession.Player;

            if (player != null)
            {
                Camera.CenterOn(player);
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
            if (PlayerQuery.YesOrNo("Quit to title screen?"))
                this.Cancel();
        }

        private void PlaceTile(UiMousePressedEventArgs evt)
        {
            if (evt.State == KeyPressState.Pressed)
            {
                if (evt.Button == UI.MouseButtons.Mouse1)
                {
                    PlacingTile = TilePrototypeOf.Dirt.ResolvePrototype();
                }
                else if (evt.Button == UI.MouseButtons.Mouse2)
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
            Hud.SetSize(width, height);

            var player = GameSession.Player;
            if (player != null)
            {
                Camera.CenterOn(player);
            }
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            _mapRenderer.SetPosition(x, y);
            FpsCounter.SetPosition(Width - FpsCounter.Text.Width - 5, 5);
            Hud.SetPosition(0, 0);
        }

        public override void OnQuery()
        {
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
            if (this.Map.NeedsRedraw)
            {
                RefreshScreen();
                this._mapRenderer.RefreshAllLayers();
            }

            this._scroller.GetPositionDiff(dt, out var dx, out var dy);

            this.SetPosition(Camera.ScreenPos.X, Camera.ScreenPos.Y);

            if (PlacingTile != null)
            {
                var mouse = Love.Mouse.GetPosition();
                var coords = GameSession.Coords;
                coords.ScreenToTile(new Vector2i((int)mouse.X - this.X, (int)mouse.Y - this.Y), out var tiledPos);
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

            this.Hud.Update(dt);
            this._mapRenderer.Update(dt);
            this.FpsCounter.Update(dt);
        }

        public override void Draw()
        {
            Love.Graphics.SetColor(255, 255, 255);

            this._mapRenderer.Draw();

            Love.Graphics.SetColor(255, 0, 0);

            var player = GameSession.Player!;
            player.GetScreenPos(out var screenPos);
            Love.Graphics.Rectangle(Love.DrawMode.Line, X + screenPos.X, Y + screenPos.Y, OrthographicCoords.TILE_SIZE, OrthographicCoords.TILE_SIZE);

            GraphicsEx.SetFont(this.FontText);
            GraphicsEx.SetColor(Color.White);
            Love.Graphics.Print(Message, 5, 5);
            Love.Graphics.Print(MouseText, 5, 20);
            Love.Graphics.Print($"Player: ({player.Spatial.Coords})", 5, 35);

            this.Hud.Draw();
            this.FpsCounter.Draw();
        }

        public override void Dispose()
        {
            this.Hud.Dispose();
            this.FpsCounter.Dispose();
        }
    }
}
