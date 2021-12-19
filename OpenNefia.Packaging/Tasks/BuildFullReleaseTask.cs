using Cake.Frosting;
using Cake.Common.Build;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.MSBuild;
using Cake.Common.Tools.DotNet.Build;

namespace OpenNefia.Packaging.Tasks
{
    [TaskName("BuildFullRelease")]
    public sealed class BuildFullReleaseTask : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            var dotNetSettings = new DotNetMSBuildSettings()
            {
                NoLogo = true,
                ContinuousIntegrationBuild = context.BuildSystem().IsLocalBuild,
                MaxCpuCount = 0, // Use all cores
            };

            dotNetSettings.Targets.Add("Rebuild");
            dotNetSettings.Properties["FullRelease"] = new List<string> { "True" };

            var settings = new DotNetBuildSettings
            {
                Configuration = context.BuildConfig,
                NoIncremental = false,
                MSBuildSettings = dotNetSettings
            };

            context.DotNetBuild("./OpenNefia.EntryPoint/OpenNefia.EntryPoint.csproj", settings);
        }
    }
}