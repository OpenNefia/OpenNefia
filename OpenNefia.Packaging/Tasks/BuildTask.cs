using Cake.Core.Diagnostics;
using Cake.Frosting;
using Cake.Common.Tools.DotNet.Build;
using Cake.Common.Tools.DotNet.MSBuild;
using Cake.Common.Tools.DotNetCore.Build;
using Cake.Common.Build;
using Cake.Common.Tools.DotNet;
using System.Collections.Generic;
using System.IO;
using Cake.Common.IO;
using System.IO.Compression;

namespace OpenNefia.Packaging.Tasks
{
    [TaskName("Build")]
    public sealed class BuildTask : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            var dotNetSettings = new DotNetMSBuildSettings()
            {
                NoLogo = true,
                ContinuousIntegrationBuild = context.BuildSystem().IsLocalBuild,
                MaxCpuCount = 0 // Use all cores
            };

            dotNetSettings.Properties[BuildProperties.FullRelease] = new List<string> { "True" };

            var settings = new DotNetBuildSettings
            {
                Configuration = context.BuildConfig,
                NoRestore = true,
                NoIncremental = false,
                MSBuildSettings = dotNetSettings
            };

            context.DotNetBuild("./OpenNefia.EntryPoint/OpenNefia.EntryPoint.csproj", settings);
        }
    }

    [TaskName("Package")]
    // [IsDependentOn(typeof(BuildTask))]
    public sealed class PackageTask : FrostingTask<BuildContext>
    {
        /// <summary>
        /// Projects with a Resources/ directory that should be merged into the zip archive's.
        /// 
        /// TODO: I would rather have one resource root per mod, to not have to do destructive
        /// merging and have mods be drop-in instead. That would require some additional mod
        /// bookkeeping in core.
        /// </summary>
        private static readonly string[] ProjectsWithResources = new string[]
        {
            "OpenNefia.Core",
            "OpenNefia.Content",
            "OpenNefia.LecchoTorte",
        };

        /// <summary>
        /// File extensions to add to the zip.
        /// </summary>
        private static readonly string[] BinaryFilePatterns = new string[]
        {
            "*.exe",
            "*.dll",
            "*.pdb"
        };

        public override void Run(BuildContext context)
        {
            if (Directory.Exists(Constants.OutputDir))
            {
                context.Log.Information("Clearing old output folder...");
                Directory.Delete(Constants.OutputDir, recursive: true);
            }

            Directory.CreateDirectory(Constants.OutputDir);

            var distribName = "OpenNefia";
            var zipName = $"{distribName}.zip";
            var zipPath = Path.Combine(Constants.OutputDir, zipName);

            var entryPointOutput = $"OpenNefia.EntryPoint/bin/{context.BuildConfig}/net6.0/";

            using (var stream = File.OpenWrite(zipPath))
            {
                using (var zipArchive = new ZipArchive(stream, ZipArchiveMode.Create))
                {
                    context.Log.Information($"Packing binaries...");
                    ZipBinaryFiles(zipArchive, distribName, entryPointOutput);

                    context.Log.Information($"Packing resources...");
                    ZipResources(zipArchive, distribName);
                }
            }

            context.Log.Information($"Wrote .zip to {zipPath}");
        }

        private void ZipBinaryFiles(ZipArchive zipArchive, string distribName, string entryPointOutput)
        {
            foreach (var pattern in BinaryFilePatterns)
            {
                foreach (var file in Directory.EnumerateFiles(entryPointOutput, pattern))
                {
                    zipArchive.CreateEntryFromFile(file, $"{distribName}/{Path.GetRelativePath(entryPointOutput, file)}");
                }
            }
        }

        private void ZipResources(ZipArchive zipArchive, string distribName)
        {
            var enumOptions = new EnumerationOptions()
            {
                RecurseSubdirectories = true
            };

            foreach (var projectDir in ProjectsWithResources)
            {
                var resourcesDir = Path.Combine(projectDir, "Resources");
                
                foreach (var file in Directory.EnumerateFiles(resourcesDir, "*", enumOptions))
                {
                    zipArchive.CreateEntryFromFile(file, $"{distribName}/{Path.GetRelativePath(projectDir, file)}");
                }
            }
        }
    }
}