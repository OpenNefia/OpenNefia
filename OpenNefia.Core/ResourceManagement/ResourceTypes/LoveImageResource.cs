using JetBrains.Annotations;
using Love;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Utility;
using YamlDotNet.RepresentationModel;
using Color = OpenNefia.Core.Maths.Color;

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

            var loadParameters = TryLoadTextureParameters(cache, path) ?? new ImageLoadParameters();

            if (path.Extension == "bmp" && loadParameters.KeyColor == null)
                loadParameters.KeyColor = Color.Black;

            return ImageLoader.NewImageData(fileData, loadParameters);
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

        private static ImageLoadParameters? TryLoadTextureParameters(IResourceCache cache, ResourcePath path)
        {
            var metaPath = path.WithName(path.Filename + ".yml");
            if (cache.TryContentFileRead(metaPath, out var stream))
            {
                using (stream)
                {
                    YamlDocument yamlData;
                    using (var reader = new StreamReader(stream, EncodingHelpers.UTF8))
                    {
                        var yamlStream = new YamlStream();
                        yamlStream.Load(reader);
                        if (yamlStream.Documents.Count == 0)
                        {
                            return null;
                        }

                        yamlData = yamlStream.Documents[0];
                    }

                    return ImageLoadParameters.FromYaml((YamlMappingNode)yamlData.RootNode);
                }
            }

            return null;
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

    /// <summary>
    ///     Flags for loading of textures.
    /// </summary>
    public struct ImageLoadParameters
    {
        /// <summary>
        ///     The color of this BMP image to treat as transparent.
        /// </summary>
        public Color? KeyColor { get; set; } = null;

        public static ImageLoadParameters FromYaml(YamlMappingNode yaml)
        {
            var loadParams = new ImageLoadParameters();

            if (yaml.TryGetNode("keyColor", out var keyColor))
            {
                loadParams.KeyColor = Color.FromHex(keyColor.AsString());
            }

            return loadParams;
        }
    }
}
