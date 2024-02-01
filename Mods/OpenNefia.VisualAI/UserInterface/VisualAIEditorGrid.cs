using OpenNefia.Core.Input;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.VisualAI.Engine;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Audio;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Logic;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Utility;
using OpenNefia.Core;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.GameObjects;

namespace OpenNefia.VisualAI.UserInterface
{
    public sealed class VisualAIEditorGrid : UiElement
    {
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;

        public VisualAIPlan RootPlan { get; set; } = new();

        private sealed record IconAndTile(VisualAIBlockIcon Icon, VisualAITile.Block Tile);

        // Only one Empty or Block tile can occupy a space
        // However, there can also be a line tile on the same space
        private VisualAITile?[,] Tiles = new VisualAITile?[1, 1];
        private List<VisualAITile.Line> LineTiles = new();

        private List<IconAndTile> BlockIcons = new();

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

        public bool Enabled { get; set; } = true;

        public delegate void GridUpdatedDelegate();
        public event GridUpdatedDelegate? OnRefreshed;

        private int GridWidth => Tiles.GetLength(0);
        private int GridHeight => Tiles.GetLength(1);

        private Vector2 Offset = Vector2.Zero;
        private Vector2 CanvasSize = Vector2.Zero;

        private VisualAIBlockType _lastCategory = VisualAIBlockType.Target;

        private const float TileSize = VisualAIBlockIcon.DefaultSize;
        private const float TilePadding = VisualAIBlockIcon.DefaultPadding;

        private VisualAITile? SelectedTile => (Cursor.X >= 0 && Cursor.Y >= 0 && Cursor.X < GridWidth && Cursor.Y < GridHeight)
            ? Tiles[Cursor.X, Cursor.Y] : null;

        public VisualAIEditorGrid()
        {
            // TODO
            EntitySystem.InjectDependencies(this);

            CanControlFocus = true;
            EventFilter = UIEventFilterMode.Pass;
            OnKeyBindDown += HandleKeyBindDown;
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            var hints = base.MakeKeyHints();

            if (!Enabled)
                return hints;

            var tile = SelectedTile;

            if (tile is VisualAITile.Empty)
            {
                hints.Add(new(new LocaleKey("VisualAI.UI.Editor.KeyHints.Append"), EngineKeyFunctions.UISelect));
            }
            else if (tile is VisualAITile.Block block)
            {
                hints.Add(new(new LocaleKey("VisualAI.UI.Editor.KeyHints.Replace"), EngineKeyFunctions.UISelect));
                hints.Add(new(new LocaleKey("VisualAI.UI.Editor.KeyHints.Insert"), VisualAIKeyFunctions.Insert));
                hints.Add(new(new LocaleKey("VisualAI.UI.Editor.KeyHints.InsertDown"), VisualAIKeyFunctions.InsertDown));
                hints.Add(new(new LocaleKey("VisualAI.UI.Editor.KeyHints.Delete"), VisualAIKeyFunctions.Delete));
                hints.Add(new(new LocaleKey("VisualAI.UI.Editor.KeyHints.DeleteAndMergeDown"), VisualAIKeyFunctions.DeleteAndMergeDown));
                hints.Add(new(new LocaleKey("VisualAI.UI.Editor.KeyHints.DeleteToRight"), VisualAIKeyFunctions.DeleteToRight));

                if (block.BlockValue.Proto.Type == VisualAIBlockType.Condition)
                {
                    hints.Add(new(new LocaleKey("VisualAI.UI.Editor.KeyHints.SwapBranches"), VisualAIKeyFunctions.SwapBranches));
                }
            }

            return hints;
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (!Enabled)
            {
                return;
            }

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

            bool moveCursor = false;

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

                _lastCategory = result.Value.LastCategory;
                var newBlock = result.Value.Block;

                if (newBlock == null)
                    return;

                if (!tile.Plan.AddBlock(newBlock, out var error))
                {
                    _mes.Display(error);
                    return;
                }

                if (!newBlock.Proto.IsTerminal)
                moveCursor = true;
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

                _lastCategory = result.Value.LastCategory;
                var newBlock = result.Value.Block;

                if (newBlock == null)
                    return;

                if (!tile.Plan.ReplaceBlock(blockTile.BlockValue, newBlock, out var error))
                {
                    _mes.Display(error);
                    return;
                }

                if (!newBlock.Proto.IsTerminal)
                    moveCursor = true;
            }

            RebuildCanvas();

            // Can only move cursor here since the grid bounds are now updated accordingly.
            if (moveCursor)
                Cursor += (1, 0);
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

            _lastCategory = result.Value.LastCategory;
            var newBlock = result.Value.Block;

            if (newBlock == null)
                return;

            if (!tile.Plan.InsertBlockBefore(blockTile.BlockValue, newBlock, out var error, splitType))
                _mes.Display(error);

            RebuildCanvas();
        }

        private void DeleteBlock(VisualAIPlan.BranchTarget splitType)
        {
            var tile = SelectedTile;
            if (tile is not VisualAITile.Block blockTile)
                return;

            _audio.Play(Protos.Sound.Ok1);

            tile.Plan.RemoveBlock(blockTile.BlockValue, splitType);
            RebuildCanvas();
        }

        private void DeleteToRight()
        {
            var tile = SelectedTile;
            if (tile is not VisualAITile.Block blockTile)
                return;

            _audio.Play(Protos.Sound.Ok1);

            tile.Plan.RemoveBlockAndRest(blockTile.BlockValue);
            RebuildCanvas();
        }

        private void SwapBranches()
        {
            var tile = SelectedTile;
            if (tile is not VisualAITile.Block blockTile || blockTile.BlockValue.Proto.Type != VisualAIBlockType.Condition)
                return;

            _audio.Play(Protos.Sound.Ok1);

            tile.Plan.SwapBranches(blockTile.BlockValue);
            RebuildCanvas();
        }

        private void RefreshCanvasFromPlan()
        {
            var tiles = RootPlan.EnumerateTiles().ToList();
            var gridWidth = (tiles.MaxBy(t => t.Position.X)?.Position.X ?? 0) + 1;
            var gridHeight = (tiles.MaxBy(t => t.Position.Y)?.Position.Y ?? 0) + 1;

            Tiles = new VisualAITile?[gridWidth, gridHeight];

            foreach (var blockIcon in BlockIcons)
            {
                RemoveChild(blockIcon.Icon);
            }

            BlockIcons.Clear();
            LineTiles.Clear();

            var pos = Position + Offset;

            foreach (var tile in tiles)
            {
                if (tile is VisualAITile.Block blockTile)
                {
                    DebugTools.AssertNull(Tiles[tile.Position.X, tile.Position.Y]);
                    Tiles[tile.Position.X, tile.Position.Y] = tile;

                    var tilePos = pos + tile.Position * TileSize;
                    var isSelected = _selected.Contains(tile.Position);
                    var blockProto = blockTile.BlockValue.Proto;
                    var icon = new VisualAIBlockIcon(blockProto.Icon, blockProto.Color, isSelected, padding: TilePadding);
                    icon.SetSize(TileSize, TileSize);
                    icon.SetPosition(tilePos.X, tilePos.Y);
                    AddChild(icon);
                    BlockIcons.Add(new(icon, blockTile));
                }
                else if (tile is VisualAITile.Empty)
                {
                    DebugTools.AssertNull(Tiles[tile.Position.X, tile.Position.Y]);
                    Tiles[tile.Position.X, tile.Position.Y] = tile;
                }
                else if (tile is VisualAITile.Line line)
                {
                    LineTiles.Add(line);
                }
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
        private List<VisualAITile.Block> _trailTiles = new();
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

            var seenPlans = new HashSet<VisualAIPlan>() { selected.Plan };

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

                    if (tile is VisualAITile.Block blockTile)
                    {
                        _trailTiles.Add(blockTile);
                        if (tile == selected)
                            _trailIndex = _trailTiles.Count - 1;
                    }
                }
            }
        }


        public void Refresh()
        {
            RecalcOffsets();
            RecalcActiveTrail();
            OnRefreshed?.Invoke();
        }

        public void RebuildCanvas()
        {
            RefreshCanvasFromPlan();
            Refresh();
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);

            var pos = Position + Offset;

            foreach (var (icon, blockTile) in BlockIcons)
            {
                var tilePos = pos + (blockTile.Position) * TileSize;
                var isSelected = _selected.Contains(blockTile.Position);

                icon.SetSize(TileSize, TileSize);
                icon.SetPosition(tilePos.X, tilePos.Y);
                icon.IsSelected = isSelected;
            }
        }

        public override void Update(float dt)
        {
            foreach (var (icon, tile) in BlockIcons)
            {
                icon.Update(dt);
            }
        }

        public override void Draw()
        {
            var tileSize = TileSize - TilePadding * 2;
            var pos = Position + Offset;

            GraphicsS.SetScissorS(UIScale, X, Y, Width, Height);

            // Grid backing
            Love.Graphics.SetColor(Color.Black.WithAlphaB(20));
            for (var x = 0; x < GridWidth; x++)
            {
                for (var y = 0; y < GridHeight; y++)
                {
                    var tilePos = pos + new Vector2i(x, y) * TileSize;
                    GraphicsS.RectangleS(UIScale, Love.DrawMode.Fill, tilePos.X + TilePadding, tilePos.Y + TilePadding, tileSize, tileSize);
                }
            }

            // Lines
            Love.Graphics.SetLineWidth(4 * UIScale);
            foreach (var line in LineTiles)
            {
                var isSelected = _selected.Contains(line.Position);
                Color color = line.Type switch
                {
                    VisualAITile.LineType.Right => isSelected ? new(100, 100, 200) : new(50, 50, 100),
                    _ => isSelected ? new(200, 100, 100) : new(100, 50, 50)
                };

                Love.Graphics.SetColor(color);
                var startPos = pos + line.Position * TileSize + TileSize / 2;
                var endPos = pos + line.EndPosition * TileSize + TileSize / 2;
                GraphicsS.LineS(UIScale, startPos, endPos);
            }

            // Tiles
            Love.Graphics.SetLineWidth(1);
            foreach (var tile in AllTiles)
            {
                var tilePos = pos + (tile.Position) * TileSize;

                if (tile is VisualAITile.Empty)
                {
                    Love.Graphics.SetColor(new Color(0, 40, 80, 128));
                    GraphicsS.RectangleS(UIScale, Love.DrawMode.Fill, tilePos.X + TilePadding, tilePos.Y + TilePadding, tileSize, tileSize);
                    Love.Graphics.SetColor(new Color(200, 40, 40));
                    GraphicsS.RectangleS(UIScale, Love.DrawMode.Line, tilePos.X + TilePadding, tilePos.Y + TilePadding, tileSize, tileSize);
                }
            }

            foreach (var (icon, tile) in BlockIcons)
            {
                icon.Draw();
            }

            // Cursor
            var cursorPos = pos + Cursor * TileSize;
            Love.Graphics.SetColor(new Color(230, 230, 255, 128));
            GraphicsS.RectangleS(UIScale, Love.DrawMode.Fill, cursorPos, (TileSize, TileSize));
            Love.Graphics.SetLineWidth(2 * UIScale);
            Love.Graphics.SetColor(new Color(64, 64, 64, 128));
            GraphicsS.RectangleS(UIScale, Love.DrawMode.Line, cursorPos, (TileSize, TileSize));

            Love.Graphics.SetLineWidth(1);
            Love.Graphics.SetScissor();
        }
    }
}
