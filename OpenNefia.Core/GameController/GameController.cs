using System.Globalization;
using System.Runtime;
using OpenNefia.Core.Asynchronous;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.DebugServer;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.Input;
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
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.GameController
{
    public sealed partial class GameController : IGameController
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
        [Dependency] private readonly IUserInterfaceManagerInternal _uiManager = default!;
        [Dependency] private readonly ILocalizationManager _localizationManager = default!;
        [Dependency] private readonly ITaskManager _taskManager = default!;
        [Dependency] private readonly ITimerManager _timerManager = default!;
        [Dependency] private readonly IMapRenderer _mapRenderer = default!;
        [Dependency] private readonly IDebugServer _debugServer = default!;
        [Dependency] private readonly ISaveGameManager _saveGameManager = default!;
        [Dependency] private readonly IThemeManager _themeManager = default!;
        [Dependency] private readonly IFontManager _fontManager = default!;
        [Dependency] private readonly IInputManager _inputManager = default!;

        public Action? MainCallback { get; set; } = null;

        public bool Startup()
        {
            Console.OutputEncoding = EncodingHelpers.UTF8;
            SetupLogging(_logManager, () => new ConsoleLogHandler());

            _taskManager.Initialize();

            _modLoader.SetUseLoadContext(true);

            var userDataDir = "UserData";
            _resourceCache.Initialize(userDataDir);

            ProgramShared.DoMounts(_resourceCache);

            _fontManager.Initialize();

            _graphics.Initialize();
            ShowSplashScreen();
            BindWindowEvents();

            if (!_modLoader.TryLoadModulesFrom(new ResourcePath("/Assemblies"), string.Empty))
            {
                Logger.Fatal("Errors while loading content assemblies.");
                return false;
            }

            _serialization.Initialize();

            _uiManager.Initialize();
            _modLoader.BroadcastRunLevel(ModRunLevel.PreInit);
            _modLoader.BroadcastRunLevel(ModRunLevel.Init);

            _inputManager.Initialize();

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

            // TODO replace with config system
            var language = GetSystemLanguage();
            _localizationManager.Initialize(language);

            _saveGameManager.Initialize(userDataDir);

            _modLoader.BroadcastRunLevel(ModRunLevel.PostInit);

            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();

            return true;
        }

        private PrototypeId<LanguagePrototype> GetSystemLanguage()
        {
            var ci = CultureInfo.InstalledUICulture;
            var code = ci.Name.Replace('-', '_');

            if (code == "ja_JP")
                return LanguagePrototypeOf.Japanese;

            return LanguagePrototypeOf.English;
        }

        private void ShowSplashScreen()
        {
            _graphics.ShowSplashScreen();
            SystemStep();
        }

        private void BindWindowEvents()
        {
            _graphics.OnTextEditing += TextEditing;
            _graphics.OnTextInput += TextInput;
            _graphics.OnMouseMoved += MouseMoved;
            _graphics.OnMousePressed += MousePressed;
            _graphics.OnMouseReleased += MouseReleased;
            _graphics.OnKeyReleased += KeyUp;
            _graphics.OnKeyPressed += KeyDown;
            _graphics.OnMouseWheel += MouseWheel;
            _graphics.OnQuit += (_) =>
            {
                Shutdown();
                return false;
            };
        }

        private bool TryDownloadVanillaAssets()
        {
            var downloader = new VanillaAssetsDownloader(_resourceCache);

            if (downloader.NeedsDownload())
            {
                var result = _uiManager.Query(new MinimalProgressBarLayer(downloader));
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

            _entityManager.Startup();

            _mapRenderer.Initialize();
            _mapRenderer.RegisterTileLayers();

            _debugServer.Startup();
        }

        public void Run()
        {
            Stage2Startup();

            MainCallback?.Invoke();

            Shutdown();
        }

        private void Shutdown()
        {
            _entityManager.Shutdown();
            _uiManager.Shutdown();
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
            _inputManager.UpdateKeyRepeats(frame);
            _uiManager.UpdateLayers(frame);
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

            _uiManager.DrawLayers();

            _graphics.EndDraw();
        }

        public void SystemStep()
        {
            if (Love.Boot.QuitFlag)
            {
                Shutdown();
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
