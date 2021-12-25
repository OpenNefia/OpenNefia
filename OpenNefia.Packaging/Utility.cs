using Cake.Core;
using Cake.Core.IO;
using Cake.Git;

namespace OpenNefia.Packaging
{
    public static class Utility
    {
        public static bool PathStartsWith(string filePath, string startsWith)
        {
            return System.IO.Path.GetFullPath(filePath)
                .StartsWith(System.IO.Path.GetFullPath(startsWith), StringComparison.InvariantCultureIgnoreCase);
        }

        public static string GitCommitHash(this ICakeContext context, DirectoryPath dir)
        {
            return context.GitLogTip(dir).Sha.Substring(0, 7);
        }

        public static string GetProjectOutputDir(string project, BuildContext context)
        {
            return $"{project}/bin/{context.BuildConfig}/net6.0/{context.Runtime}/";
        }
    }
}
