using Cake.Core.Diagnostics;
using Cake.Frosting;
using System.IO.Compression;
using System.Diagnostics;
using Cake.Git;
using Cake.Core.IO;
using Path = System.IO.Path;

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
                throw new InvalidOperationException("Git repository root not found in any parent directory.");
            }

            if (Directory.Exists(Constants.OutputDir))
            {
                context.Log.Information("Clearing old output folder...");
                Directory.Delete(Constants.OutputDir, recursive: true);
            }

            Directory.CreateDirectory(Constants.OutputDir);

            var entryPointOutput = Utility.GetProjectOutputDir("OpenNefia.EntryPoint", context);

            var distribName = $"OpenNefia-{context.Runtime}";
            var zipName = $"{distribName}.zip";
            var zipPath = Path.Combine(Constants.OutputDir, zipName);

            using (var stream = File.OpenWrite(zipPath))
            {
                using (var zipArchive = new ZipArchive(stream, ZipArchiveMode.Create))
                {
                    context.Log.Information($"Copying resources...");
                    CopyResources(gitRoot, entryPointOutput);

                    context.Log.Information($"Packing files...");
                    ZipAllFiles(zipArchive, distribName, entryPointOutput);
                }
            }

            context.Log.Information($"Wrote .zip to {zipPath}");
        }

        private void CopyResources(DirectoryPath gitRoot, string entryPointOutput)
        {
            using var repository = new LibGit2Sharp.Repository(gitRoot.FullPath);

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

                    var repoRelFile = Path.GetRelativePath(gitRoot.FullPath, file).Replace("\\", "/");
                    Console.WriteLine($"{repoRelFile} {IsPathIgnored(repository, repoRelFile)}");
                    if (IsPathIgnored(repository, repoRelFile))
                        continue;

                    var relFile = Path.GetRelativePath(projectDir, file);
                    var outputFile = Path.Join(entryPointOutput, relFile);

                    var dir = Path.GetDirectoryName(outputFile)!;
                    if (!Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    if (!File.Exists(outputFile))
                        File.Copy(file, outputFile);
                }
            }
        }

        /// <summary>
        /// True if the path is ignored *and* it's not in the index.
        /// </summary>
        private bool IsPathIgnored(LibGit2Sharp.Repository repository, string file)
        {
            return repository.Ignore.IsPathIgnored(file) && repository.Index[file] == null;
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