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
    public static class ImageLoader
    {
        private static Dictionary<string, Love.Image> Cache = new Dictionary<string, Love.Image>();

        public static void ClearCache()
        {
            Cache.Clear();
        }

        private static VipsImage RemoveKeyColor(VipsImage image, Love.Color keyColor)
        {
            if (image.Bands == 4)
            {
                image = image.Flatten();
            }

            var compare = new int[] { keyColor.r, keyColor.g, keyColor.b };
            var alpha = image.Equal(compare).Ifthenelse(0, 255).BandOr();
            return image.Bandjoin(alpha);
        }

        private static Love.Image LoadBitmap(string filepath, Love.Color? keyColor)
        {
            if (Cache.TryGetValue(filepath, out var cachedImage))
            {
                return cachedImage;
            }

            if (!File.Exists(filepath))
                throw new FileNotFoundException($"File {filepath} does not exist.");

            var image = new Bitmap(filepath).ToVips();

            if (!keyColor.HasValue)
            {
                keyColor = Love.Color.Black;
            }

            image = RemoveKeyColor(image, keyColor.Value);

            var memory = image.WriteToMemory();
            var imageData = Love.Image.NewImageData(image.Width, image.Height, Love.ImageDataPixelFormat.RGBA8, memory);

            var loveImage = Love.Graphics.NewImage(imageData);
            Cache[filepath] = loveImage;

            return loveImage;
        }

        /// <summary>
        /// Wrapper around <see cref="Love.Graphics.NewImage"/> that also supports loading .BMP files
        /// with a key color.
        /// </summary>
        /// <param name="filepath">Path to image file.</param>
        /// <returns></returns>
        public static Love.Image NewImage(string filepath, Love.Color? keyColor = null)
        {
            if (Path.GetExtension(filepath) == ".bmp")
            {
                return LoadBitmap(filepath, keyColor);
            }

            return Love.Graphics.NewImage(filepath);
        }

        public static Love.Image NewImage(ResourcePath filepath, Love.Color? keyColor = null)
        {
            return NewImage(filepath.ToString(), keyColor);
        }
    }
}
