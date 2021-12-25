using Cake.Frosting;
using Cake.Common.Build;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.MSBuild;
using Cake.Common.Tools.DotNet.Build;
using Cake.Core.IO.Arguments;

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

            // whyyyyyyy does 'dotnet clean' not clean everything...
            var outputDir = Utility.GetProjectOutputDir("OpenNefia.EntryPoint", context);
            if (Directory.Exists(outputDir))
                Directory.Delete(outputDir, true);

            dotNetSettings.Targets.Add("Rebuild");
            dotNetSettings.Properties["FullRelease"] = new List<string> { "True" };

            var settings = new DotNetBuildSettings
            {
                Configuration = context.BuildConfig,
                NoIncremental = false,
                MSBuildSettings = dotNetSettings,
                Runtime = context.Runtime,
                ArgumentCustomization = (args) => { args.Append(new TextArgument("--no-self-contained")); return args; }
            };

            context.DotNetBuild("./OpenNefia.EntryPoint/OpenNefia.EntryPoint.csproj", settings);
        }
    }
}