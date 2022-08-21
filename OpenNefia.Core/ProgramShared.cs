using OpenNefia.Core.ContentPack;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Utility;
using System.Reflection;

namespace OpenNefia.Core
{
    internal static class ProgramShared
    {
        /// <summary>
        /// Embedded resource file. It should
        /// </summary>
        private const string LocalResourcesPathFile = "LocalResourcesPath.txt";

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
            // from their respective source directories via an embedded resource generated with
            // MSBuild, for development purposes (allows hot reloading from those directories).
            //
            // This will probably need changing if the mod system ends up using something like NuGet
            // to manage dependencies.
#if !FULL_RELEASE
            foreach (var mod in assemblies ?? IoCManager.Resolve<IModLoader>().LoadedModules)
            {
                var stream = mod.GetManifestResourceStream(LocalResourcesPathFile);
                if (stream == null)
                    continue;

                using var reader = new StreamReader(stream);
                var path = reader.ReadToEnd()!.Trim();
                var relative = Path.GetRelativePath(PathHelpers.GetExecutableDirectory(), path);
                res.MountContentDirectory(relative);
            }
#endif
        }
    }
}
