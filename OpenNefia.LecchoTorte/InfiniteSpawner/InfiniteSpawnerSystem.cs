using OpenNefia.Content.TurnOrder;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Content.RandomGen;
using OpenNefia.Core.Game;

namespace OpenNefia.LecchoTorte.InfiniteSpawner
{
    public sealed class InfiniteSpawnerSystem : EntitySystem
    {
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;

        public override void Initialize()
        {
            SubscribeComponent<InfiniteSpawnerComponent, NPCTurnStartedEvent>(HandleTurnStarted);
            SubscribeComponent<InfiniteSpawnerComponent, EntityRefreshSpeedEvent>(HandleRefreshSpeed);
        }

        private void HandleTurnStarted(EntityUid uid, InfiniteSpawnerComponent component, ref NPCTurnStartedEvent args)
        {
            if (!TryMap(uid, out var map))
                return;

            var spawned = _lookup.EntityQueryInMap<InfiniteSpawnedComponent>(map)
                .Any(s => s.Spawner == uid);

            if (!spawned)
                SpawnEntity(uid, component);
        }

        private void SpawnEntity(EntityUid uid, InfiniteSpawnerComponent component)
        {
            var ent = _charaGen.GenerateChara(uid, component.EntityID);
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
    }
}