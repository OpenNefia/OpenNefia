using System.Runtime;
using OpenNefia.Core.Asynchronous;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.GameController
{
    internal sealed class GameController : IGameController
    {
        [Dependency] private readonly IGraphics _graphics = default!;
        [Dependency] private readonly IResourceCacheInternal _resourceCache = default!;
        [Dependency] private readonly IAssetManager _assetManager = default!;
        [Dependency] private readonly ITileAtlasManager _atlasManager = default!;
        [Dependency] private readonly IModLoaderInternal _modLoader = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly ILogManager _logManager = default!;
        [Dependency] private readonly ISerializationManager _serialization = default!;
        [Dependency] private readonly IComponentFactory _components = default!;
        [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;
        [Dependency] private readonly IUiLayerManager _uiLayers = default!;
        [Dependency] private readonly ILocalizationManager _localizationManager = default!;
        [Dependency] private readonly ITaskManager _taskManager = default!;

        private IMapRenderer _mapRenderer = default!;
        private IMainTitleLogic _logic = default!;

        public bool Startup()
        {
            SetupLogging(_logManager, () => new ConsoleLogHandler());

            _taskManager.Initialize();

            _modLoader.SetUseLoadContext(true);

            if (!_modLoader.TryLoadModulesFrom(new ResourcePath("/Assemblies"), string.Empty))
            {
                Logger.Fatal("Errors while loading content assemblies.");
                return false;
            }

            _serialization.Initialize();

            _resourceCache.Initialize("UserData");
            _resourceCache.MountContentDirectory("../../../Assets");

            _graphics.Initialize();
            _uiLayers.Initialize();

            _modLoader.BroadcastRunLevel(ModRunLevel.PreInit);
            _modLoader.BroadcastRunLevel(ModRunLevel.Init);

            _entityManager.Initialize();

            if (!_tryDownloadVanillaAssets())
            {
                return false;
            }

            _components.DoDefaultRegistrations();
            _components.DoAutoRegistrations();
            _components.FinishRegistration();

            if (!RegisterLoveDependents())
            {
                return false;
            }

            _resourceCache.PreloadTextures();

            _prototypeManager.Initialize();
            _prototypeManager.LoadDirectory(ResourcePath.Root / "Prototypes");
            _prototypeManager.Resync();

            _assetManager.PreloadAssets();

            _initTileDefinitions();
            _atlasManager.LoadAtlases();
            _mapRenderer.RegisterTileLayers();

            _localizationManager.Initialize();

            _modLoader.BroadcastRunLevel(ModRunLevel.PostInit);

            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();

            return true;
        }

        private bool RegisterLoveDependents()
        {
            IoCSetup.RegisterLoveDependents();

            IoCManager.BuildGraph();

            _logic = IoCManager.Resolve<IMainTitleLogic>();
            _mapRenderer = IoCManager.Resolve<IMapRenderer>();

            return true;
        }

        private bool _tryDownloadVanillaAssets()
        {
            var downloader = new VanillaAssetsDownloader(_resourceCache);

            if (downloader.NeedsDownload())
            {
                var result = new MinimalProgressBarLayer(downloader).Query();
                if (!result.HasValue)
                {
                    Exception? ex = null;
                    if (result is UiResult<UiNoResult>.Error err)
                    {
                        ex = err.Exception;
                    }
                    Logger.Fatal($"Error downloading vanilla assets! {ex}");
                    return false;
                }
            }

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
            _logic.RunTitleScreen();
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
            if (Love.Graphics.IsActive())
            {
                Love.Vector4 backgroundColor = Love.Graphics.GetBackgroundColor();
                Love.Graphics.Clear(backgroundColor.r, backgroundColor.g, backgroundColor.b, backgroundColor.a);
                Love.Graphics.Origin();
                DoDraw();
                Love.Graphics.Present();
            }

            if (Love.Timer.IsLimitMaxFPS())
            {
                // Timer.SleepByMaxFPS();
            }
            else
            {
                Love.Timer.Sleep(0.001f);
            }
        }

        private void DoDraw()
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

            Love.Boot.SystemStep((Love.Scene)_graphics);
        }
    }
}
