using System.Globalization;
using System.Runtime;
using Microsoft.Extensions.Options;
using OpenNefia.Core.Asynchronous;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.Console;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Profiles;
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
        [Dependency] private readonly IConfigurationManagerInternal _config = default!;
        [Dependency] private readonly IGraphics _graphics = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IMusicManager _music = default!;
        [Dependency] private readonly IResourceCacheInternal _resourceCache = default!;
        [Dependency] private readonly IAssetManager _assetManager = default!;
        [Dependency] private readonly ITileAtlasManager _atlasManager = default!;
        [Dependency] private readonly IModLoaderInternal _modLoader = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly ILogManager _log = default!;
        [Dependency] private readonly ISerializationManager _serialization = default!;
        [Dependency] private readonly IComponentFactory _components = default!;
        [Dependency] private readonly ITileDefinitionManagerInternal _tileDefinitionManager = default!;
        [Dependency] private readonly IUserInterfaceManagerInternal _uiManager = default!;
        [Dependency] private readonly ILocalizationManager _localizationManager = default!;
        [Dependency] private readonly ITaskManager _taskManager = default!;
        [Dependency] private readonly ITimerManager _timerManager = default!;
        [Dependency] private readonly IMapRenderer _mapRenderer = default!;
        [Dependency] private readonly IThemeManager _themeManager = default!;
        [Dependency] private readonly IFontManager _fontManager = default!;
        [Dependency] private readonly IInputManager _inputManager = default!;
        [Dependency] private readonly IProfileManager _profileManager = default!;
        [Dependency] private readonly ISaveGameManagerInternal _saveGameManager = default!;
        [Dependency] private readonly ISaveGameSerializerInternal _saveGameSerializer = default!;

        public Action? MainCallback { get; set; } = null;
        private ILogHandler? _logHandler;
        public GameControllerOptions Options { get; private set; } = new();

        private bool _running = true;

        public bool Startup(GameControllerOptions options)
        {
            Options = options;
            System.Console.OutputEncoding = EncodingHelpers.UTF8;

            _resourceCache.Initialize(Options.UserDataDirectoryName);
            _profileManager.Initialize();

            InitializeConfig();
            SetupLogging(() => new ConsoleLogHandler());

            _taskManager.Initialize();

            _modLoader.SetUseLoadContext(true);
            ProgramShared.DoMounts(_resourceCache);

            _fontManager.Initialize();

            BindWindowEvents();
            _graphics.Initialize();
            ShowSplashScreen();

            _audio.Initialize();
            _music.Initialize();

            if (!_modLoader.TryLoadModulesFrom(Options.AssemblyDirectory, string.Empty))
            {
                Logger.Fatal("Errors while loading content assemblies.");
                return false;
            }

            foreach (var loadedModule in _modLoader.LoadedModules)
            {
                _config.LoadCVarsFromAssembly(loadedModule);
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
            _themeManager.LoadDirectory(Options.ThemeDirectory);
            // _themeManager.SetActiveTheme("Beautify.Beautify");

            _prototypeManager.Initialize();
            _prototypeManager.LoadDirectory(Options.PrototypeDirectory);
            _prototypeManager.Resync();

            _assetManager.PreloadAssets();

            _tileDefinitionManager.Initialize();
            _tileDefinitionManager.RegisterAll();

            _localizationManager.Initialize();

            _saveGameManager.Initialize();

            _atlasManager.Initialize();
            _atlasManager.LoadAtlases();

            _entityManager.Startup();

            _saveGameSerializer.Initialize();

            _mapRenderer.Initialize();
            _mapRenderer.RegisterTileLayers();

            _modLoader.BroadcastRunLevel(ModRunLevel.PostInit);

            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();

            return true;
        }

        private void InitializeConfig()
        {
            _config.Initialize();
            
            // Load our own (non-mod) CVars.
            _config.LoadCVarsFromAssembly(typeof(GameController).Assembly);

            if (Options.LoadConfigAndUserData)
            {
                var configFile = ResourcePath.Root / Options.ConfigFileName;
                if (_profileManager.CurrentProfile.Exists(configFile))
                {
                    // Load config from user data if available.
                    _config.LoadFromFile(configFile);
                }
                else
                {
                    // Else we just use code-defined defaults and let it save to file when the user changes things.
                    _config.SetSaveFile(configFile);
                }
            }

            _config.OverrideConVars(EnvironmentVariables.GetEnvironmentCVars());
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
                DoShutdown();
                return false;
            };
        }

        private bool TryDownloadVanillaAssets()
        {
            var downloader = new VanillaAssetsDownloader(_resourceCache);

            if (downloader.NeedsDownload())
            {
                var layer = new MinimalProgressBarLayer();
                layer.Initialize(downloader);
                var result = _uiManager.Query(layer);
                if (!result.HasValue)
                {
                    Exception? ex = null;
                    if (result is UiResult<UINone>.Error err)
                    {
                        ex = err.Exception;
                    }
                    Logger.Fatal($"Error downloading vanilla assets! {ex}");
                    return false;
                }
            }

            return true;
        }

        internal void SetupLogging(Func<ILogHandler> logHandlerFactory)
        {
            var logHandler = logHandlerFactory() ?? null;

            var logEnabled = _config.GetCVar(CVars.LogEnabled);

            if (logEnabled && logHandler == null)
            {
                var logPath = _config.GetCVar(CVars.LogPath);
                var logFormat = _config.GetCVar(CVars.LogFormat);
                var logFilename = logFormat.Replace("%(date)s", DateTime.Now.ToString("yyyy-MM-dd"))
                    .Replace("%(time)s", DateTime.Now.ToString("hh-mm-ss"));
                var fullPath = Path.Combine(logPath, logFilename);

                if (!Path.IsPathRooted(fullPath))
                {
                    logPath = PathHelpers.ExecutableRelativeFile(fullPath);
                }

                logHandler = new FileLogHandler(logPath);
            }

            _log.RootSawmill.Level = _config.GetCVar(CVars.LogLevel);

            if (logEnabled && logHandler != null)
            {
                _logHandler = logHandler;
                _log.RootSawmill.AddHandler(_logHandler!);
            }

            _log.GetSawmill("repl.exec").Level = LogLevel.Info;
            _log.GetSawmill("go.sys").Level = LogLevel.Info;
            _log.GetSawmill("input.binding").Level = LogLevel.Info;
            _log.GetSawmill("ai.vanilla").Level = LogLevel.Info;
            _log.GetSawmill("ui.input").Level = LogLevel.Info;
        }

        public void Run()
        {
            _running = true;

            MainCallback?.Invoke();

            DoShutdown();
        }

        public void Shutdown()
        {
            // Already got shut down I assume,
            if (!_running)
                return;

            Logger.Info("Shutting down!");

            _running = false;
        }

        private void DoShutdown()
        {
            _entityManager.Shutdown();
            _uiManager.Shutdown();
            _graphics.Shutdown();
            _audio.Shutdown();
            _music.Shutdown();
            _themeManager.Shutdown();
            if (_logHandler != null)
            {
                _log.RootSawmill.RemoveHandler(_logHandler);
                (_logHandler as IDisposable)?.Dispose();
            }
            Logger.Log(LogLevel.Info, "Quitting game.");
            Environment.Exit(0);
        }

        public void Update(FrameEventArgs frame)
        {
            // TODO: Make this not rely on hacks somehow?
            if (!_running)
            {
                DoShutdown();
            }

            _taskManager.ProcessPendingTasks();
            _timerManager.UpdateTimers(frame);
            _inputManager.UpdateKeyRepeats(frame);
            _uiManager.UpdateLayers(frame);
            _taskManager.ProcessPendingTasks();
        }

        public void Draw()
        {
            if (Love.Graphics.IsActive())
            {
                Love.Vector4 backgroundColor = Love.Graphics.GetBackgroundColor();
                Love.Graphics.Clear(backgroundColor.r, backgroundColor.g, backgroundColor.b, backgroundColor.a);
                Love.Graphics.SetScissor();
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
                DoShutdown();
            }

            Love.Boot.SystemStep((Love.Scene)_graphics);
        }
    }
}
