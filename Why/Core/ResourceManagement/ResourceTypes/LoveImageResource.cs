using System.IO;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.ResourceManagement
{
    public sealed class LoveImageResource : BaseResource
    {
        private Love.Image _texture = default!;
        public override ResourcePath Fallback => new("/Textures/noSprite.png");

        public Love.Image Texture => _texture;

        public override void Load(IResourceCache cache, ResourcePath path)
        {
            _texture = ImageLoader.NewImage(path);
        }

        public override void Reload(IResourceCache cache, ResourcePath path, CancellationToken ct = default)
        {
            _texture.Dispose();
            _texture = ImageLoader.NewImage(path);
        }

        // TODO: Due to a bug in Roslyn, NotNullIfNotNullAttribute doesn't work.
        // So this can't work with both nullables and non-nullables at the same time.
        // I decided to only have it work with non-nullables as such.
        public static implicit operator Love.Image(LoveImageResource res)
        {
            return res.Texture;
        }
    }
}
