﻿using OpenNefia.Core.Maths;

namespace OpenNefia.Core.Rendering
{
    /// <summary>
    /// Represents one copy of a loaded asset's graphics data, which
    /// can be reused with multiple <see cref="IAssetDrawable"/>s.
    /// </summary>
    public interface IAssetInstance : IDisposable
    {
        int PixelWidth { get; }
        int PixelHeight { get; }
        Vector2i PixelSize { get; }

        Vector2 VirtualSize(float uiScale);

        AssetPrototype Asset { get; }
        IReadOnlyDictionary<string, UIBox2i> Regions { get; }
        uint CountX { get; }
        uint CountY { get; }

        Love.SpriteBatch MakeBatch(List<AssetInstance.AssetBatchPart> parts, int maxSprites = 2048);
        Love.SpriteBatch MakeSpriteBatch(int count, Love.SpriteBatchUsage usage);

        void Draw(float x, float y, float width = 0, float height = 0, bool centered = false, float rotation = 0);
        void Draw(Love.Quad quad, float x, float y, float width = 0, float height = 0, bool centered = false, float rotation = 0);
        void DrawRegion(string regionId, float x = 0, float y = 0, float width = 0, float height = 0, bool centered = false, float rotation = 0);
        void DrawS(float uiScale, float vx, float vy, float vwidth = 0, float vheight = 0, bool centered = false, float rotation = 0);
        void DrawS(float uiScale, Love.Quad quad, float vx, float vy, float vwidth = 0, float vheight = 0, bool centered = false, float rotation = 0);
        void DrawRegionS(float uiScale, string regionId, float vx = 0, float vy = 0, float vwidth = 0, float vheight = 0, bool centered = false, float rotation = 0);
        float VirtualWidth(float uiScale);
        float VirtualHeight(float uiScale);
    }
    
    public class AssetInstance : IAssetInstance
    {
        /// <summary>
        /// Data to submit to <see cref="MakeBatch(List{AssetBatchPart}, int, SpriteBatchUsage)"/>
        /// </summary>
        public class AssetBatchPart
        {
            public string RegionId { get; set; } = string.Empty;

            /// <summary>
            /// Position of the part, in physical pixels.
            /// </summary>
            public Vector2i PixelPosition { get; set; }

            /// <summary>
            /// X position of the part, in physical pixels.
            /// </summary>
            public int PixelX => PixelPosition.X;

            /// <summary>
            /// Y position of the part, in physical pixels.
            /// </summary>
            public int PixelY => PixelPosition.Y;

            public AssetBatchPart(string id, int x, int y)
            {
                RegionId = id;
                PixelPosition = new(x, y);
            }

            public override string ToString()
            {
                return $"{RegionId}: ({PixelX}, {PixelY})";
            }
        }

        public AssetPrototype Asset { get; }
        private Love.Image Image { get; }

        private Dictionary<string, Love.Quad> Quads;
        private AssetRegions _regions;

        public IReadOnlyDictionary<string, UIBox2i> Regions => _regions;

        public uint CountX { get; }
        public uint CountY { get; }

        public int PixelWidth { get => Image.GetWidth(); }
        public int PixelHeight { get => Image.GetHeight(); }
        public Vector2i PixelSize => new(PixelWidth, PixelHeight);

        public Vector2 VirtualSize(float uiScale) => (Vector2)PixelSize / uiScale;
        public float VirtualWidth(float uiScale) => PixelWidth / uiScale;
        public float VirtualHeight(float uiScale) => PixelWidth / uiScale;

        public AssetInstance(AssetPrototype asset, Love.Image image, AssetRegions regions)
        {
            Asset = asset;
            Image = image;
            Quads = new Dictionary<string, Love.Quad>();
            CountX = Asset.CountX;
            CountY = Asset.CountY;
            _regions = regions;

            SetupQuads();
        }

        private void SetupQuads()
        {
            var countX = CountX;
            var countY = CountY;

            var imageWidth = Image.GetWidth();
            var imageHeight = Image.GetHeight();

            if (countX > 1 || countY > 1)
            {
                var width = imageWidth / countX;
                var height = imageHeight / countY;

                uint quadId = 0;
                for (int j = 0; j < countY; j++)
                {
                    for (int i = 0; i < countX; i++)
                    {
                        Quads[quadId.ToString()] = Love.Graphics.NewQuad(width * i, height * j, width, height, imageWidth, imageHeight);
                        quadId++;
                    }
                }
            }
            else
            {
                Quads["0"] = Love.Graphics.NewQuad(0, 0, imageWidth, imageHeight, imageWidth, imageHeight);
            }

            foreach (var pair in _regions)
            {
                var key = pair.Key;
                var region = pair.Value;
                Quads[key] = Love.Graphics.NewQuad(region.Left, region.Top, region.Width, region.Height, imageWidth, imageHeight);
            }
        }

        public Love.SpriteBatch MakeBatch(List<AssetBatchPart> parts, int maxSprites = 2048)
        {
            var batch = Love.Graphics.NewSpriteBatch(Image, maxSprites, Love.SpriteBatchUsage.Static);
            batch.Clear();

            foreach (var part in parts)
            {
                batch.Add(Quads[part.RegionId], part.PixelX, part.PixelY);
            }

            batch.Flush();

            return batch;
        }

        public Love.SpriteBatch MakeSpriteBatch(int count, Love.SpriteBatchUsage usage)
        {
            return Love.Graphics.NewSpriteBatch(Image, count, usage);
        }

        public void Draw(float x, float y, float width = 0, float height = 0, bool centered = false, float rotation = 0)
        {
            GraphicsEx.DrawImage(Image, x, y, width, height, centered, rotation);
        }

        public void Draw(Love.Quad quad, float x, float y, float width = 0, float height = 0, bool centered = false, float rotation = 0)
        {
            GraphicsEx.DrawImage(quad, Image, x, y, width, height, centered, rotation);
        }

        public void DrawRegion(string regionId, float x, float y, float width = 0, float height = 0, bool centered = false, float rotation = 0)
        {
            var quad = Quads[regionId];
            if (quad == null)
            {
                throw new ArgumentException($"Invalid region ID {regionId}");
            }

            GraphicsEx.DrawImageRegion(Image, quad, x, y, width, height, centered, rotation);
        }

        public void DrawS(float uiScale, float vx, float vy, float vwidth = 0, float vheight = 0, bool centered = false, float rotation = 0)
        {
            Draw(vx * uiScale, vy * uiScale, vwidth * uiScale, vheight * uiScale, centered, rotation);
        }

        public void DrawS(float uiScale, Love.Quad quad, float vx, float vy, float vwidth = 0, float vheight = 0, bool centered = false, float rotation = 0)
        {
            Draw(quad, vx * uiScale, vy * uiScale, vwidth * uiScale, vheight * uiScale, centered, rotation);
        }

        public void DrawRegionS(float uiScale, string regionId, float vx, float vy, float vwidth = 0, float vheight = 0, bool centered = false, float rotation = 0)
        {
            DrawRegion(regionId, vx * uiScale, vy * uiScale, vwidth * uiScale, vheight * uiScale, centered, rotation);
        }

        public void Dispose()
        {
            foreach (var quad in Quads.Values)
            {
                quad.Dispose();
            }
            Quads.Clear();
        }
    }
}
