using OpenNefia.Content.Charas;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Parties;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.World;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Maps
{
    public interface IMapCharaGenSystem : IEntitySystem
    {
        void SpawnMobs(IMap map, PrototypeId<EntityPrototype>? charaId = null, MapCharaGenComponent? mapCharaGen = null);
    }

    public sealed class MapCharaGenSystem : EntitySystem, IMapCharaGenSystem
    {
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly IPartySystem _parties = default!;

        public override void Initialize()
        {
            SubscribeComponent<MapCharaGenComponent, GetCharaFilterEvent>(SetDefaultFilter);
            SubscribeComponent<MapCharaGenComponent, MapOnTimePassedEvent>(RespawnMobs);
            SubscribeComponent<CharaComponent, EntityGeneratedEvent>(HandleEntityGenerated, priority: EventPriorities.VeryHigh);
            SubscribeComponent<CharaComponent, EntityDeletedEvent>(HandleEntityDeleted, priority: EventPriorities.VeryHigh);
        }

        private void HandleEntityGenerated(EntityUid uid, CharaComponent component, ref EntityGeneratedEvent args)
        {
            if (!TryMap(uid, out var map) || !TryComp<MapCharaGenComponent>(map.MapEntityUid, out var mapCharaGen))
                return;

            if (!HasComp<TurnOrderComponent>(uid) || _parties.IsInPlayerParty(uid))
                return;

            mapCharaGen.CurrentCharaCount = Math.Max(mapCharaGen.CurrentCharaCount + 1, 0);
        }

        private void HandleEntityDeleted(EntityUid uid, CharaComponent component, EntityDeletedEvent args)
        {
            if (!TryMap(uid, out var map) || !TryComp<MapCharaGenComponent>(map.MapEntityUid, out var mapCharaGen))
                return;

            if (!HasComp<TurnOrderComponent>(uid) || _parties.IsInPlayerParty(uid))
                return;

            mapCharaGen.CurrentCharaCount = Math.Max(mapCharaGen.CurrentCharaCount - 1, 0);
        }

        private void SetDefaultFilter(EntityUid uid, MapCharaGenComponent component, ref GetCharaFilterEvent args)
        {
            if (component.CharaFilterGen != null)
            {
                Logger.DebugS("map.charagen", $"Setting chara filter: {component.CharaFilterGen}");
                EntitySystem.InjectDependencies(component.CharaFilterGen);
                args.CharaFilter = component.CharaFilterGen.GenerateFilter(args.Map);
            }
        }

        private void RespawnMobs(EntityUid uid, MapCharaGenComponent component, ref MapOnTimePassedEvent args)
        {
            // >>>>>>>> shade2 / main.hsp:545        if gTurn¥20 = 0 : call chara_spawn ...
            if (args.MinutesPassed == 0)
                return;

            if (_world.State.PlayTurns % 20 == 0)
            {
                SpawnMobs(args.Map);
            }
            // <<<<<<<< shade2 / main.hsp:545      if gTurn¥20 = 0 : call chara_spawn ..
        }

        public void SpawnMobs(IMap map, PrototypeId<EntityPrototype>? charaId = null, MapCharaGenComponent? mapCharaGen = null)
        {
            var ev = new BeforeSpawnMobsEvent(map);
            if (Raise(map.MapEntityUid, ev))
                return;

            if (!Resolve(map.MapEntityUid, ref mapCharaGen))
                return;

            var maxCharas = mapCharaGen.MaxCharaCount;
            if (maxCharas <= 0)
                return;

            var currentCharas = mapCharaGen.CurrentCharaCount;

            void Spawn(IMap map, PrototypeId<EntityPrototype>? charaId)
            {
                var filter = _charaGen.GenerateCharaFilter(map);
                if (filter.Id == null)
                    filter.Id = charaId;
                _charaGen.GenerateChara(map, filter);
            }

            if (currentCharas < maxCharas / 4 && _rand.OneIn(2))
                Spawn(map, charaId);

            if (currentCharas < maxCharas / 2 && _rand.OneIn(4))
                Spawn(map, charaId);

            if (currentCharas < maxCharas && _rand.OneIn(8))
                Spawn(map, charaId);
        }
    }

    public sealed class BeforeSpawnMobsEvent : HandledEntityEventArgs
    {
        public IMap Map { get; }

        public BeforeSpawnMobsEvent(IMap map)
        {
            Map = map;
        }
    }
}
