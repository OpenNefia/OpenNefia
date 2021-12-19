using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Core.Rendering
{
    public interface IAssetManager
    {
        void LoadAsset(PrototypeId<AssetPrototype> id);
        IAssetDrawable GetSizedAsset(PrototypeId<AssetPrototype> id, Vector2i size);
        IAssetDrawable GetAsset(PrototypeId<AssetPrototype> id);
        void PreloadAssets();
    }
}
