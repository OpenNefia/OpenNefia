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

        private readonly Dictionary<PrototypeId<AssetPrototype>, AssetInstance> _assets = new();

        private Love.Image LoadImageSource(AssetSpecifier spec)
        {
            var atlasPath = spec.Filepath;
            var imageRegion = spec.Region!.Value;
            var parentImage = _resourceCache.GetResource<LoveImageResource>(atlasPath).Image;

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

            return image;
        }

        private Love.Image LoadImage(AssetPrototype asset)
        {
            Love.Image image;

            var imageSpec = asset.Image;

            image = _resourceCache.GetResource<LoveImageResource>(imageSpec.Filepath);

            if (imageSpec.Filter != null)
            {
                image.SetFilter(imageSpec.Filter.Min, imageSpec.Filter.Mag, imageSpec.Filter.Anisotropy);
            }

            return image;
        }

        private static AssetRegions GetRegions(AssetPrototype prototype, Vector2i size)
        {
            var regions = new AssetRegions();
            foreach (var (key, region) in prototype.Regions)
                regions.Add(key, region);

            if (prototype.RegionSpecifier != null)
            {
                foreach (var (key, region) in prototype.RegionSpecifier.GetRegions(size))
                    regions.Add(key, region);
            }

            return regions;
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

            _assets[id] = new AssetInstance(prototype, image, regions);
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

        public IAssetInstance GetSizedAsset(PrototypeId<AssetPrototype> id, Vector2i size)
        {
            var prototype = id.ResolvePrototype();

            var image = LoadImage(prototype);
            var regions = GetRegions(prototype, size);

            return new AssetInstance(prototype, image, regions);
        }

        public IAssetInstance GetAsset(PrototypeId<AssetPrototype> id)
        {
            return _assets[id];
        }
    }
}
