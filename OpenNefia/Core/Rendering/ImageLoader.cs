using NetVips.Extensions;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VipsImage = NetVips.Image;

namespace OpenNefia.Core.Rendering
{
    /// <summary>
    /// TODO this can't be put into IResourceCache because of the need to
    /// specify a key color.
    /// 
    /// Robust worked around this by putting a .yml file next to every image
    /// with the metadata to use (I don't really like this).
    /// </summary>
    [Obsolete("TODO move to IResourceCache")]
    public static class ImageLoader
    {
        private static Dictionary<ResourcePath, Love.Image> _cache = new();

        public static void ClearCache()
        {
            _cache.Clear();
        }

        private static VipsImage RemoveKeyColor(VipsImage image, Maths.Color keyColor)
        {
            if (image.Bands == 4)
            {
                image = image.Flatten();
            }

            var compare = new int[] { keyColor.RByte, keyColor.GByte, keyColor.BByte };
            var alpha = image.Equal(compare).Ifthenelse(0, 255).BandOr();
            return image.Bandjoin(alpha);
        }

        private static Love.Image LoadBitmap(Stream stream, Maths.Color keyColor)
        {
            var image = new Bitmap(stream).ToVips();

            image = RemoveKeyColor(image, keyColor);

            var memory = image.WriteToMemory();
            var imageData = Love.Image.NewImageData(image.Width, image.Height, Love.ImageDataPixelFormat.RGBA8, memory);

            var loveImage = Love.Graphics.NewImage(imageData);

            return loveImage;
        }

        /// <summary>
        /// Wrapper around <see cref="Love.Graphics.NewImage"/> that also supports loading .BMP files
        /// with a key color.
        /// </summary>
        /// <param name="filepath">Path to image file.</param>
        /// <returns></returns>
        public static Love.Image NewImage(Stream stream, ResourcePath filepath, Maths.Color keyColor)
        {
            if (_cache.TryGetValue(filepath, out var cachedImage))
            {
                return cachedImage;
            }

            Love.Image image = LoadBitmap(stream, keyColor);

            _cache[filepath] = image;
            return image;
        }
    }
}
