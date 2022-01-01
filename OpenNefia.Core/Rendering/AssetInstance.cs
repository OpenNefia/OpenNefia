using Love;
using OpenNefia.Core.Maths;

namespace OpenNefia.Core.Rendering
{
    /// <summary>
    /// Represents one copy of a loaded asset's graphics data, which
    /// can be reused with multiple <see cref="IAssetDrawable"/>s.
    /// </summary>
    public interface IAssetInstance : IDisposable
    {
        int Width { get; }
        int Height { get; }
        Vector2i Size { get; }

        AssetPrototype Asset { get; }
        uint CountX { get; }
        uint CountY { get; }

        Love.SpriteBatch MakeBatch(List<AssetInstance.AssetBatchPart> parts, int maxSprites = 2048);
        Love.SpriteBatch MakeSpriteBatch(int count, Love.SpriteBatchUsage usage);

        void Draw(float x, float y, float width = 0, float height = 0, bool centered = false, float rotation = 0);
        void Draw(Love.Quad quad, float x, float y, float width = 0, float height = 0, bool centered = false, float rotation = 0);
        void DrawRegion(string regionId, float x = 0, float y = 0, float width = 0, float height = 0, bool centered = false, float rotation = 0);
    }

    public class AssetInstance : IAssetInstance
    {
        /// <summary>
        /// Data to submit to <see cref="MakeBatch(List{AssetBatchPart}, int, SpriteBatchUsage)"/>
        /// </summary>
        public class AssetBatchPart
        {
            public string RegionId { get; set; } = string.Empty;
            public int X { get; set; } = 0;
            public int Y { get; set; } = 0;

            public AssetBatchPart(string id, int x, int y)
            {
                RegionId = id;
                X = x;
                Y = y;
            }

            public override string ToString()
            {
                return $"{RegionId}: ({X}, {Y})";
            }
        }

        public AssetPrototype Asset { get; }
        private Love.Image Image { get; }

        private Dictionary<string, Love.Quad> Quads;
        private AssetRegions Regions;

        public uint CountX { get; }
        public uint CountY { get; }

        public int Width { get => this.Image.GetWidth(); }
        public int Height { get => this.Image.GetHeight(); }
        public Vector2i Size => new(Width, Height);

        public AssetInstance(AssetPrototype asset, Love.Image image, AssetRegions regions)
        {
            this.Asset = asset;
            this.Image = image;
            this.Quads = new Dictionary<string, Quad>();
            this.CountX = this.Asset.CountX;
            this.CountY = this.Asset.CountY;
            this.Regions = regions;

            this.SetupQuads();
        }

        private void SetupQuads()
        {
            var countX = this.CountX;
            var countY = this.CountY;

            var imageWidth = this.Image.GetWidth();
            var imageHeight = this.Image.GetHeight();

            if (countX > 1 || countY > 1)
            {
                var width = imageWidth / countX;
                var height = imageHeight / countY;

                uint quadId = 0;
                for (int j = 0; j < countY; j++)
                {
                    for (int i = 0; i < countX; i++)
                    {
                        this.Quads[quadId.ToString()] = Love.Graphics.NewQuad(width * i, height * j, width, height, imageWidth, imageHeight);
                        quadId++;
                    }
                }
            }
            else
            {
                this.Quads["0"] = Love.Graphics.NewQuad(0, 0, imageWidth, imageHeight, imageWidth, imageHeight);
            }

            foreach (var pair in this.Regions)
            {
                var key = pair.Key;
                var region = pair.Value;
                this.Quads[key] = Love.Graphics.NewQuad(region.Left, region.Top, region.Width, region.Height, imageWidth, imageHeight);
            }
        }

        public Love.SpriteBatch MakeBatch(List<AssetBatchPart> parts, int maxSprites = 2048)
        {
            var batch = Love.Graphics.NewSpriteBatch(this.Image, maxSprites, Love.SpriteBatchUsage.Static);
            batch.Clear();

            foreach (var part in parts)
            {
                batch.Add(this.Quads[part.RegionId], part.X, part.Y);
            }

            batch.Flush();

            return batch;
        }

        public SpriteBatch MakeSpriteBatch(int count, SpriteBatchUsage usage)
        {
            return Love.Graphics.NewSpriteBatch(Image, count, usage);
        }

        public void Draw(float x, float y, float width = 0, float height = 0, bool centered = false, float rotation = 0)
        {
            GraphicsEx.DrawImage(this.Image, x, y, width, height, centered, rotation);
        }

        public void Draw(Love.Quad quad, float x, float y, float width = 0, float height = 0, bool centered = false, float rotation = 0)
        {
            GraphicsEx.DrawImage(quad, this.Image, x, y, width, height, centered, rotation);
        }

        public void DrawRegion(string regionId, float x, float y, float width = 0, float height = 0, bool centered = false, float rotation = 0)
        {
            var quad = this.Quads[regionId];
            if (quad == null)
            {
                throw new ArgumentException($"Invalid region ID {regionId}");
            }

            GraphicsEx.DrawImageRegion(this.Image, quad, x, y, width, height, centered, rotation);
        }

        public void Dispose()
        {
            foreach (var quad in this.Quads.Values)
            {
                quad.Dispose();
            }
            this.Quads.Clear();
        }
    }
}
