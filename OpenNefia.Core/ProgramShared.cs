using OpenNefia.Core.ContentPack;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core
{
    internal static class ProgramShared
    {
        private const string LocalResourcesPathFile = "LocalResourcesPath.txt";

        internal static void DoCoreMounts(IResourceManagerInternal res)
        {
            res.MountContentDirectory("Resources");

#if !FULL_RELEASE
            // Assets directory in OpenNefia.Core.
            res.MountContentDirectory("../../../../OpenNefia.Core/Resources");
#endif
        }

        internal static void DoContentMounts(IResourceManagerInternal res)
        {
#if !FULL_RELEASE
            var modLoader = IoCManager.Resolve<IModLoader>();
            foreach (var mod in modLoader.LoadedModules)
            {
                var stream = mod.GetManifestResourceStream(LocalResourcesPathFile);
                if (stream == null)
                {
                    throw new ArgumentException($"Content assembly '{mod.FullName}' has no embedded '{LocalResourcesPathFile}'");
                }

                using var reader = new StreamReader(stream);
                var path = reader.ReadToEnd()!.Trim();
                res.MountContentDirectory(ResourcePath.FromRelativeSystemPath(path).ToRelativePath().ToString());
            }
#endif
        }
    }
}
