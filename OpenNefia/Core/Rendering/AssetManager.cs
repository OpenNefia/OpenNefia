using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Rendering
{
    public class AssetManager : IAssetManager
    {
        private readonly Dictionary<PrototypeId<AssetPrototype>, AssetDrawable> _assets = new();

        private Love.Image LoadImageSource(ResourcePath atlasPath, ImageRegion imageRegion)
        {
            var parentImage = ImageLoader.NewImage(atlasPath.ToString());

            var quad = Love.Graphics.NewQuad(imageRegion.X, imageRegion.Y, imageRegion.Width, imageRegion.Height, parentImage.GetWidth(), parentImage.GetHeight());

            var canvas = Love.Graphics.NewCanvas(imageRegion.Width, imageRegion.Height);
            var oldCanvas = Love.Graphics.GetCanvas();

            // Reset global drawing state to be clean so the asset gets copied correctly
            Love.Graphics.GetBlendMode(out Love.BlendMode blendMode, out Love.BlendAlphaMode blendAlphaMode);
            var scissor = Love.Graphics.GetScissor();
            var color = Love.Graphics.GetColor();
            Love.Graphics.SetBlendMode(Love.BlendMode.Alpha);
            GraphicsEx.SetScissor();
            Love.Graphics.SetColor(1f, 1f, 1f, 1f);
            Love.Graphics.SetCanvas(canvas);

            Love.Graphics.Draw(quad, parentImage, 0, 0);

            Love.Graphics.SetBlendMode(blendMode, blendAlphaMode);
            GraphicsEx.SetScissor(scissor); // BUG: Love.Graphics.SetScissor is bugged (does not distinguish null scissors).
            Love.Graphics.SetColor(color);
            Love.Graphics.SetCanvas(oldCanvas);

            var image = Love.Graphics.NewImage(canvas.NewImageData());

            quad.Dispose();
            canvas.Dispose();

            return image;
        }

        private Love.Image LoadImage(AssetPrototype asset)
        {
            Love.Image image;

            var imageSpec = asset.Image;

            if (imageSpec.ImagePath != null)
            {
                var path = imageSpec.ImagePath;
                image = ImageLoader.NewImage(path.ToString());
            }
            else if (imageSpec.ImageRegion != null)
            {
                image = LoadImageSource(imageSpec.ImagePath!, imageSpec.ImageRegion);
            }
            else
            {
                throw new ArgumentException($"Asset has neither ImagePath nor ImageRegion: {asset.ID}");
            }

            if (imageSpec.ImageFilter != null)
            {
                image.SetFilter(imageSpec.ImageFilter.Min, imageSpec.ImageFilter.Mag, imageSpec.ImageFilter.Anisotropy);
            }

            return image;
        }

        private static AssetRegions GetRegions(AssetPrototype prototype, int width, int height)
        {
            if (prototype.RegionSpecifier != null)
            {
                return prototype.RegionSpecifier.GetRegions(width, height);
            }
            return prototype.Regions;
        }

        public void LoadAsset(PrototypeId<AssetPrototype> id)
        {
            if (_assets.ContainsKey(id))
            {
                throw new InvalidOperationException($"Asset {id} has already been loaded");
            }

            var prototype = id.ResolvePrototype();

            // TODO better caching of custom batch size assets
            if (prototype.RequiresSizeArgument)
            {
                throw new ArgumentException($"Asset {id} can only be loaded as a batch asset", nameof(id));
            }
            
            var image = LoadImage(prototype);
            var regions = prototype.Regions;

            _assets[id] = new AssetDrawable(prototype, image, regions);
        }

        public AssetDrawable GetSizedAsset(PrototypeId<AssetPrototype> id, Vector2i size)
        {
            var prototype = id.ResolvePrototype();

            var image = LoadImage(prototype);

            var regions = prototype.Regions;

            if (prototype.RegionSpecifier == null)
            {
                regions = GetRegions(prototype, size.X, size.Y);
            }

            return new AssetDrawable(prototype, image, regions);
        }

        public AssetDrawable GetAsset(PrototypeId<AssetPrototype> id)
        {
            return _assets[id];
        }
    }
}
