using OpenNefia.Core.Data;
using OpenNefia.Core.Util;
using OpenNefia.Mod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    public class TileAtlasFactory : IDisposable
    {
        internal RectanglePacker Binpack { get; }
        public int TileWidth { get; }
        public int TileHeight { get; }

        private List<TileSpec> TileSpecs;
        private Dictionary<string, AtlasTile> AtlasTiles;
        private Love.Canvas WorkCanvas;
        private ImageFilter Filter;

        public delegate void LoadTileDelegate(Love.Image image, Love.Quad quad, int rectX, int rectY);
        private LoadTileDelegate? OnLoadTile;

        public TileAtlasFactory(int tileWidth = OrthographicCoords.TILE_SIZE, int tileHeight = OrthographicCoords.TILE_SIZE, int tileCountX = 48, int tileCountY = 48)
        {
            TileWidth = tileWidth;
            TileHeight = tileHeight;

            var imageWidth = tileCountX * tileWidth;
            var imageHeight = tileCountY * tileHeight;
            Binpack = new RectanglePacker(imageWidth, imageHeight);
            WorkCanvas = Love.Graphics.NewCanvas(imageWidth, imageHeight);
            Filter = new ImageFilter(Love.FilterMode.Linear, Love.FilterMode.Linear, 1);
            TileSpecs = new List<TileSpec>();
            AtlasTiles = new Dictionary<string, AtlasTile>();
            OnLoadTile = null;
        }

        public TileAtlasFactory WithLoadTileCallback(LoadTileDelegate? callback)
        {
            this.OnLoadTile = callback;
            return this;
        }

        private Tuple<Love.Image, Love.Quad> LoadImageAndQuad(TileSpec tile)
        {
            if (tile.ImageRegion != null)
            {
                var image = ImageLoader.NewImage(tile.ImageRegion.SourceImagePath.Resolve());
                var quad = Love.Graphics.NewQuad(tile.ImageRegion.X, tile.ImageRegion.Y, tile.ImageRegion.Width, tile.ImageRegion.Height, image.GetWidth(), image.GetHeight());
                return Tuple.Create(image, quad);
            }
            else if (tile.ImagePath != null)
            {
                var image = ImageLoader.NewImage(tile.ImagePath.Resolve());
                var quad = Love.Graphics.NewQuad(0, 0, image.GetWidth(), image.GetHeight(), image.GetWidth(), image.GetHeight());
                return Tuple.Create(image, quad);
            }
            else
            {
                throw new Exception("Invalid tile spec");
            }
        }

        public void LoadTile(TileSpec tile)
        {
            var (image, quad) = LoadImageAndQuad(tile);

            var quadSize = quad.GetViewport();

            if (!this.Binpack.Pack((int)quadSize.Width, (int)quadSize.Height, out int rectX, out int rectY))
            {
                throw new Exception($"Ran out of space while packing tile atlas ({tile.TileIndex})");
            }

            if (this.OnLoadTile != null)
            {
                this.OnLoadTile(image, quad, rectX, rectY);
            }
            else
            {
                Love.Graphics.Draw(quad, image, rectX, rectY);
            }

            var innerQuad = Love.Graphics.NewQuad(rectX, rectY, quadSize.Width, quadSize.Height, this.WorkCanvas.GetWidth(), this.WorkCanvas.GetHeight());

            var isTall = quadSize.Height == quadSize.Width * 2;
            var yOffset = 0;
            if (isTall)
                yOffset = -this.TileHeight;

            var atlasTile = new AtlasTile(innerQuad, yOffset);

            // Special case for wall tiles
            if (tile.HasOverhang)
                atlasTile.HasOverhang = true;

            this.AtlasTiles.Add(tile.TileIndex, atlasTile);

            quad.Dispose();
        }

        public TileAtlasFactory LoadTiles(IEnumerable<TileSpec> tiles)
        {
            TileSpecs.AddRange(tiles);

            return this;
        }

        private static string GetHash(HashAlgorithm hashAlgorithm, string input)
        {
            // Convert the input string to a byte array and compute the hash.
            byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            var sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        public TileAtlas Build()
        {
            var path = new ModLocalPath(typeof(CoreMod), "Cache/TileAtlas");
            var dir = path.Resolve();
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string hashString;
            using (var sha256Hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256))
            {
                foreach (var tile in TileSpecs.OrderBy(x => x.TileIndex))
                {
                    sha256Hash.AppendData(Encoding.UTF8.GetBytes(tile.TileIndex));

                    if (tile.ImagePath != null)
                    {
                        sha256Hash.AppendData(Encoding.UTF8.GetBytes(tile.ImagePath.Resolve()));
                    }
                    else if (tile.ImageRegion != null)
                    {
                        sha256Hash.AppendData(Encoding.UTF8.GetBytes(tile.ImageRegion.SourceImagePath.Resolve()));
                    }
                }

                var data = sha256Hash.GetCurrentHash();

                var sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                hashString = sBuilder.ToString();
            }

            var serializedFilepath = path.Join($"{hashString}.nbt").Resolve();

            if (File.Exists(serializedFilepath))
            {
                Logger.Info($"Cache hit! {hashString}");

                var atlas = new TileAtlas(null!, null!, null!);
                return SerializationUtils.Deserialize(serializedFilepath, atlas, nameof(TileAtlas));
            }

            var canvas = Love.Graphics.GetCanvas();

            Love.Graphics.SetCanvas(this.WorkCanvas);
            Love.Graphics.SetBlendMode(Love.BlendMode.Alpha);
            Love.Graphics.SetColor(Love.Color.White);
            GraphicsEx.SetDefaultFilter(this.Filter);

            foreach (var tile in TileSpecs)
            {
                LoadTile(tile);
            }

            Love.Graphics.SetCanvas(canvas);
            Love.Graphics.SetDefaultFilter(Love.FilterMode.Linear, Love.FilterMode.Linear, 1);

            var imageData = this.WorkCanvas.NewImageData();
            var imageFilepath = path.Join($"{hashString}.png").Resolve();
            var fileData = imageData.Encode(Love.ImageFormat.PNG);
            File.WriteAllBytes(imageFilepath, fileData.GetBytes());

            var image = Love.Graphics.NewImage(imageData);

            var tileAtlas = new TileAtlas(image, imageFilepath, this.AtlasTiles);

            SerializationUtils.Serialize(serializedFilepath, tileAtlas, nameof(TileAtlas));

            return tileAtlas;
        }

        public void Dispose()
        {
            this.WorkCanvas.Dispose();
        }
    }
}
