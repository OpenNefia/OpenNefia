using OpenNefia.Content.Charas;
using OpenNefia.Content.DeferredEvents;
using OpenNefia.Content.EmotionIcon;
using OpenNefia.Content.Factions;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Stayers;
using OpenNefia.Content.Talk;
using OpenNefia.Content.UI;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Home
{
    public interface IHomeSystem : IEntitySystem
    {
        int CalcItemValue(EntityUid entity, ItemComponent? itemComp = null);
        int CalcFurnitureValue(EntityUid entity, ItemComponent? itemComp = null);
        IEnumerable<(ItemComponent item, int value)> CalcMostValuableItems(IMap map);
        int CalcTotalHomeValue(IMap map);
        int CalcTotalFurnitureValue(IMap map);
        int SumTotalValue(int baseValue, int furnitureValue, int homeValue);
        HomeRank CalcRank(IMap map, MapHomeComponent? mapHome = null);
        void UpdateRank(IMap map);
    }

    public sealed class HomeSystem : EntitySystem, IHomeSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly ITagSystem _tags = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IDeferredEventsSystem _deferredEvents = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly IStayersSystem _stayers = default!;
        [Dependency] private readonly IEmotionIconSystem _emoicons = default!;
        [Dependency] private readonly ITalkSystem _talk = default!;

        public override void Initialize()
        {
            SubscribeComponent<MapHomeComponent, MapEnterEvent>(WelcomeHome, priority: EventPriorities.Low);
        }

        private void WelcomeHome(EntityUid uid, MapHomeComponent component, MapEnterEvent args)
        {
            _deferredEvents.Add(() => DefEvWelcomeHome(args.Map));
        }

        private bool CanWelcome(EntityUid ent, IMap map)
        {
            return EntityManager.IsAlive(ent)
                && !HasComp<Roles.RoleSpecialComponent>(ent)
                && !HasComp<Roles.RoleAdventurerComponent>(ent)
                && (HasComp<ServantComponent>(ent)
                    || _factions.GetRelationToPlayer(ent) == Relation.Neutral
                    || _stayers.IsStayingInMapGlobal(ent, map));
        }

        private void DefEvWelcomeHome(IMap map)
        {
            var extraTalks = 0;

            void Welcome(EntityUid ent)
            {
                _emoicons.SetEmotionIcon(ent, "Elona.Happy", 20);
                if (!_talk.Say(ent, "Elona.Welcome"))
                    extraTalks++;
            }

            foreach (var spatial in _lookup.GetEntitiesDirectlyIn(map.Id)
                .Where(spatial => CanWelcome(spatial.Owner, map)))
            {
                Welcome(spatial.Owner);
            }

            for (var i = 0; i < extraTalks; i++)
                _mes.Display(Loc.GetString("Elona.Home.Okaeri"), UiColors.MesSkyBlue);

            // TODO maid
        }

        public int CalcItemValue(EntityUid entity, ItemComponent? itemComp = null)
        {
            if (!Resolve(entity, ref itemComp))
                return 0;

            var value = itemComp.Value * _stacks.GetCount(entity);

            if (_tags.HasTag(entity, Protos.Tag.ItemCatFurniture))
                value /= 20;
            else if (_tags.HasTag(entity, Protos.Tag.ItemCatTree))
                value /= 10;
            else if (_tags.HasTag(entity, Protos.Tag.ItemCatOre))
                value /= 10;
            else
                value /= 1000;

            return value;
        }

        public int CalcFurnitureValue(EntityUid entity, ItemComponent? itemComp = null)
        {
            if (!Resolve(entity, ref itemComp) || !_tags.HasTag(entity, Protos.Tag.ItemCatFurniture))
                return 0;

            return Math.Clamp(itemComp.Value / 50, 50, 500);
        }

        public IEnumerable<(ItemComponent item, int value)> CalcMostValuableItems(IMap map)
        {
            return _lookup.EntityQueryInMap<ItemComponent>(map)
                .Select(item => (item, CalcItemValue(item.Owner, item)))
                .OrderByDescending(tuple => tuple.Item2)
                .Take(10);
        }

        public int CalcTotalHomeValue(IMap map)
        {
            return CalcMostValuableItems(map).Select(tuple => tuple.value).Sum();
        }

        public int CalcTotalFurnitureValue(IMap map)
        {
            return _lookup.EntityQueryInMap<ItemComponent>(map)
                .Select(item => CalcFurnitureValue(item.Owner, item))
                .Sum();
        }

        public int SumTotalValue(int baseValue, int furnitureValue, int homeValue)
        {
            return baseValue + furnitureValue + homeValue / 3;
        }

        public HomeRank CalcRank(IMap map, MapHomeComponent? mapHome = null)
        {
            if (!Resolve(map.MapEntityUid, ref mapHome))
                return new HomeRank(0, 0, 0, 0);

            var baseValue = mapHome.HomeRankValue;
            var homeValue = Math.Clamp(CalcTotalHomeValue(map), 0, 10000);
            var furnitureValue = Math.Clamp(CalcTotalFurnitureValue(map), 0, 10000);
            var homeRank = Math.Max(10000 - SumTotalValue(baseValue, homeValue, furnitureValue), 100);

            return new HomeRank(homeRank, baseValue, homeValue, furnitureValue);
        }

        public void UpdateRank(IMap map)
        {
            var newRank = CalcRank(map);
            // TODO rank
        }
    }

    public sealed record HomeRank(int Rank, int BaseValue, int HomeValue, int FurnitureValue);
}