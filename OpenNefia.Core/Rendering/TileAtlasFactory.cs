using OpenNefia.Core.ContentPack;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.Utility;
using System.Security.Cryptography;
using System.Text;

namespace OpenNefia.Core.Rendering
{
    /// <summary>
    /// Dynamically generates tile atlases by aggregating a series of
    /// <see cref="TileSpecifier"/>s and packing them into a single image.
    /// </summary>
    public sealed class TileAtlasFactory : IDisposable
    {
        private readonly IResourceCache _resourceCache;

        private static readonly ResourcePath _cachePath = new ResourcePath("/Cache/Atlas");

        internal RectanglePacker _binpack { get; }
        public int _tileWidth { get; }
        public int _tileHeight { get; }

        private List<TileSpecifier> _tileSpecs = new();
        private Dictionary<string, AtlasTile> _atlasTiles = new();
        private ImageFilter _filter = new(Love.FilterMode.Nearest, Love.FilterMode.Linear, 1);
        private Dictionary<Love.Image, Love.SpriteBatch> _pendingQuads = new();

        private Love.Canvas _workCanvas;

        public TileAtlasFactory(IResourceCache resourceCache, 
            int tileWidth = OrthographicCoords.TILE_SIZE, 
            int tileHeight = OrthographicCoords.TILE_SIZE, 
            int tileCountX = 48, 
            int tileCountY = 48)
        {
            _resourceCache = resourceCache;

            _tileWidth = tileWidth;
            _tileHeight = tileHeight;

            var imageWidth = tileCountX * tileWidth;
            var imageHeight = tileCountY * tileHeight;
            _binpack = new RectanglePacker(imageWidth, imageHeight);

            _workCanvas = Love.Graphics.NewCanvas(imageWidth, imageHeight);
        }

        private Tuple<Love.Image, Love.Quad> LoadImageAndQuad(TileSpecifier tile)
        {
            Love.Image image = _resourceCache.GetResource<LoveImageResource>(tile.Filepath);

            if (tile.Region != null)
            {
                var imageRegion = tile.Region.Value;
                var quad = Love.Graphics.NewQuad(imageRegion.Left, imageRegion.Top, 
                    imageRegion.Width, imageRegion.Height, 
                    image.GetWidth(), image.GetHeight());

                return Tuple.Create(image, quad);
            }
            else
            {
                var quad = Love.Graphics.NewQuad(0, 0, 
                    image.GetWidth(), image.GetHeight(), 
                    image.GetWidth(), image.GetHeight());

                return Tuple.Create(image, quad);
            }
        }

        public void LoadTile(TileSpecifier tile)
        {
            var (image, quadInSource) = LoadImageAndQuad(tile);

            var quadSize = quadInSource.GetViewport();

            if (!_binpack.Pack((int)quadSize.Width, (int)quadSize.Height, out int rectX, out int rectY))
            {
                throw new Exception($"Ran out of space while packing tile atlas ({tile.AtlasIndex})");
            }

            if (!_pendingQuads.ContainsKey(image))
                _pendingQuads.Add(image, Love.Graphics.NewSpriteBatch(image, 2048, Love.SpriteBatchUsage.Static));

            var quadInAtlas = Love.Graphics.NewQuad(rectX, rectY, quadSize.Width, quadSize.Height, _workCanvas.GetWidth(), _workCanvas.GetHeight());

            _pendingQuads[image].Add(quadInSource, rectX, rectY);

            var isTall = quadSize.Height == quadSize.Width * 2;
            var yOffset = 0;
            if (isTall)
                yOffset = -_tileHeight;

            var atlasTile = new AtlasTile(quadInAtlas, yOffset, tile.HasOverhang);

            _atlasTiles.Add(tile.AtlasIndex, atlasTile);
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

        private string GetSha256()
        {
            using (var sha256Hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256))
            {
                foreach (var tile in _tileSpecs.OrderBy(x => x.AtlasIndex))
                {
                    sha256Hash.AppendData(Encoding.UTF8.GetBytes(tile.AtlasIndex));
                    sha256Hash.AppendData(Encoding.UTF8.GetBytes(tile.Filepath.ToString()));
                }

                return sha256Hash.GetCurrentHash().ToHexString();
            }
        }

        private TileAtlas? CheckCache(string hashString)
        {
            if (!_resourceCache.UserData.Exists(_cachePath))
                _resourceCache.UserData.CreateDirectory(_cachePath);

            var serializedFilepath = _cachePath / $"{hashString}.bin";

            if (_resourceCache.UserData.Exists(serializedFilepath))
            {
                Logger.LogS(LogLevel.Info, "tile_atlas", $"Cache hit! {hashString}");

                return SerializationHelpers.Deserialize<TileAtlas>(serializedFilepath)!;
            }

            return null;
        }

        public TileAtlas Build()
        {
            // var hashString = GetSha256();
            // var cached = CheckCache(hashString);
            // if (cached != null)
            // {
            //     return cached;
            // }

            var canvas = Love.Graphics.GetCanvas();

            Love.Graphics.SetCanvas(_workCanvas);
            Love.Graphics.Clear();
            Love.Graphics.SetBlendMode(Love.BlendMode.Alpha);
            Love.Graphics.SetColor(Love.Color.White);
            var oldFilter = GraphicsEx.GetDefaultFilter();
            GraphicsEx.SetDefaultFilter(_filter);

            foreach (var tile in _tileSpecs)
            {
                LoadTile(tile);
            }

            foreach (var batch in _pendingQuads.Values)
            {
                Love.Graphics.Draw(batch, 0, 0);
            }

            Love.Graphics.SetCanvas(canvas);

            var imageData = _workCanvas.NewImageData();
            var image = Love.Graphics.NewImage(imageData);
            image.SetFilter(Love.FilterMode.Nearest, Love.FilterMode.Linear, 1);

            var tileAtlas = new TileAtlas(image, _atlasTiles);

            // WriteCachedAtlas(tileAtlas, imageData, hashString);

            GraphicsEx.SetDefaultFilter(oldFilter);

            return tileAtlas;
        }

        private void WriteCachedAtlas(TileAtlas tileAtlas, Love.ImageData imageData, string hashString)
        {
            var imageFilepath = _cachePath / $"{hashString}.png";
            var serializedFilepath = _cachePath / $"{hashString}.bin";
            var fileData = imageData.Encode(Love.ImageFormat.PNG);
            _resourceCache.UserData.WriteAllBytes(imageFilepath, fileData.GetBytes());
            SerializationHelpers.Serialize(serializedFilepath, tileAtlas);
        }

        public void Dispose()
        {
            _workCanvas.Dispose();

            foreach (var (sourceImage, batch) in _pendingQuads)
            {
                batch.Dispose();
                sourceImage.Dispose();
            }

            _pendingQuads.Clear();
        }
    }
}
