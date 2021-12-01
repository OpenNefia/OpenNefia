using OpenNefia.Core.Rendering;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.ResourceManagement
{
    /// <summary>
    /// TODO: <see cref="ResourceManagement.IResourceCache" /> is impractical to use with assets
    /// that require arguments to instantiate them, like fonts (size) and images (key color).
    /// This should be rectified somehow in the future.
    /// </summary>
    public static class ResourceCacheExt
    {
        public static Love.Image GetLoveImageResource(this IResourceCache cache, ResourcePath path, Maths.Color? keyColor = null)
        {
            // Automatically remove key colors in bitmap images.
            if (path.Extension == "bmp" && keyColor == null)
            {
                keyColor = Maths.Color.Black;
            }

            if (keyColor != null)
            {
                var stream = cache.ContentFileRead(path);
                return ImageLoader.NewImage(stream, path, keyColor.Value);
            }
            else
            {
                return cache.GetResource<LoveImageResource>(path);
            }
        }
    }
}
