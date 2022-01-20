using Cake.Core.Diagnostics;
using Cake.Frosting;
using System.IO.Compression;
using System.Diagnostics;
using Cake.Git;

namespace OpenNefia.Packaging.Tasks
{
    [TaskName("PackageFullRelease")]
    [IsDependentOn(typeof(BuildFullReleaseTask))]
    public sealed class PackageFullReleaseTask : FrostingTask<BuildContext>
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

        public override void Run(BuildContext context)
        {
            var gitRoot = context.GitFindRootFromPath(context.Environment.WorkingDirectory);
            if (gitRoot == null)
            {
                context.Log.Error("Git repository root not found in any parent directory.");
                return;
            }

            if (Directory.Exists(Constants.OutputDir))
            {
                context.Log.Information("Clearing old output folder...");
                Directory.Delete(Constants.OutputDir, recursive: true);
            }

            Directory.CreateDirectory(Constants.OutputDir);

            var entryPointOutput = Utility.GetProjectOutputDir("OpenNefia.EntryPoint", context);

            var commitHash = context.GitCommitHash(gitRoot);

            var distribName = $"OpenNefia-{commitHash}-{context.Runtime}";
            var zipName = $"{distribName}.zip";
            var zipPath = Path.Combine(Constants.OutputDir, zipName);

            using (var stream = File.OpenWrite(zipPath))
            {
                using (var zipArchive = new ZipArchive(stream, ZipArchiveMode.Create))
                {
                    context.Log.Information($"Copying resources...");
                    CopyResources(entryPointOutput);

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