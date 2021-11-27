using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Asynchronous;
using OpenNefia.Core.ContentPack;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.GameController
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

            foreach (var chara in _entityManager.EntityQuery<CharaComponent>())
            {
                _entityManager.EventBus.RaiseLocalEvent(chara.OwnerUid, new CharaInitEvent());
            }

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
