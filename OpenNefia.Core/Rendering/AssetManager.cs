using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.Timing;

namespace OpenNefia.Core.Rendering
{
    public class AssetManager : IAssetManager
    {
        [Dependency] private readonly IPrototypeManager _prototypes = default!;
        [Dependency] private readonly IResourceCache _resourceCache = default!;

        private readonly Dictionary<PrototypeId<AssetPrototype>, AssetDrawable> _assets = new();

        private Love.Image LoadImageSource(AssetSpecifier spec)
        {
            var atlasPath = spec.Filepath;
            var imageRegion = spec.Region!.Value;
            var parentImage = _resourceCache.GetLoveImageResource(atlasPath);

            var quad = Love.Graphics.NewQuad(imageRegion.Left, imageRegion.Top, imageRegion.Width, imageRegion.Height, parentImage.GetWidth(), parentImage.GetHeight());

            var canvas = Love.Graphics.NewCanvas(imageRegion.Width, imageRegion.Height);
            var oldCanvas = Love.Graphics.GetCanvas();

            // Reset global drawing state to be clean so the asset gets copied correctly
            Love.Graphics.GetBlendMode(out Love.BlendMode blendMode, out Love.BlendAlphaMode blendAlphaMode);
            var scissor = Love.Graphics.GetScissor();
            var color = Love.Graphics.GetColor();
            Love.Graphics.SetBlendMode(Love.BlendMode.Alpha);
            Love.Graphics.SetScissor();
            Love.Graphics.SetColor(1f, 1f, 1f, 1f);
            Love.Graphics.SetCanvas(canvas);

            Love.Graphics.Draw(quad, parentImage, 0, 0);

            Love.Graphics.SetBlendMode(blendMode, blendAlphaMode);
            if (scissor.HasValue)
                Love.Graphics.SetScissor(scissor.Value);
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

            if (imageSpec.Region == null)
            {
                image = _resourceCache.GetLoveImageResource(imageSpec.Filepath);
            }
            else
            {
                image = LoadImageSource(imageSpec);
            }

            if (imageSpec.Filter != null)
            {
                image.SetFilter(imageSpec.Filter.Min, imageSpec.Filter.Mag, imageSpec.Filter.Anisotropy);
            }

            return image;
        }

        private static AssetRegions GetRegions(AssetPrototype prototype, Vector2i size)
        {
            if (prototype.RegionSpecifier != null)
            {
                return prototype.RegionSpecifier.GetRegions(size);
            }
            return prototype.Regions;
        }

        public void LoadAsset(PrototypeId<AssetPrototype> id)
        {
            if (_assets.ContainsKey(id))
            {
                throw new InvalidOperationException($"Asset '{id}' has already been loaded");
            }

            var prototype = id.ResolvePrototype();
            var image = LoadImage(prototype);
            var regions = GetRegions(prototype, Vector2i.One);

            _assets[id] = new AssetDrawable(prototype, image, regions);
        }

        public void PreloadAssets()
        {
            Logger.InfoS("boot.asset", "Preloading assets...");

            using (var sw = new ProfilerLogger(LogLevel.Info, "boot.asset", "Preloading assets"))
            {
                foreach (var assetProto in _prototypes.EnumeratePrototypes<AssetPrototype>())
                {
                    LoadAsset(assetProto.GetStrongID());
                }
            }
        }

        public IAssetDrawable GetSizedAsset(PrototypeId<AssetPrototype> id, Vector2i size)
        {
            var prototype = id.ResolvePrototype();

            var image = LoadImage(prototype);
            var regions = GetRegions(prototype, size);

            return new AssetDrawable(prototype, image, regions);
        }

        public IAssetDrawable GetAsset(PrototypeId<AssetPrototype> id)
        {
            return _assets[id];
        }
    }
}
