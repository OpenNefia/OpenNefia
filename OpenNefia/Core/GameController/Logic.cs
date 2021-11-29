using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;

namespace OpenNefia.Core.GameController
{
    internal class Logic
    {
        internal static void Go()
        {
            var mapManager = IoCManager.Resolve<IMapManager>();
            var entityManager = IoCManager.Resolve<IEntityManager>();

            var map = new Map(40, 10);

            var uid = mapManager.RegisterMap(map);

            static void PrintMap(IMap map)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    for (int x = 0; x < map.Width; x++)
                    {
                        char c = (char)(map.Tiles[x, y].Type + '.');
                        if (map.AtPos(new Vector2i(x, y)).GetEntities().Any())
                            c = '$';

                        Console.Write($"{c}");
                    }
                    Console.Write("\n");
                }
            }

            for (int i = 0; i < 10; i++)
                entityManager.SpawnEntity("Dagger", new MapCoordinates(map.Id, 1 + i, 1));

            for (int i = 0; i < 20; i++)
                entityManager.SpawnEntity("Putit", new MapCoordinates(map.Id, 1 + i, 2));

            PrintMap(map);

            foreach (var chara in entityManager.EntityQuery<CharaComponent>())
            {
                entityManager.EventBus.RaiseLocalEvent(chara.OwnerUid, new CharaInitEvent());
            }

            foreach (var entity in map.Entities.ToList())
            {
                entityManager.EventBus.RaiseLocalEvent(entity.Uid, new TestEntityEvent(1));
                entityManager.EventBus.RaiseLocalEvent(entity.Uid, new ImpregnateEvent());
            }

            for (int i = 0; i < 10; i++)
            {
                foreach (var entity in map.Entities.ToList())
                {
                    entityManager.EventBus.RaiseLocalEvent(entity.Uid, new TurnStartEvent());
                }
            }

            foreach (var entity in map.Entities)
            {
                entityManager.DeleteEntity(entity);
            }

            PrintMap(map);
        }
    }
}