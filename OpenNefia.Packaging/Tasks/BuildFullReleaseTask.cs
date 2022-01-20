using Cake.Frosting;
using Cake.Common.Build;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.MSBuild;
using Cake.Common.Tools.DotNet.Build;
using Cake.Core.IO.Arguments;
using Cake.Git;
using Cake.Core.Diagnostics;

namespace OpenNefia.Packaging.Tasks
{
    [TaskName("BuildFullRelease")]
    public sealed class BuildFullReleaseTask : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            var gitRoot = context.GitFindRootFromPath(context.Environment.WorkingDirectory);
            if (gitRoot == null)
            {
                throw new InvalidOperationException("Git repository root not found in any parent directory.");
            }

            if (context.GitHasUncommitedChanges(gitRoot))
            {
                throw new InvalidOperationException("Uncommited changes detected; commit them first before running this task.");
            }

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
            dotNetSettings.Properties["SourceRevisionId"] = new List<string> { context.GitLogTip(gitRoot.FullPath).Sha };

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