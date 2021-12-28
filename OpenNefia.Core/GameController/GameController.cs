using System.Runtime;
using OpenNefia.Core.Asynchronous;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.DebugServer;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Timing;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.GameController
{
    public sealed class GameController : IGameController
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
        [Dependency] private readonly ITileDefinitionManagerInternal _tileDefinitionManager = default!;
        [Dependency] private readonly IUiLayerManager _uiLayers = default!;
        [Dependency] private readonly ILocalizationManager _localizationManager = default!;
        [Dependency] private readonly ITaskManager _taskManager = default!;
        [Dependency] private readonly ITimerManager _timerManager = default!;
        [Dependency] private readonly IMapRenderer _mapRenderer = default!;
        [Dependency] private readonly IDebugServer _debugServer = default!;
        [Dependency] private readonly ISaveGameManager _saveGameManager = default!;
        [Dependency] private readonly IThemeManager _themeManager = default!;

        public Action? MainCallback { get; set; } = null;

        public bool Startup()
        {
            SetupLogging(_logManager, () => new ConsoleLogHandler());

            _taskManager.Initialize();

            _modLoader.SetUseLoadContext(true);

            var userDataDir = "UserData";
            _resourceCache.Initialize(userDataDir);

            ProgramShared.DoMounts(_resourceCache);

            _graphics.Initialize();
            ShowSplashScreen();

            if (!_modLoader.TryLoadModulesFrom(new ResourcePath("/Assemblies"), string.Empty))
            {
                Logger.Fatal("Errors while loading content assemblies.");
                return false;
            }

            _serialization.Initialize();

            _uiLayers.Initialize();
            _modLoader.BroadcastRunLevel(ModRunLevel.PreInit);
            _modLoader.BroadcastRunLevel(ModRunLevel.Init);

            _entityManager.Initialize();

            if (!TryDownloadVanillaAssets())
            {
                return false;
            }

            _components.DoDefaultRegistrations();
            _components.DoAutoRegistrations();
            _components.FinishRegistration();

            _resourceCache.PreloadTextures();

            _themeManager.Initialize();
            _themeManager.LoadDirectory(ResourcePath.Root / "Themes");
            // _themeManager.SetActiveTheme("Beautify.Beautify");

            _prototypeManager.Initialize();
            _prototypeManager.LoadDirectory(ResourcePath.Root / "Prototypes");
            _prototypeManager.Resync();

            _assetManager.PreloadAssets();

            _tileDefinitionManager.Initialize();
            _tileDefinitionManager.RegisterAll();

            _localizationManager.Initialize(LanguagePrototypeOf.English);

            _saveGameManager.Initialize(userDataDir);

            _modLoader.BroadcastRunLevel(ModRunLevel.PostInit);

            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();

            return true;
        }

        private void ShowSplashScreen()
        {
            _graphics.ShowSplashScreen();
            SystemStep();
        }

        private bool TryDownloadVanillaAssets()
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

        internal static void SetupLogging(ILogManager logManager, Func<ILogHandler> logHandlerFactory)
        {
            logManager.RootSawmill.AddHandler(logHandlerFactory());

            logManager.GetSawmill("repl.exec").Level = LogLevel.Info;
        }

        /// <summary>
        /// This startup logic is heavier and only needed by the non-headless entry point.
        /// </summary>
        private void Stage2Startup()
        {
            _atlasManager.Initialize();
            _atlasManager.LoadAtlases();

            _mapRenderer.Initialize();
            _mapRenderer.RegisterTileLayers();

            _entityManager.Startup();

            _debugServer.Startup();
        }

        public void Run()
        {
            Stage2Startup();

            MainCallback?.Invoke();

            Cleanup();

            IoCManager.Clear();
        }

        private void Cleanup()
        {
            _entityManager.Shutdown();
            _uiLayers.Shutdown();
            _graphics.Shutdown();
            _debugServer.Shutdown();
            _themeManager.Shutdown();
            Logger.Log(LogLevel.Info, "Quitting game.");
            Environment.Exit(0);
        }

        public void Update(FrameEventArgs frame)
        {
            _taskManager.ProcessPendingTasks();
            _timerManager.UpdateTimers(frame);
            _uiLayers.UpdateLayers(frame);
            _taskManager.ProcessPendingTasks();
            _debugServer.CheckForRequests();
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

        internal enum DisplayMode : byte
        {
            Headless,
            Love,
        }
    }
}
