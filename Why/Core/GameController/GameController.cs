using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Why.Core.Asynchronous;
using Why.Core.ContentPack;
using Why.Core.GameObjects;
using Why.Core.Graphics;
using Why.Core.IoC;
using Why.Core.Log;
using Why.Core.Maps;
using Why.Core.Prototypes;
using Why.Core.ResourceManagement;
using Why.Core.Serialization.Manager;
using Why.Core.Utility;

namespace Why.Core.GameController
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

        public bool Startup()
        {
            SetupLogging(_logManager, () => new ConsoleLogHandler());

            _graphics.Initialize();

            _taskManager.Initialize();

            _modLoader.SetUseLoadContext(true);

            if (!_modLoader.TryLoadModulesFrom(new ResourcePath("/Assets/Assemblies"), string.Empty))
            {
                Logger.Fatal("Errors while loading content assemblies.");
                return false;
            }

            _serialization.Initialize();

            _resourceCache.Initialize(null);
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

            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();

            return true;
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
            var map = new Map(40, 10);

            var uid = _mapManager.RegisterMap(map);

            static void PrintMap(IMap map)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    for (int x = 0; x < map.Width; x++)
                    {
                        char c = (char)(map.Tiles[x, y].Type + '.');
                        if (map.AtPos(x, y).GetEntities().Any())
                            c = '$';

                        Console.Write($"{c}");
                    }
                    Console.Write("\n");
                }
            }

            for (int i = 0; i < 10; i++)
                _entityManager.SpawnEntity("Dagger", new MapCoordinates(map.Id, 1 + i, 1));

            for (int i = 0; i < 20; i++)
                _entityManager.SpawnEntity("Putit", new MapCoordinates(map.Id, 1 + i, 2));

            PrintMap(map);

            foreach (var entity in map.Entities.ToList())
            {
                _entityManager.EventBus.RaiseLocalEvent(entity.Uid, new TestEntityEvent(1));
                _entityManager.EventBus.RaiseLocalEvent(entity.Uid, new ImpregnateEvent());
            }

            for (int i = 0; i < 10; i++)
            {
                foreach (var entity in map.Entities.ToList())
                {
                    _entityManager.EventBus.RaiseLocalEvent(entity.Uid, new TurnStartEvent());
                }
            }

            foreach (var entity in map.Entities)
            {
                _entityManager.DeleteEntity(entity);
            }

            PrintMap(map);
        }

        private void Cleanup()
        {
            _entityManager.Shutdown();
        }
    }
}
