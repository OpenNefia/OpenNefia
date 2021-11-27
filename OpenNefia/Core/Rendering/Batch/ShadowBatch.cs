using Love;
using OpenNefia.Core.Data.Types;
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

        private int WidthInTiles;
        private int HeightInTiles;
        private ICoords Coords;

        public int WidthInScreen { get => WidthInTiles * Coords.TileWidth; }
        public int HeightInScreen { get => HeightInTiles * Coords.TileHeight; }

        private AssetDrawable AssetShadow;
        private AssetDrawable AssetShadowEdges;

        private SpriteBatch BatchShadow;
        private SpriteBatch BatchShadowEdges;

        private Love.Quad[,] InnerQuads;
        private Love.Quad[,] CornerQuads;
        private Love.Quad[] EdgeQuads;

        public int ShadowStrength { get; set; }

        private ShadowTile[] Tiles;
        private Rectangle ShadowBounds;

        public ShadowBatch(int width, int height, ICoords coords, IAssetManager assetManager)
        {
            this.WidthInTiles = width;
            this.HeightInTiles = height;
            this.Coords = coords;

            this.ShadowStrength = 70;

            this.AssetShadow = assetManager.GetAsset(new("Shadow"));
            this.AssetShadowEdges = new AssetDrawable(new("ShadowEdges"));

            this.BatchShadow = Love.Graphics.NewSpriteBatch(this.AssetShadow.Image, 2048, SpriteBatchUsage.Dynamic);
            this.BatchShadowEdges = Love.Graphics.NewSpriteBatch(this.AssetShadowEdges.Image, 2048, SpriteBatchUsage.Dynamic);

            InnerQuads = new Love.Quad[8, 6];
            CornerQuads = new Love.Quad[4, 3];
            EdgeQuads = new Love.Quad[17];

            Tiles = new ShadowTile[width * height];

            var iw = this.AssetShadow.Image.GetWidth();
            var ih = this.AssetShadow.Image.GetHeight();

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

            iw = this.AssetShadowEdges.Image.GetWidth();
            ih = this.AssetShadowEdges.Image.GetHeight();

            for (int i = 0; i < 17; i++)
            {
                EdgeQuads[i] = Love.Graphics.NewQuad(i * 48, 0, 48, 48, iw, ih);
            }
        }

        public void SetTileShadow(int x, int y, ShadowTile shadow)
        {
            this.Tiles[y * WidthInTiles + x] = shadow;
        }

        public void SetAllTileShadows(ShadowTile[] tiles, Rectangle shadowBounds)
        {
            if (tiles.Length != this.Tiles.Length)
                throw new Exception($"Invalid tile array size ({tiles.Length} != {this.Tiles.Length})");

            this.Tiles = tiles;
            this.ShadowBounds = shadowBounds;
        }

        public void UpdateBatches()
        {
            this.BatchShadow.Clear();
            this.BatchShadowEdges.Clear();

            for (int i = 0; i < WidthInTiles * HeightInTiles; i++)
            {
                var x = i % WidthInTiles;
                var y = i / WidthInTiles;
                this.UpdateTileShadow(x, y, Tiles[i]);
            }

            this.BatchShadow.Flush();
            this.BatchShadowEdges.Flush();
        }

        private void UpdateTileShadow(int tileX, int tileY, ShadowTile shadow)
        {
            Coords.TileToScreen(tileX, tileY, out var x, out var y);

            if (shadow == ShadowTile.None)
                return;

            var isShadow = (shadow & ShadowTile.IsShadow) == ShadowTile.IsShadow;
            if (!isShadow)
            {
                // Tile is lighted.Draw the fancy quarter-size shadow corners
                // depending on the directions that border a shadow.
                this.AddDeco(x, y, shadow);
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
                this.BatchShadow.Add(this.CornerQuads[3, 2], x, y);
            }
            else
            {
                this.BatchShadowEdges.Add(this.EdgeQuads[tile], x, y);
            }
        }

        private void AddDeco(int x, int y, ShadowTile shadow)
        {
            var deco0 = DECO[(int)shadow, 0];
            var deco1 = DECO[(int)shadow, 1];

            if (deco0 == -1)
            {
                this.AddOneDeco(x, y, deco1);
            }
            else
            {
                // deco0, deco1 is x, y index into shadow image by size 48
                this.BatchShadow.Add(this.CornerQuads[deco0, deco1], x, y);
            }

            var deco2 = DECO[(int)shadow, 2];

            if (deco2 != 0)
            {
                this.AddOneDeco(x, y, deco2);
            }
        }

        private void AddOneDeco(int x, int y, int deco)
        {
            switch (deco)
            {
                case 1: // upper-left inner
                    this.BatchShadow.Add(this.InnerQuads[7, 1], x, y);
                    break;
                case 2:
                    // lower-right inner
                    this.BatchShadow.Add(this.InnerQuads[6, 0], x + 24, y + 24);
                    break;
                case 3:
                    // lower-left inner
                    this.BatchShadow.Add(this.InnerQuads[7, 0], x, y + 24);
                    break;
                case 4:
                    // upper-right inner
                    this.BatchShadow.Add(this.InnerQuads[6, 1], x + 24, y);
                    break;
                case 5:
                    // upper-left inner
                    // lower-right inner
                    this.BatchShadow.Add(this.InnerQuads[6, 0], x + 24, y + 24);
                    this.BatchShadow.Add(this.InnerQuads[7, 1], x, y);
                    break;
                case 6:
                    // upper-right inner
                    // lower-left inner
                    this.BatchShadow.Add(this.InnerQuads[7, 0], x, y + 24);
                    this.BatchShadow.Add(this.InnerQuads[6, 1], x + 24, y);
                    break;
                case 7:
                    // lower-right inner
                    // lower-left inner
                    this.BatchShadow.Add(this.InnerQuads[7, 0], x, y + 24);
                    this.BatchShadow.Add(this.InnerQuads[6, 0], x + 24, y + 24);
                    break;
                case 8:
                    // upper-right inner
                    // upper-left inner
                    this.BatchShadow.Add(this.InnerQuads[7, 1], x, y);
                    this.BatchShadow.Add(this.InnerQuads[6, 1], x + 24, y);
                    break;
                case 9:
                    // upper-left inner
                    // lower-left inner
                    this.BatchShadow.Add(this.InnerQuads[7, 1], x, y);
                    this.BatchShadow.Add(this.InnerQuads[7, 0], x, y + 24);
                    break;
                case 10:
                    // upper-right inner
                    // lower-right inner
                    this.BatchShadow.Add(this.InnerQuads[6, 1], x + 24, y);
                    this.BatchShadow.Add(this.InnerQuads[6, 0], x + 24, y + 24);
                    break;
                case 20:
                    // left border
                    // right border
                    this.BatchShadow.Add(this.InnerQuads[0, 2], x, y);
                    this.BatchShadow.Add(this.InnerQuads[0, 3], x, y + 24);
                    this.BatchShadow.Add(this.InnerQuads[5, 2], x + 24, y);
                    this.BatchShadow.Add(this.InnerQuads[5, 3], x + 24, y + 24);
                    break;
                case 21:
                    // top border
                    // bottom border
                    this.BatchShadow.Add(this.InnerQuads[2, 0], x, y);
                    this.BatchShadow.Add(this.InnerQuads[3, 0], x + 24, y);
                    this.BatchShadow.Add(this.InnerQuads[2, 5], x, y + 24);
                    this.BatchShadow.Add(this.InnerQuads[3, 5], x + 24, y + 24);
                    break;
                case 30:
                    // right outer dart
                    this.BatchShadow.Add(this.InnerQuads[0, 0], x, y);
                    this.BatchShadow.Add(this.InnerQuads[1, 0], x + 24, y);
                    this.BatchShadow.Add(this.InnerQuads[0, 5], x, y + 24);
                    this.BatchShadow.Add(this.InnerQuads[1, 5], x + 24, y + 24);
                    break;
                case 31:
                    // left outer dart
                    this.BatchShadow.Add(this.InnerQuads[4, 0], x, y);
                    this.BatchShadow.Add(this.InnerQuads[5, 0], x + 24, y);
                    this.BatchShadow.Add(this.InnerQuads[4, 5], x, y + 24);
                    this.BatchShadow.Add(this.InnerQuads[5, 5], x + 24, y + 24);
                    break;
                case 32:
                    // upper outer dart
                    this.BatchShadow.Add(this.InnerQuads[0, 0], x, y);
                    this.BatchShadow.Add(this.InnerQuads[0, 1], x, y + 24);
                    this.BatchShadow.Add(this.InnerQuads[5, 0], x + 24, y);
                    this.BatchShadow.Add(this.InnerQuads[5, 1], x + 24, y + 24);
                    break;
                case 33:
                    // lower outer dart
                    this.BatchShadow.Add(this.InnerQuads[0, 4], x, y);
                    this.BatchShadow.Add(this.InnerQuads[0, 5], x, y + 24);
                    this.BatchShadow.Add(this.InnerQuads[5, 4], x + 24, y);
                    this.BatchShadow.Add(this.InnerQuads[5, 5], x + 24, y + 24);
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
            GraphicsEx.SetColor(255, 255, 255, this.ShadowStrength);

            GraphicsEx.SetScissor(X + ShadowBounds.X, Y + ShadowBounds.Y, ShadowBounds.Width, ShadowBounds.Height);
            Love.Graphics.Draw(this.BatchShadow, X, Y);
            Love.Graphics.Draw(this.BatchShadowEdges, X, Y);
            GraphicsEx.SetScissor();

            GraphicsEx.SetColor(255, 255, 255, (int)(this.ShadowStrength * ((256f - 9f) / 256f)));

            // Left
            GraphicsEx.Love.Graphics.Rectangle(Love.DrawMode.Fill, (X, Y, ShadowBounds.X, this.HeightInScreen);
            // Right
            GraphicsEx.Love.Graphics.Rectangle(Love.DrawMode.Fill, (X + ShadowBounds.X + ShadowBounds.Width, Y, ShadowBounds.Width, this.HeightInScreen);
            // Up
            GraphicsEx.Love.Graphics.Rectangle(Love.DrawMode.Fill, (X + ShadowBounds.X, Y, ShadowBounds.Width, ShadowBounds.Y);
            // Down
            GraphicsEx.Love.Graphics.Rectangle(Love.DrawMode.Fill, (X + ShadowBounds.X, Y + ShadowBounds.Y + ShadowBounds.Height, ShadowBounds.Width, ShadowBounds.Height);

            Love.Graphics.SetBlendMode(BlendMode.Alpha);
        }
    }
}
