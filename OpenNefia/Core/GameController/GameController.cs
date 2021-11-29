using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Asynchronous;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.UI;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.GameController
{
    internal sealed class GameController : IGameController
    {
        [Dependency] private readonly IGraphics _graphics = default!;
        [Dependency] private readonly IResourceCacheInternal _resourceCache = default!;
        [Dependency] private readonly IModLoaderInternal _modLoader = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly ITaskManager _taskManager = default!;
        [Dependency] private readonly ILogManager _logManager = default!;
        [Dependency] private readonly ISerializationManager _serialization = default!;
        [Dependency] private readonly IComponentFactory _components = default!;
        [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;
        [Dependency] private readonly IUiLayerManager _uiLayers = default!;

        public bool Startup()
        {
            SetupLogging(_logManager, () => new ConsoleLogHandler());

            _graphics.Initialize();

            _uiLayers.Initialize();

            _taskManager.Initialize();

            _modLoader.SetUseLoadContext(true);

            if (!_modLoader.TryLoadModulesFrom(new ResourcePath("/Assets/Assemblies"), string.Empty))
            {
                Logger.Fatal("Errors while loading content assemblies.");
                return false;
            }

            _serialization.Initialize();

            _resourceCache.Initialize("UserData");
            _resourceCache.MountContentDirectory("Assets");

            _modLoader.BroadcastRunLevel(ModRunLevel.PreInit);
            _modLoader.BroadcastRunLevel(ModRunLevel.Init);

            _components.DoAutoRegistrations();
            _components.FinishRegistration();

            _resourceCache.PreloadTextures();
            _entityManager.Initialize();

            _prototypeManager.Initialize();
            _prototypeManager.LoadDirectory(ResourcePath.Root / "Prototypes");
            _prototypeManager.Resync();

            _modLoader.BroadcastRunLevel(ModRunLevel.PostInit);

            _initTileDefinitions();

            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();

            return true;
        }

        private void _initTileDefinitions()
        {
            var prototypeList = new List<TilePrototype>();
            foreach (var tileDef in _prototypeManager.EnumeratePrototypes<TilePrototype>())
            {
                prototypeList.Add(tileDef);
            }

            // Ensure deterministic ordering for save files, etc.
            prototypeList.Sort((a, b) => string.Compare(a.ID, b.ID, StringComparison.Ordinal));

            foreach (var tileDef in prototypeList)
            {
                _tileDefinitionManager.Register(tileDef);
            }

            _tileDefinitionManager.Initialize();
        }

        internal static void SetupLogging(ILogManager logManager, Func<ILogHandler> logHandlerFactory)
        {
            logManager.RootSawmill.AddHandler(logHandlerFactory());
        }

        public void Run()
        {
            _entityManager.Startup();

            MainLoop();

            Cleanup();

            IoCManager.Clear();
        }

        private void MainLoop()
        {
            Logic.Go();
        }

        private void Cleanup()
        {
            _entityManager.Shutdown();
            _uiLayers.Shutdown();
            _graphics.Shutdown();
            Logger.Log(LogLevel.Info, "Quitting game.");
            Environment.Exit(0);
        }

        public void Update(float dt)
        {
            _uiLayers.UpdateLayers(dt);
        }

        public void Draw()
        {
            _graphics.BeginDraw();

            _uiLayers.DrawLayers();

            _graphics.EndDraw();
        }

        public void SystemStep()
        {
            if (Love.Boot.QuitFlag)
            {
                Cleanup();
            }

            Love.Boot.SystemStep(Scene);
        }
    }
}
