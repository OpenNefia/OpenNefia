using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Core.Rendering
{
    public interface IAssetManager
    {
        void LoadAsset(PrototypeId<AssetPrototype> id);
        IAssetInstance GetSizedAsset(PrototypeId<AssetPrototype> id, Vector2i size);
        IAssetInstance GetAsset(PrototypeId<AssetPrototype> id);
        void PreloadAssets();
    }
}
