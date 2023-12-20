using OpenNefia.Core;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.Containers;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.EngineVariables;
using OpenNefia.Core.GameController;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.HotReload;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Utility;
using System.Reflection;

namespace OpenNefia.Tests
{
    public interface IFullSimulationFactory
    {
        IFullSimulationFactory RegisterContentAssemblies(AssemblyLoadDelegate factory);
        IFullSimulationFactory RegisterComponents(CompRegistrationDelegate factory);
        IFullSimulationFactory RegisterDependencies(DiContainerDelegate factory);
        IFullSimulationFactory RegisterEntitySystems(EntitySystemRegistrationDelegate factory);
        IFullSimulationFactory RegisterPrototypes(PrototypeRegistrationDelegate factory);
        IFullSimulationFactory RegisterDataDefinitionTypes(DataDefinitionTypesRegistrationDelegate factory);
        IFullSimulationFactory LoadLocalizations(LocalizationLoadDelegate factory);
        IFullSimulationFactory LoadLocalizationsFromDisk();
        ISimulation InitializeInstance();
    }

    public delegate void AssemblyLoadDelegate(IList<Assembly> assemblies);

    /// <summary>
    /// Game simulation that loads all content types with reflection.
    /// Used for testing complex interactions between lots of components/systems
    /// without the need to guess which ones need to be specified.
    /// </summary>
    /// <remarks>
    /// For now, this is the closest analog to an integration test harness.
    /// </remarks>
    /// <seealso cref="GameSimulation"/>
    public class FullGameSimulation : ISimulation, IFullSimulationFactory
    {       
        // Required by the engine.
        private const string EmptyTile = @"
- type: Tile
  id: Empty
  image:
    filepath: /Graphic/Core/Tile/Default.png
  isSolid: false
  isOpaque: false
";

        private AssemblyLoadDelegate? _assemblyLoadDelegate;
        private DiContainerDelegate? _diFactory;
        private CompRegistrationDelegate? _regDelegate;
        private EntitySystemRegistrationDelegate? _systemDelegate;
        private PrototypeRegistrationDelegate? _protoDelegate;
        private EngineVariableRegistrationDelegate? _varDelegate;
        private DataDefinitionTypesRegistrationDelegate? _dataDefnTypesDelegate;
        private LocalizationLoadDelegate? _localizationLoadDelegate;
        private bool _loadLocalizationsFromDisk = false;

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
            var map = CreateMap(width, height);
            mapMan.SetActiveMap(map.Id);
            return map;
        }

        public IMap CreateMap(int width, int height)
        {
            var mapMan = Collection.Resolve<IMapManager>();
            var map = mapMan.CreateMap(width, height);
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

        private FullGameSimulation() { }

        public IFullSimulationFactory RegisterContentAssemblies(AssemblyLoadDelegate factory)
        {
            _assemblyLoadDelegate += factory;
            return this;
        }

        public IFullSimulationFactory RegisterDependencies(DiContainerDelegate factory)
        {
            _diFactory += factory;
            return this;
        }

        public IFullSimulationFactory RegisterComponents(CompRegistrationDelegate factory)
        {
            _regDelegate += factory;
            return this;
        }

        public IFullSimulationFactory RegisterEntitySystems(EntitySystemRegistrationDelegate factory)
        {
            _systemDelegate += factory;
            return this;
        }

        public IFullSimulationFactory RegisterPrototypes(PrototypeRegistrationDelegate factory)
        {
            _protoDelegate += factory;
            return this;
        }

        public IFullSimulationFactory RegisterEngineVariables(EngineVariableRegistrationDelegate factory)
        {
            _varDelegate += factory;
            return this;
        }

        public IFullSimulationFactory RegisterDataDefinitionTypes(DataDefinitionTypesRegistrationDelegate factory)
        {
            _dataDefnTypesDelegate += factory;
            return this;
        }

        public IFullSimulationFactory LoadLocalizations(LocalizationLoadDelegate factory)
        {
            _localizationLoadDelegate += factory;
            return this;
        }

        public IFullSimulationFactory LoadLocalizationsFromDisk()
        {
            _loadLocalizationsFromDisk = true;
            return this;
        }

        public ISimulation InitializeInstance()
        {
            var container = new DependencyCollection();
            Collection = container;

            IoCManager.InitThread(container, true);
            IoCSetup.Register(DisplayMode.Headless);

            UnitTestIoC.Setup(_loadLocalizationsFromDisk);

            _diFactory?.Invoke(container);
            container.BuildGraph();

            var assemblies = new List<Assembly>();

            assemblies.Add(typeof(OpenNefia.Core.Engine).Assembly);
            assemblies.Add(Assembly.GetExecutingAssembly());

            var contentAssemblies = new List<Assembly>();
            _assemblyLoadDelegate?.Invoke(contentAssemblies);

            var random = container.Resolve<IRandom>();
            random.PushSeed(0);


            var resMan = IoCManager.Resolve<IResourceManagerInternal>();
            Directory.CreateDirectory("Resources/");
            ProgramShared.DoCoreMounts(resMan);

            // Because of CVarDef, we have to load every one through reflection
            // just in case a system needs one of them.
            var configMan = container.Resolve<IConfigurationManagerInternal>();
            configMan.Initialize();
            foreach (var assembly in assemblies.Concat(contentAssemblies))
            {
                configMan.LoadCVarsFromAssembly(assembly);
            }

            IoCManager.Resolve<IReflectionManager>().LoadAssemblies(assemblies);

            var modLoader = IoCManager.Resolve<TestingModLoader>();
            modLoader.Assemblies = contentAssemblies.ToArray();
            modLoader.TryLoadModulesFrom(ResourcePath.Root, "");
            ProgramShared.DoContentMounts(resMan, contentAssemblies);

            var logMan = container.Resolve<ILogManager>();
            logMan.RootSawmill.AddHandler(new TestLogHandler("SIM"));

            var compFactory = container.Resolve<IComponentFactory>();
            compFactory.DoDefaultRegistrations();
            compFactory.DoAutoRegistrations();

            _regDelegate?.Invoke(compFactory);

            compFactory.FinishRegistration();

            var entityMan = container.Resolve<IEntityManager>();
            entityMan.Initialize();

            var entitySystemMan = container.Resolve<IEntitySystemManager>();

            _systemDelegate?.Invoke(entitySystemMan);

            var protoMan = container.Resolve<IPrototypeManagerInternal>();
            protoMan.Initialize();

            container.Resolve<ISerializationManager>().Initialize();

            protoMan.LoadDirectory(ResourcePath.Root / "Prototypes");
            _protoDelegate?.Invoke(protoMan);
            protoMan.ResolveResults();

            var varMan = container.Resolve<IEngineVariablesManagerInternal>();
            varMan.Initialize();
            varMan.LoadDirectory(ResourcePath.Root / "Variables");
            _varDelegate?.Invoke(varMan);

            entityMan.Startup();

            var mapManager = container.Resolve<IMapManagerInternal>();
            mapManager.CreateMap(1, 1, MapId.Global);

            var tileMan = container.Resolve<ITileDefinitionManagerInternal>();
            tileMan.RegisterAll();

            var locMan = container.Resolve<ILocalizationManager>();
            locMan.Initialize();
            _localizationLoadDelegate?.Invoke(locMan);
            locMan.Resync();

            var compLoc = container.Resolve<IComponentLocalizerInternal>();
            compLoc.Initialize();

            protoMan.RegisterEvents();

            var saveGameMan = container.Resolve<ISaveGameManager>();
            var save = new TempSaveGameHandle();
            saveGameMan.CurrentSave = save;

            return this;
        }

        public static IFullSimulationFactory NewSimulation()
        {
            return new FullGameSimulation();
        }
    }
}
