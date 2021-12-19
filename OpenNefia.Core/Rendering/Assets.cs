using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Core.Rendering
{
    public static class Assets
    {
        public static IAssetDrawable Get(PrototypeId<AssetPrototype> id)
        {
            return IoCManager.Resolve<IAssetManager>().GetAsset(id);
        }

        public static IAssetDrawable GetSized(PrototypeId<AssetPrototype> id, Vector2i size)
        {
            return IoCManager.Resolve<IAssetManager>().GetSizedAsset(id, size);
        }
    }
}
