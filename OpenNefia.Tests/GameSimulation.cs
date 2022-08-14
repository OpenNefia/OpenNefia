using JetBrains.Annotations;
using Moq;
using OpenNefia.Core;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Asynchronous;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.Containers;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.Exceptions;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.HotReload;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Profiles;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Tests
{
    public interface ISimulationFactory
    {
        ISimulationFactory RegisterComponents(CompRegistrationDelegate factory);
        ISimulationFactory RegisterDependencies(DiContainerDelegate factory);
        ISimulationFactory RegisterEntitySystems(EntitySystemRegistrationDelegate factory);
        ISimulationFactory RegisterPrototypes(PrototypeRegistrationDelegate factory);
        ISimulationFactory RegisterDataDefinitionTypes(DataDefinitionTypesRegistrationDelegate factory);
        ISimulationFactory LoadAssemblies(LoadAssembliesDelegate factory);
        ISimulationFactory LoadLocalizations(LocalizationLoadDelegate factory);
        ISimulation InitializeInstance();
    }

    public interface ISimulation
    {
        IDependencyCollection Collection { get; }

        /// <summary>
        /// Resolves a dependency directly out of IoC collection.
        /// </summary>
        T Resolve<T>();

        T GetEntitySystem<T>() where T : IEntitySystem;

        /// <summary>
        /// Creates a map and sets it as active.
        /// </summary>
        IMap CreateMapAndSetActive(int width, int height);

        EntityUid SpawnEntity(PrototypeId<EntityPrototype>? protoId, MapCoordinates coordinates);

        EntityUid SpawnEntity(PrototypeId<EntityPrototype>? protoId, EntityCoordinates coordinates);

        IMap? ActiveMap { get; }
    }

    public delegate void DiContainerDelegate(IDependencyCollection diContainer);

    public delegate void CompRegistrationDelegate(IComponentFactory factory);

    public delegate void EntitySystemRegistrationDelegate(IEntitySystemManager systemMan);

    public delegate void PrototypeRegistrationDelegate(IPrototypeManager protoMan);

    public delegate void DataDefinitionTypesRegistrationDelegate(List<Type> types);

    public delegate void LoadAssembliesDelegate(List<Assembly> assemblies);

    public delegate void LocalizationLoadDelegate(ILocalizationManager locMan);

    /// <summary>
    /// Game simulation for minimal testing. All components/prototypes must
    /// be loaded manually with this class.
    /// </summary>
    public class GameSimulation : ISimulation, ISimulationFactory
    {
        // Required by the engine.
        private const string EmptyTile = @"
- type: Tile
  id: Empty
  isSolid: false
  isOpaque: false
";

        private DiContainerDelegate? _diFactory;
        private CompRegistrationDelegate? _regDelegate;
        private EntitySystemRegistrationDelegate? _systemDelegate;
        private PrototypeRegistrationDelegate? _protoDelegate;
        private DataDefinitionTypesRegistrationDelegate? _dataDefnTypesDelegate;
        private LoadAssembliesDelegate? _assembliesLoadDelegate;
        private LocalizationLoadDelegate? _localizationLoadDelegate;

        public IDependencyCollection Collection { get; private set; } = default!;

        public IMap? ActiveMap => Resolve<IMapManager>().ActiveMap;

        public T Resolve<T>()
        {
            return Collection.Resolve<T>();
        }

        public T GetEntitySystem<T>()
            where T : IEntitySystem
        {
            return Resolve<IEntitySystemManager>().GetEntitySystem<T>();
        }

        public IMap CreateMapAndSetActive(int width, int height)
        {
            var mapMan = Collection.Resolve<IMapManager>();
            var map = mapMan.CreateMap(width, height);
            mapMan.SetActiveMap(map.Id);
            return map;
        }

        public EntityUid SpawnEntity(PrototypeId<EntityPrototype>? protoId, MapCoordinates coordinates)
        {
            var entMan = Collection.Resolve<IEntityManager>();
            return entMan.SpawnEntity(protoId, coordinates);
        }

        public EntityUid SpawnEntity(PrototypeId<EntityPrototype>? protoId, EntityCoordinates coordinates)
        {
            var entMan = Collection.Resolve<IEntityManager>();
            return entMan.SpawnEntity(protoId, coordinates);
        }

        private GameSimulation() { }

        public ISimulationFactory RegisterDependencies(DiContainerDelegate factory)
        {
            _diFactory += factory;
            return this;
        }

        public ISimulationFactory RegisterComponents(CompRegistrationDelegate factory)
        {
            _regDelegate += factory;
            return this;
        }

        public ISimulationFactory RegisterEntitySystems(EntitySystemRegistrationDelegate factory)
        {
            _systemDelegate += factory;
            return this;
        }

        public ISimulationFactory RegisterPrototypes(PrototypeRegistrationDelegate factory)
        {
            _protoDelegate += factory;
            return this;
        }

        public ISimulationFactory RegisterDataDefinitionTypes(DataDefinitionTypesRegistrationDelegate factory)
        {
            _dataDefnTypesDelegate += factory;
            return this;
        }

        public ISimulationFactory LoadAssemblies(LoadAssembliesDelegate factory)
        {
            _assembliesLoadDelegate += factory;
            return this;
        }

        public ISimulationFactory LoadLocalizations(LocalizationLoadDelegate factory)
        {
            _localizationLoadDelegate += factory;
            return this;
        }

        public ISimulation InitializeInstance()
        {
            var container = new DependencyCollection();
            Collection = container;

            IoCManager.InitThread(container, true);

            //Tier 1: System
            container.Register<ILogManager, LogManager>();
            container.Register<IRuntimeLog, RuntimeLog>();
            container.Register<IConfigurationManager, ConfigurationManager>();
            container.Register<IConfigurationManagerInternal, ConfigurationManager>();
            container.Register<IDynamicTypeFactory, DynamicTypeFactory>();
            container.Register<IDynamicTypeFactoryInternal, DynamicTypeFactory>();
            container.Register<ILocalizationManager, DummyLocalizationManager>();
            container.Register<IModLoader, TestingModLoader>();
            container.Register<IModLoaderInternal, TestingModLoader>();
            container.Register<ITaskManager, TaskManager>();
            container.Register<IHotReloadWatcher, DummyHotReloadWatcher>();
            container.Register<IHotReloadWatcherInternal, DummyHotReloadWatcher>();

            var mockUIManager = new Mock<IUserInterfaceManager>();
            mockUIManager.Setup(m => m.ActiveLayers).Returns(new List<UiLayer>());
            container.RegisterInstance<IUserInterfaceManager>(mockUIManager.Object);
            container.RegisterInstance<IGraphics>(new Mock<IGraphics>().Object);

            var assemblies = new List<Assembly>(1)
            {
                typeof(OpenNefia.Core.Engine).Assembly,
            };

            _assembliesLoadDelegate?.Invoke(assemblies);

            var realReflection = new ReflectionManager();
            realReflection.LoadAssemblies(assemblies);

            var reflectionManager = new Mock<IReflectionManager>();
            reflectionManager
                .Setup(x => x.FindTypesWithAttribute<MeansDataDefinitionAttribute>())
                .Returns(() => new[]
                {
                    typeof(DataDefinitionAttribute)
                });

            var dataDefinitionTypes = new List<Type>()
            {
                typeof(EntityPrototype),
                typeof(TilePrototype),
                typeof(SpatialComponent),
                typeof(MetaDataComponent)
            };
            _dataDefnTypesDelegate?.Invoke(dataDefinitionTypes);

            reflectionManager
                .Setup(x => x.FindTypesWithAttribute<TypeSerializerAttribute>())
                .Returns(() => realReflection.FindTypesWithAttribute<TypeSerializerAttribute>());

            reflectionManager
                .Setup(x => x.FindTypesWithAttribute<RegisterLocaleFunctionsAttribute>())
                .Returns(() => realReflection.FindTypesWithAttribute<RegisterLocaleFunctionsAttribute>());

            container.RegisterInstance<IReflectionManager>(reflectionManager.Object); // tests should not be searching for types
            container.RegisterInstance<IResourceManager>(new Mock<IResourceManager>().Object); // no disk access for tests

            //Tier 2: Simulation
            container.Register<IEntityManager, EntityManagerInternal>();
            container.Register<IEntityManagerInternal, EntityManagerInternal>();
            container.Register<IEntityFactory, EntityFactory>();
            container.Register<IEntityFactoryInternal, EntityFactory>();
            container.Register<IMapManager, MapManager>();
            container.Register<IMapManagerInternal, MapManager>();
            container.Register<IAreaManager, AreaManager>();
            container.Register<IAreaManagerInternal, AreaManager>();
            container.Register<ISerializationManager, SerializationManager>();
            container.Register<IPrototypeManager, PrototypeManager>();
            container.Register<IPrototypeManagerInternal, PrototypeManager>();
            container.Register<IComponentFactory, ComponentFactory>();
            container.Register<IComponentDependencyManager, ComponentDependencyManager>();
            container.Register<IComponentLocalizer, ComponentLocalizer>();
            container.Register<IComponentLocalizerInternal, ComponentLocalizer>();
            container.Register<IEntitySystemManager, EntitySystemManager>();
            container.Register<IGameSessionManager, GameSessionManager>();
            container.Register<ICoords, OrthographicCoords>();
            container.Register<IRandom, SysRandom>();
            container.Register<ITileDefinitionManager, TileDefinitionManager>();
            container.Register<ITileDefinitionManagerInternal, TileDefinitionManager>();
            container.Register<ISaveGameSerializer, SaveGameSerializer>();
            container.Register<ISaveGameSerializerInternal, SaveGameSerializer>();
            container.Register<ISaveGameManager, SaveGameManager>();
            container.Register<ISaveGameManagerInternal, SaveGameManager>();
            container.Register<IProfileManager, ProfileManager>();
            container.Register<IMapLoader, MapLoader>();

            container.RegisterInstance<IAudioManager>(new Mock<IAudioManager>().Object);

            _diFactory?.Invoke(container);
            container.BuildGraph();

            var random = container.Resolve<IRandom>();
            random.PushSeed(0);

            // Because of CVarDef, we have to load every one through reflection
            // just in case a system needs one of them.
            var configMan = container.Resolve<IConfigurationManagerInternal>();
            configMan.Initialize();
            configMan.LoadCVarsFromAssembly(typeof(Engine).Assembly); // Core
            configMan.LoadCVarsFromAssembly(typeof(GameSimulation).Assembly); // Tests

            var logMan = container.Resolve<ILogManager>();
            logMan.RootSawmill.AddHandler(new TestLogHandler("SIM"));

            var compFactory = container.Resolve<IComponentFactory>();
            compFactory.DoDefaultRegistrations();

            _regDelegate?.Invoke(compFactory);

            compFactory.FinishRegistration();

            reflectionManager
                .Setup(x => x.FindTypesWithAttribute(typeof(DataDefinitionAttribute)))
                .Returns(() => dataDefinitionTypes.Union(compFactory.AllRegisteredTypes));

            var entityMan = container.Resolve<IEntityManager>();
            entityMan.Initialize();

            var compLocalizer = container.Resolve<IComponentLocalizerInternal>();
            compLocalizer.Initialize();

            var entitySystemMan = container.Resolve<IEntitySystemManager>();

            entitySystemMan.LoadExtraSystemType<SpatialSystem>();
            entitySystemMan.LoadExtraSystemType<EntityLookup>();
            entitySystemMan.LoadExtraSystemType<ContainerSystem>();
            entitySystemMan.LoadExtraSystemType<StackSystem>();
            entitySystemMan.LoadExtraSystemType<MapSystem>();

            _systemDelegate?.Invoke(entitySystemMan);

            reflectionManager
                .Setup(x => x.FindAllTypes())
                .Returns(() => realReflection.FindAllTypes()

                // This is to support IGameSaveSerializer.
                .Union(entitySystemMan.SystemTypes)

                .Union(dataDefinitionTypes)
                .Union(compFactory.AllRegisteredTypes));

            entityMan.Startup();

            var mapManager = container.Resolve<IMapManagerInternal>();
            mapManager.CreateMap(1, 1, MapId.Global);

            container.Resolve<ISerializationManager>().Initialize();

            var protoMan = container.Resolve<IPrototypeManager>();
            protoMan.RegisterType<EntityPrototype>();
            protoMan.RegisterType<TilePrototype>();
            protoMan.LoadString(EmptyTile);
            _protoDelegate?.Invoke(protoMan);
            protoMan.Resync();

            var tileMan = container.Resolve<ITileDefinitionManagerInternal>();
            tileMan.RegisterAll();

            var saveGameSer = container.Resolve<ISaveGameSerializerInternal>();
            saveGameSer.Initialize();

            var locMan = container.Resolve<ILocalizationManager>();
            locMan.Initialize();
            _localizationLoadDelegate?.Invoke(locMan);
            locMan.Resync();

            return this;
        }

        public static ISimulationFactory NewSimulation()
        {
            return new GameSimulation();
        }
    }
}
