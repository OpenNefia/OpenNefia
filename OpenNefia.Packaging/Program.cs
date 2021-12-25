using Cake.Core;
using Cake.Frosting;

namespace OpenNefia.Packaging
{
    /// <summary>
    /// "I prefer cakes and candies to alcoholic drinks. You want [build artifacts]? Gimme [<see cref="ICakeContext"/>]!"
    /// </summary>
    public static class Program
    {
        public static int Main(string[] args)
        {
            return new CakeHost()
                .UseContext<BuildContext>()
                .Run(args);
        }
    }

    public class BuildContext : FrostingContext
    {
        /// <summary>
        /// Build configuration, like "Debug" or "Release".
        /// </summary>
        public string BuildConfig { get; set; } = "Release";

        /// <summary>
        /// RID of the .NET runtime to build for.
        /// </summary>
        public string Runtime { get; set; } = "win-x64";

        public BuildContext(ICakeContext context)
            : base(context)
        {
            BuildConfig = context.Arguments.GetArgument("config") ?? BuildConfig;
            Runtime = context.Arguments.GetArgument("runtime") ?? Runtime;
        }
    }
}