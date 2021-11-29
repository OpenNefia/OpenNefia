using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Core.Rendering
{
    public interface IAssetManager
    {
        void LoadAsset(PrototypeId<AssetPrototype> id);
        AssetDrawable GetSizedAsset(PrototypeId<AssetPrototype> id, Vector2i size);
        AssetDrawable GetAsset(PrototypeId<AssetPrototype> id);
        void PreloadAssets();
    }
}
