using Microsoft.Extensions.Options;
using OpenNefia.Core;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameController;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
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
            
            // TODO: Headless mode is not actually headless, because the game window has to be
            // initialized and visible to use Love.Graphics functions without crashing, and those
            // are called by initialization routines for things like the HUD, so the headless mode
            // currently has a hard dependency on a VM with a GPU (which GitHub Actions doesn't
            // offer).
            //
            // That is why the below code is duplicated from IGameController, only it avoids
            // initializing anything that touches Love.Graphics, like IHudLayer. Ideally all that
            // should be called here is IGameController.Startup() once a headless mode for Love2dCS
            // is figured out.

            var options = new GameControllerOptions();

            var _modLoader = IoCManager.Resolve<IModLoaderInternal>();
            var _resourceCache = IoCManager.Resolve<IResourceCacheInternal>();
            var _serialization = IoCManager.Resolve<ISerializationManager>();
            var _prototypes = IoCManager.Resolve<IPrototypeManagerInternal>();
            var _entityManager = IoCManager.Resolve<IEntityManagerInternal>();
            var _components = IoCManager.Resolve<IComponentFactory>();
            var _config = IoCManager.Resolve<IConfigurationManagerInternal>();

            _resourceCache.Initialize(options.UserDataDirectoryName);
            
            _modLoader.SetUseLoadContext(true);
            ProgramShared.DoMounts(_resourceCache);

            if (!_modLoader.TryLoadModulesFrom(options.AssemblyDirectory, string.Empty))
            {
                Logger.Fatal("Errors while loading content assemblies.");
                return false;
            }

            _config.Initialize();
            _config.LoadCVarsFromAssembly(typeof(GameController).Assembly);
            foreach (var loadedModule in _modLoader.LoadedModules)
            {
                _config.LoadCVarsFromAssembly(loadedModule);
            }

            _serialization.Initialize();
            _entityManager.Initialize(); 
            
            _components.DoDefaultRegistrations();
            _components.DoAutoRegistrations();
            _components.FinishRegistration();

            _prototypes.Initialize();
            _prototypes.LoadDirectory(options.PrototypeDirectory);
            _prototypes.Resync();

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
                    Console.WriteLine($"::error file={realPath},line={errorNode.Node.Start.Line},col={errorNode.Node.Start.Column}::{resPath}({errorNode.Node.Start.Line},{errorNode.Node.Start.Column})  {errorNode.ErrorReason}");
                }
            }

            Console.WriteLine($"{errors.Count} errors found in {(int)stopwatch.Elapsed.TotalMilliseconds} ms.");
            return -1;
        }

        private Dictionary<string, HashSet<ErrorNode>> Validate()
        {
            var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
            return prototypeManager.ValidateDirectory(new ResourcePath("/Prototypes"));
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