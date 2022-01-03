using OpenNefia.Core.Asynchronous;
using OpenNefia.Core.Audio;
using OpenNefia.Core.CommandLine;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.DebugServer;
using OpenNefia.Core.Exceptions;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameController;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.Input;
using OpenNefia.Core.Input.Client;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Profiles;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Timing;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.UserInterface;
using PrettyPrompt.Consoles;

namespace OpenNefia
{
    public class IoCSetup
    {
        internal static void Register(GameController.DisplayMode mode)
        {
            switch (mode)
            {
                case GameController.DisplayMode.Headless:
                    IoCManager.Register<IGraphics, HeadlessGraphics>();
                    IoCManager.Register<IInputManager, InputManager>();
                    IoCManager.Register<ITaskRunner, HeadlessTaskRunner>();
                    break;
                case GameController.DisplayMode.Love:
                    IoCManager.Register<IGraphics, LoveGraphics>();
                    IoCManager.Register<IInputManager, LoveInputManager>();
                    IoCManager.Register<ITaskRunner, LoveTaskRunner>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            IoCManager.Register<IRuntimeLog, RuntimeLog>();
            IoCManager.Register<ILogManager, LogManager>();
            IoCManager.Register<IDynamicTypeFactory, DynamicTypeFactory>();
            IoCManager.Register<IDynamicTypeFactoryInternal, DynamicTypeFactory>();
            IoCManager.Register<IEntitySystemManager, EntitySystemManager>();
            IoCManager.Register<IReflectionManager, ReflectionManager>();
            IoCManager.Register<IMapManager, MapManager>();
            IoCManager.Register<IComponentDependencyManager, ComponentDependencyManager>();
            IoCManager.Register<IComponentFactory, ComponentFactory>();
            IoCManager.Register<IPrototypeManager, PrototypeManager>();
            IoCManager.Register<IResourceManager, ResourceCache>();
            IoCManager.Register<IResourceManagerInternal, ResourceCache>();
            IoCManager.Register<IResourceCache, ResourceCache>();
            IoCManager.Register<IResourceCacheInternal, ResourceCache>();
            IoCManager.Register<IModLoader, ModLoader>();
            IoCManager.Register<IModLoaderInternal, ModLoader>();
            IoCManager.Register<ITileDefinitionManager, TileDefinitionManager>();
            IoCManager.Register<ITileDefinitionManagerInternal, TileDefinitionManager>();
            IoCManager.Register<IEntityManager, EntityManagerInternal>();
            IoCManager.Register<IEntityManagerInternal, EntityManagerInternal>();
            IoCManager.Register<IEntityFactory, EntityFactory>();
            IoCManager.Register<IEntityFactoryInternal, EntityFactory>();
            IoCManager.Register<ISerializationManager, SerializationManager>();
            IoCManager.Register<IAssetManager, AssetManager>();
            IoCManager.Register<ITileAtlasManager, TileAtlasManager>();
            IoCManager.Register<IUserInterfaceManager, UserInterfaceManager>();
            IoCManager.Register<IUserInterfaceManagerInternal, UserInterfaceManager>();
            IoCManager.Register<IRandom, SysRandom>();
            IoCManager.Register<IFontManager, FontManager>();
            IoCManager.Register<ILocalizationManager, LocalizationManager>();
            IoCManager.Register<ITaskManager, TaskManager>();
            IoCManager.Register<IGameSessionManager, GameSessionManager>();
            IoCManager.Register<IGameController, GameController>();
            IoCManager.Register<ICoords, OrthographicCoords>();
            IoCManager.Register<IMapRenderer, MapRenderer>();
            IoCManager.Register<IMapDrawables, MapDrawables>();
            IoCManager.Register<IMusicManager, LoveMusicManager>();
            IoCManager.Register<ITimerManager, TimerManager>();
            IoCManager.Register<IMapBlueprintLoader, MapBlueprintLoader>();
            IoCManager.Register<ICommandLineController, CommandLineController>();
            IoCManager.Register<IReplExecutor, CSharpReplExecutor>();
            IoCManager.Register<IConsole, DummyConsole>();
            IoCManager.Register<IDebugServer, DebugServer>();
            IoCManager.Register<ISaveGameManager, SaveGameManager>();
            IoCManager.Register<ISaveGameManagerInternal, SaveGameManager>();
            IoCManager.Register<IThemeManager, ThemeManager>();
            IoCManager.Register<IProfileManager, ProfileManager>();
        }
    }
}