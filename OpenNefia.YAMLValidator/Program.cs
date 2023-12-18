using OpenNefia.Core;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameController;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Timing;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommandLine;
using OpenNefia.Core.Asynchronous;
using OpenNefia.Core.Serialization.Markdown;

namespace OpenNefia.YAMLValidator
{
    internal class Program
    {
        private class Options
        {
            [Option('w', "watch", Required = false, HelpText = "Watch for changes and show new errors.")]
            public bool Watch { get; set; }
        }

        private enum ErrorFormat
        {
            Emacs,
            Grouped,
            GitHubActions
        }

        [Dependency] private readonly IResourceManagerInternal _resources = default!;
        [Dependency] private readonly ITaskManager _taskManager = default!;
        [Dependency] private readonly IGameController _gameController = default!;

        private static int Main(string[] args)
        {
            return new Program().Run();
        }

        private int Run()
        {
            if (!Initialize())
                return -1;

            IoCManager.InjectDependencies(this);

            // return RunValidationAndPrint(ErrorFormat.GitHubActions);
            return WatchAndValidate();
        }

        private int WatchAndValidate()
        {
            bool finished = false;

            Console.CancelKeyPress += delegate (object? sender, ConsoleCancelEventArgs e)
            {
                e.Cancel = true;
                finished = true;
            };

            WatchResources();

            float dt = 0;

            while (!finished)
            {
                if (_reloadTime > 0f)
                {
                    _reloadTime -= dt;
                    if (_reloadTime < 0f)
                    {
                        Console.Clear();
                        RunValidationAndPrint(ErrorFormat.Grouped);
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(":: Watching for changes...");
                        Console.ResetColor();
                    }
                }
                dt = _gameController.StepFrame(stepInput: true);
            }
            return -1;
        }

        private static void InitIoC()
        {
            IoCManager.InitThread();
            IoCSetup.Register(DisplayMode.Headless);
            IoCManager.BuildGraph();

            RegisterReflection();
        }

        private static void RegisterReflection()
        {
            // Gets a handle to the shared and the current (client) dll.
            IoCManager.Resolve<IReflectionManager>().LoadAssemblies(new List<Assembly>(1)
            {
                typeof(OpenNefia.Core.Engine).Assembly,
                Assembly.GetExecutingAssembly()
            });
        }

        private bool Initialize()
        {
            InitIoC();

            var options = new GameControllerOptions()
            {
                ConfigOptionOverrides = new()
                {
                    { CVars.ReplAutoloadOnStartup, false },
                    { CVars.ReplAutoloadScript, string.Empty },
                }
            };

            var gc = IoCManager.Resolve<IGameController>();

            if (!gc.Startup(options))
            {
                Logger.Fatal("Failed to start game controller!");
                return false;
            }

            return true;
        }

        private readonly List<FileSystemWatcher> _watchers = new();
        private bool _doReload = true;
        private float _reloadTime = 0.1f;

        private void WatchResources()
        {
            foreach (var path in _resources.GetContentRoots().Select(r => r.ToString())
                .Where(r => Directory.Exists(r + "/Prototypes")).Select(p => p + "/Prototypes"))
            {
                var watcher = new FileSystemWatcher(path, "*.yml")
                {
                    IncludeSubdirectories = true,
                    NotifyFilter = NotifyFilters.LastWrite
                };

                watcher.Changed += (_, args) =>
                {
                    switch (args.ChangeType)
                    {
                        case WatcherChangeTypes.Renamed:
                        case WatcherChangeTypes.Deleted:
                            return;
                        case WatcherChangeTypes.Created:
                        // case WatcherChangeTypes.Deleted:
                        case WatcherChangeTypes.Changed:
                        case WatcherChangeTypes.All:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    _taskManager.RunOnMainThread(() =>
                    {
                        var file = new ResourcePath(args.FullPath);

                        foreach (var root in IoCManager.Resolve<IResourceManager>().GetContentRoots())
                        {
                            if (!file.TryRelativeTo(root, out var relative))
                            {
                                continue;
                            }

                            if (_reloadTime <= 0f)
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine(":: Changes detected, reloading...");
                                Console.ResetColor();
                            }

                            //_reloadQueue.Add(relative);
                            _reloadTime = 1f;
                        }
                    });
                };

                watcher.EnableRaisingEvents = true;
                _watchers.Add(watcher);
            }
        }

        private int RunValidationAndPrint(ErrorFormat format)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var errors = RunValidation();
            PrintErrors(errors, stopwatch.Elapsed, format);

            return errors.Count == 0 ? 0 : -1;
        }

        private record class YamlText(string Path, string Text);

        private void PrintErrors(Dictionary<string, HashSet<ErrorNode>> errors, TimeSpan elapsed, ErrorFormat format)
        {
            if (errors.Count == 0)
            {
                Console.WriteLine($"No errors found in {(int)elapsed.TotalMilliseconds} ms.");
                return;
            }

            var fileTexts = errors.Keys
                .Select(resPath =>
                {
                    var realPath = resPath;
                    if (_resources.TryGetDiskFilePath(new ResourcePath(resPath), out var diskPath))
                        return new YamlText(diskPath, File.ReadAllText(diskPath));
                    return null;
                })
                .WhereNotNull()
                .ToDictionary(p => p.Path, p => p.Text.Split("\n"));


            foreach (var (resPath, errorHashset) in errors)
            {
                PrintErrorGroup(resPath, errorHashset, format);
                foreach (var errorNode in errorHashset)
                {
                    var realPath = resPath;
                    if (_resources.TryGetDiskFilePath(new ResourcePath(resPath), out var diskPath))
                        realPath = diskPath;
                    PrintError(errorNode, realPath, fileTexts, format);
                }
            }

            Console.WriteLine($"{errors.Count} errors found in {(int)elapsed.TotalMilliseconds} ms.");
        }

        private void PrintErrorGroup(string resPath, HashSet<ErrorNode> errorHashset, ErrorFormat format)
        {
            switch (format)
            {
                default:
                    break;
                case ErrorFormat.Grouped:
                    var realPath = resPath;
                    if (_resources.TryGetDiskFilePath(new ResourcePath(resPath), out var diskPath))
                        realPath = diskPath;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"{realPath} ({errorHashset.Count} errors):");
                    break;
            }
        }

        private static void PrintError(ErrorNode errorNode, string realPath, Dictionary<string, string[]> fileTexts, ErrorFormat format)
        {
            switch (format)
            {
                case ErrorFormat.GitHubActions:
                default:
                    // This syntax is for interfacing with GitHub Actions.
                    // https://docs.github.com/en/actions/using-workflows/workflow-commands-for-github-actions#setting-an-error-message
                    Console.WriteLine($"::error file={realPath},line={errorNode.Node.Start.Line},col={errorNode.Node.Start.Column}::{errorNode.ErrorReason}");
                    break;
                case ErrorFormat.Emacs:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"{realPath}:{errorNode.Node.Start.Line}:{errorNode.Node.Start.Column}: {errorNode.ErrorReason}");
                    Console.ResetColor();
                    break;
                case ErrorFormat.Grouped:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"--> {errorNode.Node.Start.Line}:{errorNode.Node.Start.Column}: ");
                    Console.WriteLine(errorNode.ErrorReason);
                    Console.ResetColor();
                    if (fileTexts.TryGetValue(realPath, out var lines))
                    {
                        RenderDiagnostic(errorNode.Node, lines);
                    }
                    break;
            }
        }

        private static void RenderDiagnostic(DataNode node, string[] lines)
        {
            var context = 2;

            var leftGutter = new string[context * 2 + 1];
            var leftGutterPad = 0;

            for (var i = 0; i < context * 2 + 1; i++)
            {
                leftGutter[i] = (node.Start.Line - context + i).ToString();
                leftGutterPad = int.Max(leftGutter[i].Length, leftGutterPad);
            }

            for (var i = node.Start.Line - context; i < node.End.Line + context; i++)
            { 
                var line = lines[i - 1];
                var lineNumber = leftGutter[i - node.Start.Line + context];

                Console.ResetColor();
                Console.Write($"{lineNumber}{" ".Repeat(lineNumber.Length - leftGutterPad)} | ");
                if (i == node.Start.Line && node.Start.Column < node.End.Column)
                {
                    var span = line.AsSpan();
                    var before = span[0..(node.Start.Column-2)];
                    var middle = span[(node.Start.Column-2)..(node.End.Column-1)];
                    var after = span[(node.End.Column-1)..(span.Length-1)];

                    Console.Write(before.ToString());
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(middle.ToString());
                    Console.ResetColor();
                    Console.WriteLine(after.ToString().Trim());

                    Console.Write($"{" ".Repeat(leftGutterPad)} | ");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(" ".Repeat(node.Start.Column - 1));
                    Console.WriteLine("^".Repeat(node.End.Column - node.Start.Column));
                }
                else
                {
                    Console.WriteLine(line);
                }
            }
            Console.WriteLine();
        }

        private Dictionary<string, HashSet<ErrorNode>> Validate()
        {
            var errors = ValidatePrototypes();
            var mapErrors = ValidateMapBlueprints();

            foreach (var (file, errorSet) in mapErrors)
            {
                errors.GetOrInsertNew(file).AddRange(errorSet);
            }

            return errors;
        }

        private static Dictionary<string, HashSet<ErrorNode>> ValidatePrototypes()
        {
            var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
            return prototypeManager.ValidateDirectory(new ResourcePath("/Prototypes"));
        }

        private Dictionary<string, HashSet<ErrorNode>> ValidateMapBlueprints()
        {
            var mapLoader = IoCManager.Resolve<IMapLoader>();
            return mapLoader.ValidateDirectory(new ResourcePath("/Maps"));
        }

        public Dictionary<string, HashSet<ErrorNode>> RunValidation()
        {
            var allErrors = new Dictionary<string, HashSet<ErrorNode>>();

            var errors = Validate();

            foreach (var (key, val) in errors)
            {
                var newErrors = val.Where(n => n.AlwaysRelevant).ToHashSet();

                if (newErrors.Count == 0) continue;
                allErrors[key] = newErrors;
            }

            return allErrors;
        }
    }
}