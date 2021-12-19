using System;
using System.IO;

namespace OpenNefia.Packaging
{
    public static class Utility
    {
        internal static bool PathStartsWith(string filePath, string startsWith)
        {
            return Path.GetFullPath(filePath)
                .StartsWith(Path.GetFullPath(startsWith), StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
