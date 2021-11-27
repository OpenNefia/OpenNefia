using OpenNefia.Core.Data;
using OpenNefia.Core.Data.Serial;
using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Effect;
using OpenNefia.Core.Logic;
using OpenNefia.Core.Map;
using OpenNefia.Core.Object;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Hud;
using OpenNefia.Game;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNefia.Core.UI.Layer
{
    public class FieldLayer : BaseUiLayer<UiNoResult>
    {
        public static FieldLayer? Instance = null;

        public Map Map { get; private set; }

        private UiScroller Scroller;
        public Camera Camera { get; }
        private MapRenderer MapRenderer;
        public MapDrawables MapDrawables { get; }
        private UiFpsCounter FpsCounter;
        public HudLayer Hud { get; }

        private FontDef FontText;

        public string Message { get; private set; }
        private string MouseText;
        private TileDef? PlacingTile = null;

        public List<Thing> Things;

        internal FieldLayer(Map map)
        {
            Map = map;

            Scroller = new UiScroller();
            Camera = new Camera(this.Map, this);
            Things = new List<Thing>();
            FontText = FontDefOf.WindowTitle;

            int x = 0;
            int y = 0;
            foreach (var pair in ThingRepo.Instance.Iter())
            {
                var thingData = pair.Value;
                Things.Add(new Thing(thingData, x, y));
                x += 1;
            }

            var result = PrintMessage("dood");
            Console.WriteLine($"Got back: {result}");
            Message = result;
            this.MouseText = "";

            FpsCounter = new UiFpsCounter();
            MapRenderer = new MapRenderer(this.Map);
            MapDrawables = new MapDrawables();
            Hud = new HudLayer();

            this.BindKeys();

            RefreshScreen();
        }

        protected virtual void BindKeys()
        {
            this.Keybinds[Keybind.Entries.Identify] += (state) => this.QueryLayer();
            this.Keybinds[Keybind.Entries.Escape] += (_) => this.PromptToCancel();
            this.Keybinds[Keys.Ctrl | Keys.S] += (_) => this.Save();
            this.Keybinds[Keys.Ctrl | Keys.O] += (_) => this.Load();
            this.Keybinds[Keys.Ctrl | Keys.T] += (_) => new PicViewLayer(Atlases.Tile.Image).Query();
            this.Keybinds[Keybind.Entries.North] += (_) => this.MovePlayer(0, -1);
            this.Keybinds[Keybind.Entries.South] += (_) => this.MovePlayer(0, 1);
            this.Keybinds[Keybind.Entries.West] += (_) => this.MovePlayer(-1, 0);
            this.Keybinds[Keybind.Entries.East] += (_) => this.MovePlayer(1, 0);
            this.Keybinds[Keys.G] += (_) => this.GetItem();
            this.Keybinds[Keys.D] += (_) => this.DropItem();
            this.Keybinds[Keys.C] += (_) => this.CastSpell();
            this.Keybinds[Keys.Q] += (_) => this.DrinkItem();
            this.Keybinds[Keys.T] += (_) => this.ThrowItem();
            this.Keybinds[Keys.Ctrl | Keys.B] += (_) => this.ActivateBeautify();
            this.Keybinds[Keys.Period] += (_) => this.MovePlayer(0, 0);

            this.Scroller.BindKeys(this);

            this.MouseMoved.Callback += (evt) =>
            {
                this.MouseText = $"{evt.X}, {evt.Y}";
            };

            this.MouseButtons[UI.MouseButtons.Mouse1].Bind((evt) => PlaceTile(evt), trackReleased: true);
            this.MouseButtons[UI.MouseButtons.Mouse2].Bind((evt) => PlaceTile(evt), trackReleased: true);
            this.MouseButtons[UI.MouseButtons.Mouse3].Bind((evt) => PlaceTile(evt), trackReleased: true);
        }

        public void RefreshScreen()
        {
            var player = Current.Player!;

            Camera.CenterOn(player);
            Map.RefreshVisibility();

            var coords = GraphicsEx.Coords;
            coords.TileToScreen(player.X, player.Y, out var listenerX, out var listenerY);
            listenerX += coords.TileWidth / 2;
            listenerY += coords.TileHeight / 2;
            Love.Audio.SetPosition(listenerX, listenerY, 0f);
        }

        private void MovePlayer(int dx, int dy)
        {
            var player = Current.Player;

            if (player != null)
            {
                CharaAction.Move(player, player.X + dx, player.Y + dy);
                RefreshScreen();
            }
        }

        private void GetItem()
        {
            var player = Current.Player;

            if (player != null)
            {
                var pos = player.GetTilePos()!.Value;
                var item = pos.GetMapObjects<Item>().FirstOrDefault();

                if (item != null && CharaAction.PickUpItem(player, item))
                {
                    Sounds.PlayOneShot(SoundDefOf.Get1, pos);

                    if (item.StackAll())
                    {
                        Sounds.PlayOneShot(SoundDefOf.Heal1);
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
            var player = Current.Player;

            if (player != null)
            {
                var item = player.Inventory.FirstOrDefault();

                if (item != null && CharaAction.DropItem(player, item))
                {
                    Sounds.PlayOneShot(SoundDefOf.Drop1, player.X, player.Y);

                    if (item.StackAll())
                    {
                        Sounds.PlayOneShot(SoundDefOf.AtkChaos);
                    }
                }

                RefreshScreen();
            }
        }

        private void DrinkItem()
        {
            var player = Current.Player;

            if (player != null)
            {
                var item = player.Inventory.Where(i => i.CanDrink(player)).FirstOrDefault();

                if (item != null)
                {
                    CharaAction.Drink(player, item);
                    Sounds.PlayOneShot(SoundDefOf.Drink1, player.X, player.Y);
                }

                RefreshScreen();
            }
        }

        private void ThrowItem()
        {
            var player = Current.Player;

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
                Spell.CastSpell(result.Value.ChoiceData, Current.Player!);
                RefreshScreen();
            }
        }

        public void PromptToCancel()
        {
            if (Input.YesOrNo("Quit to title screen?"))
                this.Cancel();
        }

        bool IsBeautify = false;

        private void ActivateBeautify()
        {
            if (IsBeautify)
                return;

            Console.WriteLine($"Applying beautify!");

            DefLoader.ApplyActiveThemes(new List<ThemeDef>() { ThemeDefOf.Beautify });
            Startup.RegenerateTileAtlases();
            MapRenderer.OnThemeSwitched();
            Map.Redraw();
            IsBeautify = true;
        }

        private void PlaceTile(MouseButtonEvent evt)
        {
            if (evt.State == KeyPressState.Pressed)
            {
                if (evt.Button == UI.MouseButtons.Mouse1)
                {
                    PlacingTile = TileDefOf.Dirt;
                }
                else if (evt.Button == UI.MouseButtons.Mouse2)
                {
                    PlacingTile = TileDefOf.WallBrick;
                }
                else
                {
                    PlacingTile = TileDefOf.Flooring1;
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

        public void Save()
        {
            Console.WriteLine("Saving...");
            Map.Save(Map, "TestMap.nbt");
        }

        public void Load()
        {
            Console.WriteLine("Loading...");
            var map = Map.Load("TestMap.nbt", Current.Game);
            Current.Game.ActiveMap = map;
            Map = map;
            MapRenderer.SetMap(Map);
            foreach (MapObject obj in this.Map.InnerPool)
            {
                if (obj.Uid == Current.Player?.Uid)
                {
                    Current.Player = (Chara)obj;
                }
            }
            RefreshScreen();
        }

        public override void SetSize(int width = 0, int height = 0)
        {
            base.SetSize(width, height);
            MapRenderer.SetSize(width, height);
            FpsCounter.SetSize(400, 500);
            Hud.SetSize(width, height);

            var player = Current.Player;
            if (player != null)
            {
                Camera.CenterOn(player);
            }
        }

        public override void SetPosition(int x = 0, int y = 0)
        {
            base.SetPosition(x, y);
            MapRenderer.SetPosition(x, y);
            MapDrawables.SetPosition(x, y);
            FpsCounter.SetPosition(Width - FpsCounter.Text.Width - 5, 5);
            Hud.SetPosition(0, 0);
        }

        public override void OnQuery()
        {
            Music.PlayMusic(MusicDefOf.Field1);
        }

        public override void OnQueryFinish()
        {
        }

        private void QueryLayer()
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
            if (this.Map._NeedsRedraw)
            {
                Map.RefreshVisibility();
                this.MapRenderer.RefreshAllLayers();
            }

            this.Scroller.GetPositionDiff(dt, out var dx, out var dy);

            this.SetPosition(Camera.ScreenX, Camera.ScreenY);

            if (PlacingTile != null)
            {
                var mouse = Love.Mouse.GetPosition();
                var coords = GraphicsEx.Coords;
                coords.ScreenToTile((int)mouse.X - this.X, (int)mouse.Y - this.Y, out var tileX, out var tileY);

                if (Map.GetTile(tileX, tileY) != PlacingTile)
                {
                    if (PlacingTile.IsSolid)
                    {
                        Sounds.PlayOneShot(SoundDefOf.Offer1, tileX, tileY);
                    }
                    Map.SetTile(tileX, tileY, PlacingTile);
                    Map.MemorizeTile(tileX, tileY);
                }
            }

            this.Hud.Update(dt);
            this.MapRenderer.Update(dt);
            this.MapDrawables.Update(dt);
            this.FpsCounter.Update(dt);
        }

        public override void Draw()
        {
            Love.Graphics.SetColor(255, 255, 255);

            this.MapRenderer.Draw();

            Love.Graphics.SetColor(255, 0, 0);

            var player = Current.Player!;
            player.GetScreenPos(out var sx, out var sy);
            GraphicsEx.LineRect(X + sx, Y + sy, OrthographicCoords.TILE_SIZE, OrthographicCoords.TILE_SIZE);

            GraphicsEx.SetFont(this.FontText);
            Love.Graphics.Print(Message, 5, 5);
            Love.Graphics.Print(MouseText, 5, 20);
            Love.Graphics.Print($"Player: ({player.X}, {player.Y})", 5, 35);

            this.MapDrawables.Draw();
            this.Hud.Draw();
            this.FpsCounter.Draw();
        }

        public override void Dispose()
        {
            this.Hud.Dispose();
            this.MapDrawables.Dispose();
            this.MapRenderer.Dispose();
            this.FpsCounter.Dispose();
        }
    }
}
