using OpenNefia.Core.ResourceManagement;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenNefia.Core.Rendering
{
    /// <summary>
    /// Image loader that handles making parts of a BMP image transparent.
    /// </summary>
    public static class ImageLoader
    {
        private static void RemoveKeyColor(Image<Rgba32> image, Maths.Color keyColor)
        {
            image.ProcessPixelRows(accessor => {    
                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<Rgba32> pixelRow = accessor.GetRowSpan(y);
                    for (int x = 0; x < pixelRow.Length; x++)
                    {
                        ref Rgba32 pixel = ref pixelRow[x];
                        if (pixel.R == keyColor.RByte && pixel.G == keyColor.GByte && pixel.B == keyColor.BByte)
                        {
                            pixel = Color.Transparent;
                        }
                    }
                }
            });
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
                var image = Image.Load<Rgba32>(bytes);

                RemoveKeyColor(image, loadParams.KeyColor.Value);

                var newBytes = new byte[image.Width * image.Height * 4];
                image.CopyPixelDataTo(newBytes);

                return Love.Image.NewImageData(image.Width, image.Height, Love.ImageDataPixelFormat.RGBA8, newBytes);
            }

            return Love.Image.NewImageData(fileData);
        }
    }
}
