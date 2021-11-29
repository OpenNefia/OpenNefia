using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Core.Rendering
{
    public interface IAssetManager
    {
        public void LoadAsset(PrototypeId<AssetPrototype> id);
        public AssetDrawable GetSizedAsset(PrototypeId<AssetPrototype> id, Vector2i size);
        public AssetDrawable GetAsset(PrototypeId<AssetPrototype> id);
    }
}
