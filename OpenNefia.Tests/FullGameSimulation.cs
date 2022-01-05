using OpenNefia.Core.Containers;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameController;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Reflection;
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
        ISimulation InitializeInstance();
    }

    public delegate void AssemblyLoadDelegate(IList<Assembly> assemblies);

    public class FullGameSimulation : ISimulation, IFullSimulationFactory
    {
        // Required by the engine.
        private const string EmptyTile = @"
- type: Tile
  id: Empty
  isSolid: false
  isOpaque: false
";

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

        public Entity SpawnEntity(PrototypeId<EntityPrototype>? protoId, MapCoordinates coordinates)
        {
            var entMan = Collection.Resolve<IEntityManager>();
            return entMan.SpawnEntity(protoId, coordinates);
        }

        public Entity SpawnEntity(PrototypeId<EntityPrototype>? protoId, EntityCoordinates coordinates)
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
            IoCSetup.Register(GameController.DisplayMode.Headless);

            IoCManager.Register<IModLoader, TestingModLoader>(overwrite: true);
            IoCManager.Register<IModLoaderInternal, TestingModLoader>(overwrite: true);
            IoCManager.Register<TestingModLoader, TestingModLoader>(overwrite: true);
            IoCManager.Register<ILocalizationManager, TestingLocalizationManager>(overwrite: true);

            _diFactory?.Invoke(container);
            container.BuildGraph();

            var assemblies = new List<Assembly>();

            assemblies.Add(typeof(OpenNefia.Core.Engine).Assembly);
            assemblies.Add(Assembly.GetExecutingAssembly());

            var contentAssemblies = new List<Assembly>();
            _assemblyLoadDelegate?.Invoke(contentAssemblies);

            IoCManager.Resolve<IReflectionManager>().LoadAssemblies(assemblies);

            var modLoader = IoCManager.Resolve<TestingModLoader>();
            modLoader.Assemblies = contentAssemblies.ToArray();
            modLoader.TryLoadModulesFrom(ResourcePath.Root, "");

            var logMan = container.Resolve<ILogManager>();
            logMan.RootSawmill.AddHandler(new TestLogHandler("SIM"));

            var compFactory = container.Resolve<IComponentFactory>();
            compFactory.DoDefaultRegistrations();

            _regDelegate?.Invoke(compFactory);

            compFactory.FinishRegistration();

            var entityMan = container.Resolve<IEntityManager>();
            entityMan.Initialize();

            var entitySystemMan = container.Resolve<IEntitySystemManager>();

            _systemDelegate?.Invoke(entitySystemMan);

            var mapManager = container.Resolve<IMapManager>();

            entityMan.Startup();

            container.Resolve<ISerializationManager>().Initialize();

            var protoMan = container.Resolve<IPrototypeManager>();
            protoMan.Initialize();
            protoMan.LoadString(EmptyTile);
            _protoDelegate?.Invoke(protoMan);
            protoMan.Resync();

            var tileMan = container.Resolve<ITileDefinitionManagerInternal>();
            tileMan.RegisterAll();

            return this;
        }

        public static IFullSimulationFactory NewSimulation()
        {
            return new FullGameSimulation();
        }
    }
}
