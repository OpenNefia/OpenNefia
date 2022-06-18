using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Core.Rendering
{
    // TODO: Thinking of moving all calls to these into UiLayer.Initialize()
    // and such, and adding IAssetManager as a [Dependency].
    public static class Assets
    {
        [Obsolete]
        public static IAssetInstance Get(PrototypeId<AssetPrototype> id)
        {
            return IoCManager.Resolve<IAssetManager>().GetAsset(id);
        }

        [Obsolete]
        public static IAssetInstance GetSized(PrototypeId<AssetPrototype> id, Vector2i size)
        {
            return IoCManager.Resolve<IAssetManager>().GetSizedAsset(id, size);
        }
    }
}
