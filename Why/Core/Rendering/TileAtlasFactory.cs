using OpenNefia.Core.ContentPack;
using OpenNefia.Core.Data;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Utility;
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
        private readonly IResourceCache _resourceCache = default!;

        internal RectanglePacker _binpack { get; }
        public int _tileWidth { get; }
        public int _tileHeight { get; }

        private List<TileSpecifier> _tileSpecs = new();
        private Dictionary<string, AtlasTile> _atlasTiles = new();
        private ImageFilter _filter = new();

        private Love.Canvas _workCanvas;

        public delegate void LoadTileDelegate(Love.Image image, Love.Quad quad, int rectX, int rectY);
        private LoadTileDelegate? OnLoadTile;

        public TileAtlasFactory(int tileWidth = OrthographicCoords.TILE_SIZE, 
            int tileHeight = OrthographicCoords.TILE_SIZE, 
            int tileCountX = 48, 
            int tileCountY = 48)
        {
            _resourceCache = IoCManager.Resolve<IResourceCache>();

            _tileWidth = tileWidth;
            _tileHeight = tileHeight;

            var imageWidth = tileCountX * tileWidth;
            var imageHeight = tileCountY * tileHeight;
            _binpack = new RectanglePacker(imageWidth, imageHeight);

            _workCanvas = Love.Graphics.NewCanvas(imageWidth, imageHeight);
        }

        public TileAtlasFactory WithLoadTileCallback(LoadTileDelegate? callback)
        {
            this.OnLoadTile = callback;
            return this;
        }

        private Tuple<Love.Image, Love.Quad> LoadImageAndQuad(TileSpecifier tile)
        {
            Love.Image image = _resourceCache.GetResource<LoveImageResource>(tile.ImagePath);

            if (tile.ImageRegion != null)
            {
                var quad = Love.Graphics.NewQuad(tile.ImageRegion.X, tile.ImageRegion.Y, tile.ImageRegion.Width, tile.ImageRegion.Height, image.GetWidth(), image.GetHeight());
                return Tuple.Create(image, quad);
            }
            else
            {
                var quad = Love.Graphics.NewQuad(0, 0, image.GetWidth(), image.GetHeight(), image.GetWidth(), image.GetHeight());
                return Tuple.Create(image, quad);
            }
        }

        public void LoadTile(TileSpecifier tile)
        {
            var (image, quad) = LoadImageAndQuad(tile);

            var quadSize = quad.GetViewport();

            if (!this._binpack.Pack((int)quadSize.Width, (int)quadSize.Height, out int rectX, out int rectY))
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

            var innerQuad = Love.Graphics.NewQuad(rectX, rectY, quadSize.Width, quadSize.Height, this._workCanvas.GetWidth(), this._workCanvas.GetHeight());

            var isTall = quadSize.Height == quadSize.Width * 2;
            var yOffset = 0;
            if (isTall)
                yOffset = -this._tileHeight;

            var atlasTile = new AtlasTile(innerQuad, yOffset);

            // Special case for wall tiles
            if (tile.HasOverhang)
                atlasTile.HasOverhang = true;

            this._atlasTiles.Add(tile.TileIndex, atlasTile);

            quad.Dispose();
        }

        public TileAtlasFactory LoadTiles(IEnumerable<TileSpecifier> tiles)
        {
            _tileSpecs.AddRange(tiles);

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
            var path = new ResourcePath("Cache/Atlas");
            if (!_resourceCache.UserData.Exists(path))
                _resourceCache.UserData.CreateDirectory(path);

            string hashString;
            using (var sha256Hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256))
            {
                foreach (var tile in _tileSpecs.OrderBy(x => x.TileIndex))
                {
                    sha256Hash.AppendData(Encoding.UTF8.GetBytes(tile.TileIndex));
                    sha256Hash.AppendData(Encoding.UTF8.GetBytes(tile.ImagePath.ToString()));
                }

                var data = sha256Hash.GetCurrentHash();

                var sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                hashString = sBuilder.ToString();
            }

            var serializedFilepath = path / $"{hashString}.bin";

            if (_resourceCache.UserData.Exists(serializedFilepath))
            {
                Logger.LogS(LogLevel.Info, "tile_atlas", $"Cache hit! {hashString}");

                var stream = _resourceCache.UserData.Open(serializedFilepath, FileMode.Open);
                return SerializationHelpers.Deserialize<TileAtlas>(serializedFilepath)!;
            }

            var canvas = Love.Graphics.GetCanvas();

            Love.Graphics.SetCanvas(this._workCanvas);
            Love.Graphics.SetBlendMode(Love.BlendMode.Alpha);
            Love.Graphics.SetColor(Love.Color.White);
            GraphicsEx.SetDefaultFilter(this._filter);

            foreach (var tile in _tileSpecs)
            {
                LoadTile(tile);
            }

            Love.Graphics.SetCanvas(canvas);
            Love.Graphics.SetDefaultFilter(Love.FilterMode.Linear, Love.FilterMode.Linear, 1);

            var imageData = this._workCanvas.NewImageData();
            var imageFilepath = path / $"{hashString}.png";
            var fileData = imageData.Encode(Love.ImageFormat.PNG);
            _resourceCache.UserData.WriteAllBytes(imageFilepath, fileData.GetBytes());

            var image = Love.Graphics.NewImage(imageData);

            var tileAtlas = new TileAtlas(image, imageFilepath, this._atlasTiles);

            SerializationHelpers.Serialize(serializedFilepath, tileAtlas));

            return tileAtlas;
        }

        public void Dispose()
        {
            this._workCanvas.Dispose();
        }
    }
}
