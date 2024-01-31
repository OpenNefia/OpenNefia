using OpenNefia.Content.Equipment;
using OpenNefia.Core.Input;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.VisualAI.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Audio;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Logic;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using static OpenNefia.VisualAI.Engine.VisualAITile;
using OpenNefia.Core.Utility;

namespace OpenNefia.VisualAI.UserInterface
{
    public sealed class VisualAIEditorGrid : UiElement
    {
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;

        public VisualAIPlan Plan { get; set; } = new();

        private VisualAITile?[,] Tiles = new VisualAITile?[1, 1];

        private Vector2i _cursor { get; set; } = Vector2i.Zero;
        private Vector2i Cursor
        {
            get => _cursor;
            set
            {
                _cursor = new(MathHelper.Wrap(value.X, 0, GridWidth - 1), MathHelper.Wrap(value.Y, 0, GridHeight - 1));
                Refresh();
            }
        }

        public delegate void GridUpdatedDelegate();
        public event GridUpdatedDelegate? OnRefreshed;

        private int GridWidth => Tiles.GetLength(0);
        private int GridHeight => Tiles.GetLength(1);

        private Vector2 Offset = Vector2.Zero;
        private Vector2 CanvasSize = Vector2.Zero;
        private float TileSize => 48;
        private float TilePadding => 8;

        private VisualAIBlockType _lastCategory = VisualAIBlockType.Target;

        private VisualAITile? SelectedTile => (Cursor.X >= 0 && Cursor.Y >= 0 && Cursor.X < GridWidth && Cursor.Y < GridHeight)
            ? Tiles[Cursor.X, Cursor.Y] : null;

        public VisualAIEditorGrid()
        {
            OnKeyBindDown += HandleKeyBindDown;
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            var hints = base.MakeKeyHints();

            var tile = SelectedTile;

            if (tile is VisualAITile.Empty)
            {
                hints.Add(new("VisualAI.UI.Editor.KeyHints.Append", EngineKeyFunctions.UISelect));
            }
            else if (tile is VisualAITile.Block block)
            {
                hints.Add(new("VisualAI.UI.Editor.KeyHints.Replace", EngineKeyFunctions.UISelect));
                hints.Add(new("VisualAI.UI.Editor.KeyHints.Insert", VisualAIKeyFunctions.Insert));
                hints.Add(new("VisualAI.UI.Editor.KeyHints.InsertDown", VisualAIKeyFunctions.InsertDown));
                hints.Add(new("VisualAI.UI.Editor.KeyHints.Delete", VisualAIKeyFunctions.Delete));
                hints.Add(new("VisualAI.UI.Editor.KeyHints.DeleteAndMergeDown", VisualAIKeyFunctions.DeleteAndMergeDown));
                hints.Add(new("VisualAI.UI.Editor.KeyHints.DeleteToRight", VisualAIKeyFunctions.DeleteToRight));

                if (block.BlockValue.Proto.Type == VisualAIBlockType.Condition)
                {
                    hints.Add(new("VisualAI.UI.Editor.KeyHints.SwapBranches", VisualAIKeyFunctions.SwapBranches));
                }
            }

            return hints;
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function.TryToDirection(out var dir))
            {
                Cursor += dir.ToIntVec();
                args.Handle();
            }
            else if (args.Function == EngineKeyFunctions.UISelect)
            {
                AppendBlock();
                args.Handle();
            }
            else if (args.Function == VisualAIKeyFunctions.Insert)
            {
                InsertBlock(VisualAIPlan.BranchTarget.TrueBranch);
                args.Handle();
            }
            else if (args.Function == VisualAIKeyFunctions.InsertDown)
            {
                InsertBlock(VisualAIPlan.BranchTarget.FalseBranch);
                args.Handle();
            }
            else if (args.Function == VisualAIKeyFunctions.Delete)
            {
                DeleteBlock(VisualAIPlan.BranchTarget.TrueBranch);
                args.Handle();
            }
            else if (args.Function == VisualAIKeyFunctions.DeleteAndMergeDown)
            {
                DeleteBlock(VisualAIPlan.BranchTarget.FalseBranch);
                args.Handle();
            }
            else if (args.Function == VisualAIKeyFunctions.DeleteToRight)
            {
                DeleteToRight();
                args.Handle();
            }
            else if (args.Function == VisualAIKeyFunctions.SwapBranches)
            {
                SwapBranches();
                args.Handle();
            }
        }

        private void AppendBlock()
        {
            var tile = SelectedTile;
            if (tile == null)
                return;

            _audio.Play(Protos.Sound.Ok1);

            if (tile is VisualAITile.Empty)
            {
                var args = new VisualAIInsertMenu.Args()
                {
                    Category = _lastCategory
                };
                var result = UserInterfaceManager.Query<VisualAIInsertMenu, VisualAIInsertMenu.Args, VisualAIInsertMenu.Result>(args);

                if (!result.HasValue)
                    return;

                var newBlock = result.Value.Block;

                if (!Plan.AddBlock(newBlock, out var error))
                {
                    _mes.Display(error);
                    return;
                }

                if (!newBlock.Proto.IsTerminal)
                    Cursor += (1, 0);
            }
            else if (tile is VisualAITile.Block blockTile)
            {
                var args = new VisualAIInsertMenu.Args()
                {
                    Category = blockTile.BlockValue.Proto.Type,
                    BlockID = blockTile.BlockValue.ProtoID
                };
                var result = UserInterfaceManager.Query<VisualAIInsertMenu, VisualAIInsertMenu.Args, VisualAIInsertMenu.Result>(args);

                if (!result.HasValue)
                    return;

                var newBlock = result.Value.Block;

                if (!Plan.ReplaceBlock(blockTile.BlockValue, newBlock, out var error))
                {
                    _mes.Display(error);
                    return;
                }

                if (!newBlock.Proto.IsTerminal)
                    Cursor += (1, 0);
            }

            Refresh();
        }

        private void InsertBlock(VisualAIPlan.BranchTarget splitType)
        {
            var tile = SelectedTile;
            if (tile is not VisualAITile.Block blockTile)
                return;

            _audio.Play(Protos.Sound.Ok1);

            var args = new VisualAIInsertMenu.Args()
            {
                Category = _lastCategory
            };
            var result = UserInterfaceManager.Query<VisualAIInsertMenu, VisualAIInsertMenu.Args, VisualAIInsertMenu.Result>(args);

            if (!result.HasValue)
                return;

            var newBlock = result.Value.Block;

            if (!Plan.InsertBlockBefore(blockTile.BlockValue, newBlock, out var error, splitType))
                _mes.Display(error);

            Refresh();
        }

        private void DeleteBlock(VisualAIPlan.BranchTarget splitType)
        {
            var tile = SelectedTile;
            if (tile is not VisualAITile.Block blockTile)
                return;

            _audio.Play(Protos.Sound.Ok1);

            Plan.RemoveBlock(blockTile.BlockValue, splitType);
            Refresh();
        }

        private void DeleteToRight()
        {
            var tile = SelectedTile;
            if (tile is not VisualAITile.Block blockTile)
                return;

            _audio.Play(Protos.Sound.Ok1);

            Plan.RemoveBlockAndRest(blockTile.BlockValue);
            Refresh();
        }

        private void SwapBranches()
        {
            var tile = SelectedTile;
            if (tile is not VisualAITile.Block blockTile || blockTile.BlockValue.Proto.Type != VisualAIBlockType.Condition)
                return;

            _audio.Play(Protos.Sound.Ok1);

            Plan.SwapBranches(blockTile.BlockValue);
            Refresh();
        }

        private void ResizeCanvasFromPlan()
        {
            var tiles = Plan.EnumerateTiles().ToList();
            var gridWidth = tiles.MaxBy(t => t.Position.X)?.Position.X ?? 1;
            var gridHeight = tiles.MaxBy(t => t.Position.Y)?.Position.Y ?? 1;

            Tiles = new VisualAITile?[gridWidth, gridHeight];

            foreach (var tile in tiles)
            {
                DebugTools.AssertNull(Tiles[tile.Position.X, tile.Position.Y]);
                Tiles[tile.Position.X, tile.Position.Y] = tile;
            }
        }

        private void RecalcOffsets()
        {
            var offsetX = 0f;
            var selectedX = Cursor.X * TileSize;
            if (selectedX + TileSize > Width)
            {
                offsetX = float.Max(Width - (selectedX + TileSize), GridWidth - (Width / TileSize) * -TileSize);
            }

            var offsetY = 0f;
            var selectedY = Cursor.Y * TileSize;
            if (selectedY + TileSize > Height)
            {
                offsetY = float.Max(Height - (selectedY + TileSize), GridHeight - (Height / TileSize) * -TileSize);
            }

            Offset = new(offsetX, offsetY);
        }

        private HashSet<Vector2i> _selected = new();
        private List<VisualAITile> _trailTiles = new();
        private int _trailIndex = 0;
        public VisualAIEditorTrail.Data TrailData => new(_trailTiles, _trailIndex);

        private IEnumerable<VisualAITile> AllTiles
        {
            get
            {
                for (var x = 0; x < GridWidth; x++)
                {
                    for (var y = 0; y < GridHeight; y++)
                    {
                        var tile = Tiles[x, y];
                        if (tile != null)
                            yield return tile;
                    }
                }
            }
        }

        private void RecalcActiveTrail()
        {
            _selected.Clear();
            _trailTiles.Clear();
            _trailIndex = 0;

            var selected = SelectedTile;
            if (selected == null)
                return;

            var seenPlans = new HashSet<VisualAIPlan>();

            var backwards = selected.Plan.Parent;
            while (backwards != null)
            {
                seenPlans.Add(backwards);
                backwards = backwards.Parent;
            }

            var forwards = selected.Plan.SubplanTrueBranch;
            while (forwards != null)
            {
                seenPlans.Add(forwards);
                forwards = forwards.SubplanTrueBranch;
            }

            foreach (var tile in AllTiles)
            {
                if (seenPlans.Contains(tile.Plan))
                {
                    _selected.Add(tile.Position);

                    if (tile is VisualAITile.Block)
                    {
                        _trailTiles.Add(tile);
                        if (tile == selected)
                            _trailIndex = _trailTiles.Count - 1;
                    }
                }
            }
        }

        public void Refresh()
        {
            ResizeCanvasFromPlan();
            RecalcOffsets();
            RecalcActiveTrail();
            OnRefreshed?.Invoke();
        }

        public static void DrawTile(float uiScale, PrototypeId<AssetPrototype>? icon, Color color, bool isSelected, Vector2 pos, float tileSize, float padding, int borderSize = 1)
        {
            var size = tileSize - padding * 2;

            Love.Graphics.SetColor(Color.White);
            GraphicsS.RectangleS(uiScale, Love.DrawMode.Fill, pos.X + padding - borderSize * 2, pos.Y + padding - borderSize * 2, size + borderSize * 4, size + borderSize * 4);
            Love.Graphics.SetColor(Color.Black);
            GraphicsS.RectangleS(uiScale, Love.DrawMode.Fill, pos.X + padding - borderSize, pos.Y + padding - borderSize, size + borderSize * 2, size + borderSize * 2);
            Love.Graphics.SetColor(color);
            GraphicsS.RectangleS(uiScale, Love.DrawMode.Fill, pos.X + padding, pos.Y + padding, size, size);

            if (icon != null)
            {
                Love.Graphics.SetColor(isSelected ? Color.White : Color.Gray);
                var instance = Assets.Get(icon.Value);
                instance.Draw(uiScale, pos.X + padding - borderSize * 4 + 1, pos.Y + padding - borderSize * 4 + 1, size + borderSize * 4, size + borderSize * 4);
            }
        }

        public override void Draw()
        {
            var tileSize = TileSize - TilePadding * 2;
            var pos = Position + Offset;

            GraphicsS.SetScissorS(UIScale, X, Y, Width, Height);

            // Grid backing
            Love.Graphics.SetColor(Color.Black.WithAlphaB(20));
            foreach (var tile in AllTiles)
            {
                var tilePos = pos + (tile.Position) * TileSize;
                GraphicsS.RectangleS(UIScale, Love.DrawMode.Fill, tilePos.X + TilePadding, tilePos.Y + TilePadding, tileSize, tileSize);
            }

            // Lines
            Love.Graphics.SetLineWidth(4 * UIScale);
            foreach (var tile in AllTiles)
            {
                if (tile is VisualAITile.Line line)
                {
                    var isSelected = _selected.Contains(line.Position);
                    Color color = line.Type switch
                    {
                        VisualAITile.LineType.Right => isSelected ? new(100, 100, 200) : new(50, 50, 100),
                        _ => isSelected ? new(200, 100, 100) : new(100, 50, 50)
                    };

                    Love.Graphics.SetColor(color);
                    var startPos = pos + line.Position * (TileSize * 1.5f);
                    var endPos = pos + line.EndPosition * (TileSize * 1.5f);
                    GraphicsS.LineS(UIScale, startPos, endPos);
                }
            }

            // Tiles
            Love.Graphics.SetLineWidth(1);
            foreach (var tile in AllTiles)
            {
                var tilePos = pos + (tile.Position) * TileSize;
                var isSelected = _selected.Contains(tile.Position);

                if (tile is VisualAITile.Empty)
                {
                    Love.Graphics.SetColor(new Color(0, 40, 80, 128));
                    GraphicsS.RectangleS(UIScale, Love.DrawMode.Fill, tilePos.X + TilePadding, tilePos.Y + TilePadding, tileSize, tileSize);
                    Love.Graphics.SetColor(new Color(200, 40, 40));
                    GraphicsS.RectangleS(UIScale, Love.DrawMode.Line, tilePos.X + TilePadding, tilePos.Y + TilePadding, tileSize, tileSize);
                }
                else if (tile is VisualAITile.Block block)
                {
                    Color tileColor = block.BlockValue.Proto.Color;
                    if (!isSelected)
                        tileColor = tileColor.Lighten(0.5f);
                    Love.Graphics.SetColor(tileColor);
                    DrawTile(UIScale, block.BlockValue.Proto.Icon, tileColor, isSelected, pos, tileSize, TilePadding);
                }
            }

            // Cursor
            var cursorPos = pos + Cursor * TileSize;
            Love.Graphics.SetColor(new Color(230, 230, 255, 128));
            GraphicsS.RectangleS(UIScale, Love.DrawMode.Fill, pos, (TileSize, TileSize));
            Love.Graphics.SetLineWidth(2 * UIScale);
            Love.Graphics.SetColor(new Color(64, 64, 64, 128));
            GraphicsS.RectangleS(UIScale, Love.DrawMode.Line, pos, (TileSize, TileSize));
            
            Love.Graphics.SetLineWidth(1);
            Love.Graphics.SetScissor();
        }
    }
}
