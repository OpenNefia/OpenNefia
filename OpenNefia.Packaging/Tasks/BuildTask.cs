using Cake.Core.Diagnostics;
using Cake.Frosting;
using Cake.Common.Build;
using Cake.Common.Tools.DotNet;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Cake.Common.Tools.DotNet.MSBuild;
using Cake.Common.Tools.DotNet.Build;
using System.Diagnostics;
using Cake.Git;

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

            dotNetSettings.Targets.Add("Publish");
            dotNetSettings.Properties["FullRelease"] = new List<string> { "True" };

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
    [IsDependentOn(typeof(BuildTask))]
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
        /// Resource directories that should not be redistributed.
        /// </summary>
        private static readonly string[] IgnoredResourceDirs = new string[]
        {
            "Resources/Graphic/Elona/",
            "Resources/Sound/Elona/"
        };

        /// <summary>
        /// Resource files that should not be redistributed.
        /// </summary>
        private static readonly HashSet<string> IgnoredResourceFiles = new()
        {
            ".keep"
        };

        private string GetProjectOutputDir(string project, BuildContext context)
        {
            return $"{project}/bin/{context.BuildConfig}/net6.0/";
        }

        public override void Run(BuildContext context)
        {
            if (Directory.Exists(Constants.OutputDir))
            {
                context.Log.Information("Clearing old output folder...");
                Directory.Delete(Constants.OutputDir, recursive: true);
            }

            Directory.CreateDirectory(Constants.OutputDir);

            var entryPointOutput = Path.Combine(GetProjectOutputDir("OpenNefia.EntryPoint", context), "publish");
            var coreAssemblyPath = Path.Combine(entryPointOutput, "OpenNefia.Core.dll");

            var assemblyInfo = FileVersionInfo.GetVersionInfo(coreAssemblyPath);

            var commitHash = context.GitLogTip(context.Environment.WorkingDirectory).Sha.Substring(0, 7);

            var distribName = $"OpenNefia-{assemblyInfo.ProductVersion}-{commitHash}";
            var zipName = $"{distribName}.zip";
            var zipPath = Path.Combine(Constants.OutputDir, zipName);


            using (var stream = File.OpenWrite(zipPath))
            {
                using (var zipArchive = new ZipArchive(stream, ZipArchiveMode.Create))
                {
                    context.Log.Information($"Copying resources...");
                    CopyResources(entryPointOutput);

                    context.Log.Information($"Moving content assemblies...");
                    MoveContentAssemblies(entryPointOutput, context);

                    context.Log.Information($"Packing files...");
                    ZipAllFiles(zipArchive, distribName, entryPointOutput);
                }
            }

            context.Log.Information($"Wrote .zip to {zipPath}");
        }

        private void CopyResources(string entryPointOutput)
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
                    var filename = Path.GetFileName(file);
                    if (IgnoredResourceFiles.Contains(filename))
                        continue;

                    var relFile = Path.GetRelativePath(projectDir, file);
                    if (IgnoredResourceDirs.Any(ignoredDir => Utility.PathStartsWith(relFile, ignoredDir)))
                        continue;

                    var outputFile = Path.Join(entryPointOutput, relFile);

                    var dir = Path.GetDirectoryName(outputFile)!;
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    if (!File.Exists(outputFile))
                        File.Copy(file, outputFile);
                }
            }
        }

        private void MoveContentAssemblies(string entryPointOutput, BuildContext context)
        {
            var assembliesDir = Path.Combine(entryPointOutput, "Resources", "Assemblies");
            Directory.CreateDirectory(assembliesDir);

            foreach (var projectName in ProjectsWithResources)
            {
                // Core is loaded internally by the engine.
                if (projectName == "OpenNefia.Core")
                    continue;

                var binOutput = GetProjectOutputDir(projectName, context);
                var contentAssemblyName = $"{projectName}.dll";
                var contentAssemblyFile = Path.Combine(binOutput, contentAssemblyName);
                var assemblyPath = Path.Combine(assembliesDir, contentAssemblyName);

                // Content assemblies should be moved into Resources/Assemblies/
                // and not be duplicated.
                if (!File.Exists(assemblyPath))
                {
                    File.Move(contentAssemblyFile, assemblyPath);
                }
                else if (File.Exists(contentAssemblyFile))
                {
                    File.Delete(contentAssemblyFile);
                }
            }
        }

        private void ZipAllFiles(ZipArchive zipArchive, string distribName, string entryPointOutput)
        {
            var enumOptions = new EnumerationOptions()
            {
                RecurseSubdirectories = true
            };

            foreach (var file in Directory.EnumerateFiles(entryPointOutput, "*", enumOptions))
            {
                zipArchive.CreateEntryFromFile(file, Path.Join(distribName, Path.GetRelativePath(entryPointOutput, file)));
            }
        }
    }
}