using OpenNefia.Core;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.Containers;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameController;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Result;
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
        private static ThreadLocal<PrototypeManagerCache> _prototypeManagerCache = new();

        private AssemblyLoadDelegate? _assemblyLoadDelegate;
        private DiContainerDelegate? _diFactory;
        private CompRegistrationDelegate? _regDelegate;
        private EntitySystemRegistrationDelegate? _systemDelegate;
        private PrototypeRegistrationDelegate? _protoDelegate;
        private DataDefinitionTypesRegistrationDelegate? _dataDefnTypesDelegate;

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

        public IFullSimulationFactory RegisterDataDefinitionTypes(DataDefinitionTypesRegistrationDelegate factory)
        {
            _dataDefnTypesDelegate += factory;
            return this;
        }

        public ISimulation InitializeInstance()
        {
            var container = new DependencyCollection();
            Collection = container;

            IoCManager.InitThread(container, true);
            IoCSetup.Register(DisplayMode.Headless);

            IoCManager.Register<IModLoader, TestingModLoader>(overwrite: true);
            IoCManager.Register<IModLoaderInternal, TestingModLoader>(overwrite: true);
            IoCManager.Register<TestingModLoader, TestingModLoader>(overwrite: true);
            IoCManager.Register<ILocalizationManager, DummyLocalizationManager>(overwrite: true);

            _diFactory?.Invoke(container);
            container.BuildGraph();

            var assemblies = new List<Assembly>();

            assemblies.Add(typeof(OpenNefia.Core.Engine).Assembly);
            assemblies.Add(Assembly.GetExecutingAssembly());

            var contentAssemblies = new List<Assembly>();
            _assemblyLoadDelegate?.Invoke(contentAssemblies);

            var random = container.Resolve<IRandom>();
            random.PushSeed(0);

            // Because of CVarDef, we have to load every one through reflection
            // just in case a system needs one of them.
            var configMan = container.Resolve<IConfigurationManagerInternal>();
            configMan.Initialize();
            foreach (var assembly in assemblies.Concat(contentAssemblies))
            {
                configMan.LoadCVarsFromAssembly(assembly);
            }

            IoCManager.Resolve<IReflectionManager>().LoadAssemblies(assemblies);

            var resMan = IoCManager.Resolve<IResourceManagerInternal>();
            Directory.CreateDirectory("Resources/");
            ProgramShared.DoMounts(resMan);

            var modLoader = IoCManager.Resolve<TestingModLoader>();
            modLoader.Assemblies = contentAssemblies.ToArray();
            modLoader.TryLoadModulesFrom(ResourcePath.Root, "");

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

            // Don't reparse prototypes from disk every run.
            if (_prototypeManagerCache.IsValueCreated)
            {
                protoMan.LoadFromCachedResults(_prototypeManagerCache.Value!);
            }
            else
            {
                lock (_prototypeManagerCache)
                {
                    protoMan.LoadDirectory(ResourcePath.Root / "Prototypes");

                    _prototypeManagerCache.Value = protoMan.Cache;
                }
            }

            _protoDelegate?.Invoke(protoMan);
            protoMan.Resync();

            entityMan.Startup();

            var mapManager = container.Resolve<IMapManagerInternal>();
            mapManager.CreateMap(1, 1, MapId.Global);

            var tileMan = container.Resolve<ITileDefinitionManagerInternal>();
            tileMan.RegisterAll();

            var locMan = container.Resolve<ILocalizationManager>();
            locMan.Initialize();

            var compLoc = container.Resolve<IComponentLocalizerInternal>();
            compLoc.Initialize();

            return this;
        }

        public static IFullSimulationFactory NewSimulation()
        {
            return new FullGameSimulation();
        }
    }
}
