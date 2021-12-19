using OpenNefia.Core.Utility;

namespace OpenNefia.Core.ResourceManagement
{
    public sealed class LoveImageResource : BaseResource
    {
        private Love.ImageData _imageData = default!;
        private Love.Image _image = default!;
        public override ResourcePath Fallback => new("/Graphic/Core/Chip/Default.png");

        public Love.ImageData ImageData => _imageData;
        public Love.Image Image => _image;

        private static Love.ImageData LoadImageData(IResourceCache cache, ResourcePath path)
        {
            var fileData = cache.GetResource<LoveFileDataResource>(path);
            return Love.Image.NewImageData(fileData);
        }

        public override void Load(IResourceCache cache, ResourcePath path)
        {
            _imageData = LoadImageData(cache, path);
            _image = Love.Graphics.NewImage(_imageData);
        }

        public override void Reload(IResourceCache cache, ResourcePath path, CancellationToken ct = default)
        {
            _image.Dispose();
            _imageData.Dispose();
            _imageData = LoadImageData(cache, path);
            _image = Love.Graphics.NewImage(_imageData);
        }

        // TODO: Due to a bug in Roslyn, NotNullIfNotNullAttribute doesn't work.
        // So this can't work with both nullables and non-nullables at the same time.
        // I decided to only have it work with non-nullables as such.
        public static implicit operator Love.Image(LoveImageResource res)
        {
            return res.Image;
        }

        public static implicit operator Love.ImageData(LoveImageResource res)
        {
            return res.ImageData;
        }
    }
}
