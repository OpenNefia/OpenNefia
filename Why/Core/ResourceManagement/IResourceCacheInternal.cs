using OpenNefia.Core.ContentPack;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.ResourceManagement
{
    internal interface IResourceCacheInternal : IResourceCache, IResourceManagerInternal
    {
        void PreloadTextures();
    }
}
