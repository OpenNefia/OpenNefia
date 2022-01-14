using Love;
using OpenNefia.Content.MapVisibility;
using OpenNefia.Core.Game;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Content.Rendering
{
    public class ShadowBatch : BaseDrawable
    {
        private static readonly int[,] DECO = {
        //                 W           E            WE          S            S E          SW           SWE
        // 0000         0001         0010         0011         0100         0101         0110         0111
           { 0, 0,  0}, { 0, 1,  0}, { 1, 2,  0}, { 0, 0,  0}, { 1, 0,  0}, { 0, 0,  0}, {-1, 21, 0}, {-1, 30, 0},  //  00000000
           { 2, 1,  0}, {-1, 20, 0}, { 2, 2,  0}, {-1, 33, 0}, { 2, 0,  0}, {-1, 32, 0}, {-1, 31, 0}, { 3, 1,  0},  //  00001000 N
           {-1, 1,  0}, { 0, 1,  0}, { 1, 2,  0}, { 0, 2,  0}, { 1, 0,  0}, { 0, 0,  0}, {-1, 21, 0}, {-1, 30, 0},  //  00010000
           { 2, 1,  1}, {-1, 20, 0}, { 2, 2,  0}, {-1, 33, 0}, { 2, 0,  0}, {-1, 32, 0}, {-1, 31, 0}, { 3, 1,  0},  //  00011000 N
           {-1, 2,  0}, { 0, 1,  0}, { 1, 2,  0}, { 0, 2,  0}, { 1, 0,  0}, { 0, 0,  0}, {-1, 21, 0}, {-1, 30, 0},  //  00100000
           { 2, 1,  0}, {-1, 20, 0}, { 2, 2,  0}, {-1, 33, 0}, { 2, 0,  0}, {-1, 32, 0}, {-1, 31, 0}, { 3, 1,  0},  //  00101000 N
           {-1, 5,  0}, { 0, 1,  2}, { 1, 2,  1}, { 0, 2,  0}, { 1, 0,  2}, { 0, 0,  2}, {-1, 21, 0}, {-1, 30, 0},  //  00110000
           { 2, 1,  1}, {-1, 20, 0}, { 2, 2,  1}, {-1, 33, 0}, { 2, 0,  0}, {-1, 32, 0}, {-1, 31, 0}, { 3, 1,  0},  //  00111000 N
           {-1, 3,  0}, { 0, 1,  0}, { 1, 2,  0}, { 0, 2,  0}, { 1, 0,  0}, { 0, 0,  0}, {-1, 21, 0}, {-1, 30, 0},  //  01000000
           { 2, 1,  0}, {-1, 20, 0}, { 2, 2,  0}, {-1, 33, 0}, { 2, 0,  0}, {-1, 32, 0}, {-1, 31, 0}, { 3, 1,  0},  //  01001000 N
           {-1, 9,  0}, { 0, 1,  0}, { 1, 2,  1}, { 0, 2,  0}, { 1, 0,  3}, { 0, 0,  0}, {-1, 21, 0}, {-1, 30, 0},  //  01010000
           { 2, 1,  0}, {-1, 20, 0}, { 2, 2,  0}, { 0, 1,  0}, { 2, 0,  0}, { 0, 1,  0}, {-1, 31, 0}, { 3, 1,  0},  //  01011000 N
           {-1, 7,  0}, { 0, 1,  2}, { 1, 2,  0}, { 0, 2,  0}, { 1, 0,  0}, { 0, 0,  0}, {-1, 21, 0}, {-1, 30, 0},  //  01100000
           { 2, 1,  3}, {-1, 20, 0}, { 2, 2,  0}, {-1, 33, 0}, { 2, 0,  0}, {-1, 32, 0}, {-1, 31, 0}, { 3, 1,  0},  //  01101000 N
           {-1, -1, 0}, { 0, 1,  2}, { 1, 2,  1}, { 0, 2,  0}, { 1, 0,  0}, { 0, 0,  2}, {-1, 21, 0}, {-1, 30, 0},  //  01110000
           { 2, 1,  0}, {-1, 20, 0}, { 2, 2,  1}, {-1, 33, 0}, { 2, 0,  0}, {-1, 32, 0}, {-1, 31, 0}, { 3, 1,  0},  //  01111000 N
           {-1, 4,  0}, { 0, 1,  0}, { 1, 2,  0}, { 0, 2,  0}, { 1, 0,  0}, { 0, 0,  0}, {-1, 21, 0}, {-1, 30, 0},  //  10000000
           { 2, 1,  0}, {-1, 20, 0}, { 2, 2,  0}, {-1, 33, 0}, { 2, 0,  0}, {-1, 32, 0}, {-1, 31, 0}, { 3, 1,  0},  //  10001000 N
           {-1, 8,  0}, { 0, 1,  4}, { 1, 2,  0}, { 0, 2,  0}, { 1, 0,  0}, { 0, 0,  0}, {-1, 21, 0}, {-1, 30, 0},  //  10010000
           { 2, 1,  1}, {-1, 20, 0}, { 2, 2,  0}, {-1, 33, 0}, { 2, 0,  0}, {-1, 32, 0}, {-1, 31, 0}, { 3, 1,  0},  //  10011000 N
           {-1, 10, 0}, { 0, 1,  0}, { 1, 2,  4}, { 0, 2,  0}, { 1, 0,  2}, { 0, 0,  0}, {-1, 21, 0}, {-1, 30, 0},  //  10100000
           { 2, 1,  0}, {-1, 20, 0}, { 2, 2,  0}, {-1, 33, 0}, { 2, 0,  0}, {-1, 32, 0}, {-1, 31, 0}, { 3, 1,  0},  //  10101000 N
           {-1, -1, 0}, { 0, 1,  0}, { 1, 2,  8}, { 0, 2,  0}, { 1, 0,  2}, { 0, 0,  2}, {-1, 21, 0}, {-1, 30, 0},  //  10110000
           { 2, 1,  1}, {-1, 20, 0}, { 2, 2,  1}, {-1, 33, 0}, { 2, 0,  0}, {-1, 32, 0}, {-1, 31, 0}, { 3, 1,  0},  //  10111000 N
           {-1, 6,  0}, { 0, 1,  0}, { 1, 2,  4}, { 0, 2,  4}, { 1, 0,  3}, { 0, 0,  0}, {-1, 21, 0}, {-1, 30, 0},  //  11000000
           { 2, 1,  3}, {-1, 20, 0}, { 2, 2,  0}, {-1, 33, 0}, { 2, 0,  3}, {-1, 32, 0}, {-1, 31, 0}, { 3, 1,  0},  //  11001000 N
           {-1, -1, 0}, { 0, 1,  4}, { 1, 2,  0}, { 0, 2,  0}, { 1, 0,  3}, { 0, 0,  0}, {-1, 21, 0}, {-1, 30, 0},  //  11010000
           { 2, 1,  0}, {-1, 20, 0}, { 2, 2,  0}, {-1, 33, 0}, { 2, 0,  3}, {-1, 32, 0}, {-1, 31, 0}, { 3, 1,  0},  //  11011000 N
           {-1, -1, 0}, { 0, 1,  0}, { 1, 2,  4}, { 0, 2,  0}, { 1, 0,  0}, { 0, 0,  0}, {-1, 21, 0}, {-1, 30, 0},  //  11100000
           { 2, 1,  3}, {-1, 20, 0}, { 2, 2,  0}, {-1, 33, 0}, { 2, 0,  3}, {-1, 32, 0}, {-1, 31, 0}, { 3, 1,  0},  //  11101000 N
           {-1, -1, 0}, { 0, 1, 10}, { 1, 2,  8}, { 0, 2,  4}, { 1, 0,  7}, { 0, 0,  0}, {-1, 21, 0}, {-1, 30, 0},  //  11110000
           { 2, 1,  0}, {-1, 20, 0}, { 2, 2,  0}, {-1, 33, 0}, { 2, 0,  0}, {-1, 32, 0}, {-1, 31, 0}, { 3, 1,  0},  // 100000000
        };

        private static readonly int[] SHADOW_MAP = { -1, 8, 9, 4, 11, 6, -1, 0, 10, -1, 5, 2, 7, 3, 1, -1, -1 };

        private IAssetManager _assetManager = default!;
        private IAssetInstance _assetShadow = default!;
        private IAssetInstance _assetShadowEdges = default!;
        private SpriteBatch _batchShadow = default!;
        private SpriteBatch _batchShadowEdges = default!;
        private ICoords _coords = default!;

        public Vector2i ScreenSize { get => _sizeInTiles * _coords.TileSize; }
        public int ShadowStrength { get; set; } = 70;

        private Quad[,] _innerQuads = new Quad[8, 6];
        private Quad[,] _cornerQuads = new Quad[4, 3];
        private Quad[] _edgeQuads = new Quad[17];

        private Vector2i _sizeInTiles;
        private ShadowTile[,] _tiles = new ShadowTile[0, 0];
        private UIBox2i _shadowBounds;

        public void Initialize(IAssetManager assetManager, ICoords coords)
        {
            _assetManager = assetManager;
            _coords = coords;

            _assetShadow = _assetManager.GetAsset(new("Elona.Shadow"));
            _assetShadowEdges = _assetManager.GetAsset(new("Elona.ShadowEdges"));

            _batchShadow = _assetShadow.MakeSpriteBatch(2048, SpriteBatchUsage.Dynamic);
            _batchShadowEdges = _assetShadowEdges.MakeSpriteBatch(2048, SpriteBatchUsage.Dynamic);

            var iw = _assetShadow.Width;
            var ih = _assetShadow.Height;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    _innerQuads[i, j] = Graphics.NewQuad(i * 24, j * 24, 24, 24, iw, ih);
                }
            }
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    _cornerQuads[i, j] = Graphics.NewQuad(i * 48, j * 48, 48, 48, iw, ih);
                }
            }

            iw = _assetShadowEdges.Width;
            ih = _assetShadowEdges.Height;

            for (int i = 0; i < 17; i++)
            {
                _edgeQuads[i] = Graphics.NewQuad(i * 48, 0, 48, 48, iw, ih);
            }
        }

        public void OnThemeSwitched()
        {
            _assetShadow = _assetManager.GetAsset(new("Elona.Shadow"));
            _assetShadowEdges = _assetManager.GetAsset(new("Elona.ShadowEdges"));

            _batchShadow = _assetShadow.MakeSpriteBatch(2048, SpriteBatchUsage.Dynamic);
            _batchShadowEdges = _assetShadowEdges.MakeSpriteBatch(2048, SpriteBatchUsage.Dynamic);
        }

        public void SetMapSize(Vector2i sizeInTiles)
        {
            _sizeInTiles = sizeInTiles;
            _tiles = new ShadowTile[_sizeInTiles.X, _sizeInTiles.Y];
        }

        public void SetTileShadow(int x, int y, ShadowTile shadow)
        {
            _tiles[x, y] = shadow;
        }

        public void SetAllTileShadows(ShadowTile[,] tiles, UIBox2i shadowBounds)
        {
            if (tiles.Length != _tiles.Length)
                throw new Exception($"Invalid tile array size ({tiles.Length} != {_tiles.Length})");

            _tiles = tiles;
            _shadowBounds = shadowBounds;
        }

        public void UpdateBatches()
        {
            _batchShadow.Clear();
            _batchShadowEdges.Clear();

            for (int x = 0; x < _sizeInTiles.X; x++)
            {
                for (int y = 0; y < _sizeInTiles.Y; y++)
                {
                    UpdateTileShadow(new Vector2i(x, y), _tiles[x, y]);
                }
            }

            _batchShadow.Flush();
            _batchShadowEdges.Flush();
        }

        private void UpdateTileShadow(Vector2i tilePos, ShadowTile shadow)
        {
            var screenPos = _coords.TileToScreen(tilePos);

            if (shadow == ShadowTile.None)
                return;

            var isShadow = (shadow & ShadowTile.IsShadow) == ShadowTile.IsShadow;
            if (!isShadow)
            {
                // Tile is lighted.Draw the fancy quarter-size shadow corners
                // depending on the directions that border a shadow.
                AddDeco(screenPos, shadow);
                return;
            }

            // Extract the cardinal part (NSEW).
            var cardinal = shadow & ShadowTile.Cardinal;
            int tile = -1;

            if (cardinal == ShadowTile.Cardinal)
            {
                // All four cardinal directions border a shadow. Check the
                // corner directions.

                var intercardinal = shadow & ShadowTile.Intercardinal;

                switch ((uint)intercardinal)
                {
                    case 0b01110000: //    SW SE NW
                        tile = 12;
                        break;
                    case 0b11010000: // NE SW    NW
                        tile = 13;
                        break;
                    case 0b10110000: // NE    SE NW
                        tile = 14;
                        break;
                    case 0b11100000: // NE SW SE
                        tile = 15;
                        break;
                    case 0b11000000: // NE SW
                        tile = 16;
                        break;
                    case 0b00110000: //       SE NW
                        tile = 16;
                        break;
                }
            }
            else
            {
                tile = SHADOW_MAP[(int)cardinal];
            }

            if (tile == -1)
            {
                _batchShadow.Add(_cornerQuads[3, 2], screenPos.X, screenPos.Y);
            }
            else
            {
                _batchShadowEdges.Add(_edgeQuads[tile], screenPos.X, screenPos.Y);
            }
        }

        private void AddDeco(Vector2i screenPos, ShadowTile shadow)
        {
            var deco0 = DECO[(int)shadow, 0];
            var deco1 = DECO[(int)shadow, 1];

            if (deco0 == -1)
            {
                AddOneDeco(screenPos, deco1);
            }
            else
            {
                // deco0, deco1 is x, y index into shadow image by size 48
                _batchShadow.Add(_cornerQuads[deco0, deco1], screenPos.X, screenPos.Y);
            }

            var deco2 = DECO[(int)shadow, 2];

            if (deco2 != 0)
            {
                AddOneDeco(screenPos, deco2);
            }
        }

        private void AddOneDeco(Vector2i screenPos, int deco)
        {
            var (x, y) = screenPos;

            switch (deco)
            {
                case 1: // upper-left inner
                    _batchShadow.Add(_innerQuads[7, 1], x, y);
                    break;
                case 2:
                    // lower-right inner
                    _batchShadow.Add(_innerQuads[6, 0], x + 24, y + 24);
                    break;
                case 3:
                    // lower-left inner
                    _batchShadow.Add(_innerQuads[7, 0], x, y + 24);
                    break;
                case 4:
                    // upper-right inner
                    _batchShadow.Add(_innerQuads[6, 1], x + 24, y);
                    break;
                case 5:
                    // upper-left inner
                    // lower-right inner
                    _batchShadow.Add(_innerQuads[6, 0], x + 24, y + 24);
                    _batchShadow.Add(_innerQuads[7, 1], x, y);
                    break;
                case 6:
                    // upper-right inner
                    // lower-left inner
                    _batchShadow.Add(_innerQuads[7, 0], x, y + 24);
                    _batchShadow.Add(_innerQuads[6, 1], x + 24, y);
                    break;
                case 7:
                    // lower-right inner
                    // lower-left inner
                    _batchShadow.Add(_innerQuads[7, 0], x, y + 24);
                    _batchShadow.Add(_innerQuads[6, 0], x + 24, y + 24);
                    break;
                case 8:
                    // upper-right inner
                    // upper-left inner
                    _batchShadow.Add(_innerQuads[7, 1], x, y);
                    _batchShadow.Add(_innerQuads[6, 1], x + 24, y);
                    break;
                case 9:
                    // upper-left inner
                    // lower-left inner
                    _batchShadow.Add(_innerQuads[7, 1], x, y);
                    _batchShadow.Add(_innerQuads[7, 0], x, y + 24);
                    break;
                case 10:
                    // upper-right inner
                    // lower-right inner
                    _batchShadow.Add(_innerQuads[6, 1], x + 24, y);
                    _batchShadow.Add(_innerQuads[6, 0], x + 24, y + 24);
                    break;
                case 20:
                    // left border
                    // right border
                    _batchShadow.Add(_innerQuads[0, 2], x, y);
                    _batchShadow.Add(_innerQuads[0, 3], x, y + 24);
                    _batchShadow.Add(_innerQuads[5, 2], x + 24, y);
                    _batchShadow.Add(_innerQuads[5, 3], x + 24, y + 24);
                    break;
                case 21:
                    // top border
                    // bottom border
                    _batchShadow.Add(_innerQuads[2, 0], x, y);
                    _batchShadow.Add(_innerQuads[3, 0], x + 24, y);
                    _batchShadow.Add(_innerQuads[2, 5], x, y + 24);
                    _batchShadow.Add(_innerQuads[3, 5], x + 24, y + 24);
                    break;
                case 30:
                    // right outer dart
                    _batchShadow.Add(_innerQuads[0, 0], x, y);
                    _batchShadow.Add(_innerQuads[1, 0], x + 24, y);
                    _batchShadow.Add(_innerQuads[0, 5], x, y + 24);
                    _batchShadow.Add(_innerQuads[1, 5], x + 24, y + 24);
                    break;
                case 31:
                    // left outer dart
                    _batchShadow.Add(_innerQuads[4, 0], x, y);
                    _batchShadow.Add(_innerQuads[5, 0], x + 24, y);
                    _batchShadow.Add(_innerQuads[4, 5], x, y + 24);
                    _batchShadow.Add(_innerQuads[5, 5], x + 24, y + 24);
                    break;
                case 32:
                    // upper outer dart
                    _batchShadow.Add(_innerQuads[0, 0], x, y);
                    _batchShadow.Add(_innerQuads[0, 1], x, y + 24);
                    _batchShadow.Add(_innerQuads[5, 0], x + 24, y);
                    _batchShadow.Add(_innerQuads[5, 1], x + 24, y + 24);
                    break;
                case 33:
                    // lower outer dart
                    _batchShadow.Add(_innerQuads[0, 4], x, y);
                    _batchShadow.Add(_innerQuads[0, 5], x, y + 24);
                    _batchShadow.Add(_innerQuads[5, 4], x + 24, y);
                    _batchShadow.Add(_innerQuads[5, 5], x + 24, y + 24);
                    break;
            }
        }

        public void RefreshTiles()
        {
        }

        public override void Update(float dt)
        {
        }

        public override void Draw()
        {
            Graphics.SetBlendMode(BlendMode.Subtract);
            GraphicsEx.SetColor(255, 255, 255, ShadowStrength);

            Graphics.SetScissor(X + _shadowBounds.Left, Y + _shadowBounds.Top, _shadowBounds.Width, _shadowBounds.Height);
            Graphics.Draw(_batchShadow, X, Y);
            Graphics.Draw(_batchShadowEdges, X, Y);
            Graphics.SetScissor();

            GraphicsEx.SetColor(255, 255, 255, (int)(ShadowStrength * ((256f - 9f) / 256f)));

            //// Left
            //Love.Graphics.Rectangle(Love.DrawMode.Fill, X, Y, ShadowBounds.Top, ScreenSize.Y);
            //// Right
            //Love.Graphics.Rectangle(Love.DrawMode.Fill, X + ShadowBounds.Left, Y, ShadowBounds.Width, ScreenSize.Y);
            //// Up
            //Love.Graphics.Rectangle(Love.DrawMode.Fill, X + ShadowBounds.Left, Y, ShadowBounds.Width, ShadowBounds.Top);
            //// Down
            //Love.Graphics.Rectangle(Love.DrawMode.Fill, X + ShadowBounds.Left, Y + ShadowBounds.Bottom, ShadowBounds.Width, ShadowBounds.Height);

            Graphics.SetBlendMode(BlendMode.Alpha);
        }
    }
}
