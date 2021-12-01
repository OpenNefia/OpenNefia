using Love;
using OpenNefia.Core.Data.Types;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
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

        [Dependency] private readonly IAssetManager _assetManager;

        private Vector2i SizeInTiles;
        private ICoords Coords;

        public Vector2i ScreenSize { get => SizeInTiles * Coords.TileSize; }

        private IAssetDrawable AssetShadow;
        private IAssetDrawable AssetShadowEdges;

        private SpriteBatch BatchShadow;
        private SpriteBatch BatchShadowEdges;

        private Love.Quad[,] InnerQuads;
        private Love.Quad[,] CornerQuads;
        private Love.Quad[] EdgeQuads;

        public int ShadowStrength { get; set; }

        private ShadowTile[,] Tiles;
        private UIBox2i ShadowBounds;

        public ShadowBatch(Vector2i sizeInTiles, ICoords coords, IAssetManager assetManager)
        {
            _assetManager = assetManager;

            SizeInTiles = sizeInTiles;
            Coords = coords;

            ShadowStrength = 70;

            AssetShadow = _assetManager.GetAsset(new("Shadow"));
            AssetShadowEdges = _assetManager.GetAsset(new("ShadowEdges"));

            BatchShadow = AssetShadow.MakeSpriteBatch(2048, SpriteBatchUsage.Dynamic);
            BatchShadowEdges = AssetShadowEdges.MakeSpriteBatch(2048, SpriteBatchUsage.Dynamic);

            InnerQuads = new Love.Quad[8, 6];
            CornerQuads = new Love.Quad[4, 3];
            EdgeQuads = new Love.Quad[17];

            Tiles = new ShadowTile[SizeInTiles.X, SizeInTiles.Y];

            var iw = AssetShadow.Width;
            var ih = AssetShadow.Height;

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    InnerQuads[i, j] = Love.Graphics.NewQuad(i * 24, j * 24, 24, 24, iw, ih);
                }
            }
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    CornerQuads[i, j] = Love.Graphics.NewQuad(i * 48, j * 48, 48, 48, iw, ih);
                }
            }

            iw = AssetShadowEdges.Width;
            ih = AssetShadowEdges.Height;

            for (int i = 0; i < 17; i++)
            {
                EdgeQuads[i] = Love.Graphics.NewQuad(i * 48, 0, 48, 48, iw, ih);
            }
        }

        public void SetTileShadow(int x, int y, ShadowTile shadow)
        {
            Tiles[x, y] = shadow;
        }

        public void SetAllTileShadows(ShadowTile[,] tiles, UIBox2i shadowBounds)
        {
            if (tiles.Length != Tiles.Length)
                throw new Exception($"Invalid tile array size ({tiles.Length} != {Tiles.Length})");

            Tiles = tiles;
            ShadowBounds = shadowBounds;
        }

        public void UpdateBatches()
        {
            BatchShadow.Clear();
            BatchShadowEdges.Clear();

            for (int x = 0; x < SizeInTiles.X; x++)
            {
                for (int y = 0; y < SizeInTiles.Y; y++)
                {
                    UpdateTileShadow(new Vector2i(x, y), Tiles[x, y]);
                }
            }

            BatchShadow.Flush();
            BatchShadowEdges.Flush();
        }

        private void UpdateTileShadow(Vector2i tilePos, ShadowTile shadow)
        {
            Coords.TileToScreen(tilePos, out var screenPos);

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
                BatchShadow.Add(CornerQuads[3, 2], screenPos.X, screenPos.Y);
            }
            else
            {
                BatchShadowEdges.Add(EdgeQuads[tile], screenPos.X, screenPos.Y);
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
                BatchShadow.Add(CornerQuads[deco0, deco1], screenPos.X, screenPos.Y);
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
                    BatchShadow.Add(InnerQuads[7, 1], x, y);
                    break;
                case 2:
                    // lower-right inner
                    BatchShadow.Add(InnerQuads[6, 0], x + 24, y + 24);
                    break;
                case 3:
                    // lower-left inner
                    BatchShadow.Add(InnerQuads[7, 0], x, y + 24);
                    break;
                case 4:
                    // upper-right inner
                    BatchShadow.Add(InnerQuads[6, 1], x + 24, y);
                    break;
                case 5:
                    // upper-left inner
                    // lower-right inner
                    BatchShadow.Add(InnerQuads[6, 0], x + 24, y + 24);
                    BatchShadow.Add(InnerQuads[7, 1], x, y);
                    break;
                case 6:
                    // upper-right inner
                    // lower-left inner
                    BatchShadow.Add(InnerQuads[7, 0], x, y + 24);
                    BatchShadow.Add(InnerQuads[6, 1], x + 24, y);
                    break;
                case 7:
                    // lower-right inner
                    // lower-left inner
                    BatchShadow.Add(InnerQuads[7, 0], x, y + 24);
                    BatchShadow.Add(InnerQuads[6, 0], x + 24, y + 24);
                    break;
                case 8:
                    // upper-right inner
                    // upper-left inner
                    BatchShadow.Add(InnerQuads[7, 1], x, y);
                    BatchShadow.Add(InnerQuads[6, 1], x + 24, y);
                    break;
                case 9:
                    // upper-left inner
                    // lower-left inner
                    BatchShadow.Add(InnerQuads[7, 1], x, y);
                    BatchShadow.Add(InnerQuads[7, 0], x, y + 24);
                    break;
                case 10:
                    // upper-right inner
                    // lower-right inner
                    BatchShadow.Add(InnerQuads[6, 1], x + 24, y);
                    BatchShadow.Add(InnerQuads[6, 0], x + 24, y + 24);
                    break;
                case 20:
                    // left border
                    // right border
                    BatchShadow.Add(InnerQuads[0, 2], x, y);
                    BatchShadow.Add(InnerQuads[0, 3], x, y + 24);
                    BatchShadow.Add(InnerQuads[5, 2], x + 24, y);
                    BatchShadow.Add(InnerQuads[5, 3], x + 24, y + 24);
                    break;
                case 21:
                    // top border
                    // bottom border
                    BatchShadow.Add(InnerQuads[2, 0], x, y);
                    BatchShadow.Add(InnerQuads[3, 0], x + 24, y);
                    BatchShadow.Add(InnerQuads[2, 5], x, y + 24);
                    BatchShadow.Add(InnerQuads[3, 5], x + 24, y + 24);
                    break;
                case 30:
                    // right outer dart
                    BatchShadow.Add(InnerQuads[0, 0], x, y);
                    BatchShadow.Add(InnerQuads[1, 0], x + 24, y);
                    BatchShadow.Add(InnerQuads[0, 5], x, y + 24);
                    BatchShadow.Add(InnerQuads[1, 5], x + 24, y + 24);
                    break;
                case 31:
                    // left outer dart
                    BatchShadow.Add(InnerQuads[4, 0], x, y);
                    BatchShadow.Add(InnerQuads[5, 0], x + 24, y);
                    BatchShadow.Add(InnerQuads[4, 5], x, y + 24);
                    BatchShadow.Add(InnerQuads[5, 5], x + 24, y + 24);
                    break;
                case 32:
                    // upper outer dart
                    BatchShadow.Add(InnerQuads[0, 0], x, y);
                    BatchShadow.Add(InnerQuads[0, 1], x, y + 24);
                    BatchShadow.Add(InnerQuads[5, 0], x + 24, y);
                    BatchShadow.Add(InnerQuads[5, 1], x + 24, y + 24);
                    break;
                case 33:
                    // lower outer dart
                    BatchShadow.Add(InnerQuads[0, 4], x, y);
                    BatchShadow.Add(InnerQuads[0, 5], x, y + 24);
                    BatchShadow.Add(InnerQuads[5, 4], x + 24, y);
                    BatchShadow.Add(InnerQuads[5, 5], x + 24, y + 24);
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
            Love.Graphics.SetBlendMode(BlendMode.Subtract);
            GraphicsEx.SetColor(255, 255, 255, ShadowStrength);

            Love.Graphics.SetScissor(X + ShadowBounds.Left, Y + ShadowBounds.Top, ShadowBounds.Width, ShadowBounds.Height);
            Love.Graphics.Draw(BatchShadow, X, Y);
            Love.Graphics.Draw(BatchShadowEdges, X, Y);
            Love.Graphics.SetScissor();

            GraphicsEx.SetColor(255, 255, 255, (int)(ShadowStrength * ((256f - 9f) / 256f)));

            //// Left
            //Love.Graphics.Rectangle(Love.DrawMode.Fill, X, Y, ShadowBounds.Top, ScreenSize.Y);
            //// Right
            //Love.Graphics.Rectangle(Love.DrawMode.Fill, X + ShadowBounds.Left, Y, ShadowBounds.Width, ScreenSize.Y);
            //// Up
            //Love.Graphics.Rectangle(Love.DrawMode.Fill, X + ShadowBounds.Left, Y, ShadowBounds.Width, ShadowBounds.Top);
            //// Down
            //Love.Graphics.Rectangle(Love.DrawMode.Fill, X + ShadowBounds.Left, Y + ShadowBounds.Bottom, ShadowBounds.Width, ShadowBounds.Height);

            Love.Graphics.SetBlendMode(BlendMode.Alpha);
        }
    }
}
