using NetVips.Extensions;
using OpenNefia.Core.ResourceManagement;
using System.Drawing;
using static NetVips.Enums;
using VipsImage = NetVips.Image;

namespace OpenNefia.Core.Rendering
{
    /// <summary>
    /// Image loader that handles making parts of a BMP image transparent.
    /// </summary>
    public static class ImageLoader
    {
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

        /// <summary>
        /// Wrapper around <see cref="Love.Graphics.NewImageData"/> that also supports loading .BMP files
        /// with a transparent key color.
        /// </summary>
        /// <param name="fileData">File data that contains image data.</param>
        /// <returns></returns>
        public static Love.ImageData NewImageData(Love.FileData fileData, ImageLoadParameters loadParams)
        {
            if (loadParams.KeyColor != null && fileData.GetExtension() == "bmp")
            {
                var bytes = fileData.GetBytes();
                using var reader = new MemoryStream(bytes);
                var image = new Bitmap(reader).ToVips();

                image = RemoveKeyColor(image, loadParams.KeyColor.Value);

                var pixelDataFormat = GetLovePixelDataFormat(image);

                var memory = image.WriteToMemory();
                return Love.Image.NewImageData(image.Width, image.Height, pixelDataFormat, memory);
            }

            return Love.Image.NewImageData(fileData);
        }

        private static Love.ImageDataPixelFormat GetLovePixelDataFormat(VipsImage image)
        {
            switch (image.Interpretation)
            {
                case Interpretation.Srgb:
                case Interpretation.Rgb:
                    switch (image.Format)
                    {
                        case BandFormat.Char:
                        case BandFormat.Uchar:
                            return Love.ImageDataPixelFormat.RGBA8;
                        case BandFormat.Short:
                        case BandFormat.Ushort:
                            return Love.ImageDataPixelFormat.RGBA16;
                        case BandFormat.Float:
                            return Love.ImageDataPixelFormat.RGBA16F;
                        case BandFormat.Double:
                            return Love.ImageDataPixelFormat.RGBA32F;
                    }
                    break;
            }

            throw new InvalidOperationException($"Unknown .BMP image format: Interpretation={image.Interpretation} Format={image.Format}");
        }
    }
}
