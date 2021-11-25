using Why.Core.ContentPack;
using Why.Core.Utility;

namespace Why.Core.ResourceManagement
{
    internal interface IResourceCacheInternal : IResourceCache, IResourceManagerInternal
    {
        void PreloadTextures();
    }
}
