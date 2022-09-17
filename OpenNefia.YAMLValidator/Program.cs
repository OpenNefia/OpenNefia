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

namespace OpenNefia.YAMLValidator
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            return new Program().Run();
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

        private int Run()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            if (!Initialize())
                return -1;

            var errors = RunValidation();

            if (errors.Count == 0)
            {
                Console.WriteLine($"No errors found in {(int)stopwatch.Elapsed.TotalMilliseconds} ms.");
                return 0;
            }

            var res = IoCManager.Resolve<IResourceManagerInternal>(); 
            
            foreach (var (resPath, errorHashset) in errors)
            {
                foreach (var errorNode in errorHashset)
                {
                    var realPath = resPath;
                    if (res.TryGetDiskFilePath(new ResourcePath(resPath), out var diskPath))
                        realPath = diskPath;

                    // This syntax is for interfacing with GitHub Actions.
                    // https://docs.github.com/en/actions/using-workflows/workflow-commands-for-github-actions#setting-an-error-message
                    Console.WriteLine($"::error file={realPath},line={errorNode.Node.Start.Line},col={errorNode.Node.Start.Column}::{errorNode.ErrorReason}");
                }
            }

            Console.WriteLine($"{errors.Count} errors found in {(int)stopwatch.Elapsed.TotalMilliseconds} ms.");
            return -1;
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