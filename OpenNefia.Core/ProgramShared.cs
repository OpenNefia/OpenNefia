using OpenNefia.Core.ContentPack;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Utility;
using System.Reflection;

namespace OpenNefia.Core
{
    internal static class ProgramShared
    {
        internal static void DoCoreMounts(IResourceManagerInternal res)
        {
            res.MountContentDirectory("Resources");

#if !FULL_RELEASE
            // Assets directory in OpenNefia.Core.
            res.MountContentDirectory("../../../../OpenNefia.Core/Resources");
#endif
        }

        internal static void DoContentMounts(IResourceManagerInternal res, IEnumerable<Assembly>? assemblies = null)
        {
            // In full release mode, all mods should be packaged under the "Resources" folder,
            // preferably in .zip format. The following mounts the "Resources" folder for each mod
            // from their respective source directories via a text file containing the path to its
            // Resources directory generated with MSBuild, for development purposes (allows hot
            // reloading from those directories).
#if !FULL_RELEASE
            foreach (var txtFile in res.ContentFindFiles("/References").ToList())
            {
                var path = res.ContentFileReadAllText(txtFile).Trim();
                var relative = Path.GetRelativePath(PathHelpers.GetExecutableDirectory(), path);
                res.MountContentDirectory(relative);
            }
#endif
        }
    }
}
