using OpenNefia.Content.TurnOrder;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Content.RandomGen;
using OpenNefia.Core.Game;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Maps;
using OpenNefia.Content.EntityGen;

namespace OpenNefia.LecchoTorte.InfiniteSpawner
{
    public sealed class InfiniteSpawnerSystem : EntitySystem
    {
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;

        public override void Initialize()
        {
            SubscribeBroadcast<MapEnterEvent>(HandleMapEntered);
            SubscribeComponent<InfiniteSpawnerComponent, NPCTurnStartedEvent>(HandleTurnStarted);
            SubscribeComponent<InfiniteSpawnerComponent, EntityRefreshSpeedEvent>(HandleRefreshSpeed);
            SubscribeComponent<InfiniteSpawnedComponent, EntityDeletedEvent>(HandleDeleted);
            SubscribeComponent<InfiniteSpawnedComponent, EntityKilledEvent>(HandleKilled);
        }

        private void HandleTurnStarted(EntityUid uid, InfiniteSpawnerComponent component, ref NPCTurnStartedEvent args)
        {
            SpawnIfMissing(component);
        }

        private void HandleMapEntered(MapEnterEvent args)
        {
            foreach (var spawner in _lookup.EntityQueryInMap<InfiniteSpawnerComponent>(args.Map))
                SpawnIfMissing(spawner);
        }

        private void SpawnIfMissing(InfiniteSpawnerComponent component)
        {
            if (!TryMap(component.Owner, out var map))
                return;

            var spawned = _lookup.EntityQueryInMap<InfiniteSpawnedComponent>(map)
                .Any(s => s.Spawner == component.Owner);

            if (!spawned)
                SpawnEntity(component.Owner, component);
        }

        private void SpawnEntity(EntityUid uid, InfiniteSpawnerComponent? component = null)
        {
            if (!Resolve(uid, ref component))
                return;

            var ent = _charaGen.GenerateChara(uid, component.EntityID, args: EntityGenArgSet.Make(new EntityGenCommonArgs() { LevelOverride = 100 }));
            if (IsAlive(ent))
                EnsureComp<InfiniteSpawnedComponent>(ent.Value).Spawner = uid;
        }

        private void HandleRefreshSpeed(EntityUid uid, InfiniteSpawnerComponent component, ref EntityRefreshSpeedEvent args)
        {
            if (!TryComp<TurnOrderComponent>(_gameSession.Player, out var playerTurnOrder))
                return;

            args.OutSpeed = playerTurnOrder.CurrentSpeed;
            args.OutSpeedModifier = playerTurnOrder.CurrentSpeedModifier;
        }

        private void HandleDeleted(EntityUid uid, InfiniteSpawnedComponent component, EntityDeletedEvent args)
        {
            if (IsAlive(component.Spawner))
                SpawnEntity(component.Spawner);
        }

        private void HandleKilled(EntityUid uid, InfiniteSpawnedComponent component, ref EntityKilledEvent args)
        {
            if (IsAlive(component.Spawner))
                SpawnEntity(component.Spawner);
        }
    }
}