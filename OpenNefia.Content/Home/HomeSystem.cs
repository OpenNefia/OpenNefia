using OpenNefia.Content.Charas;
using OpenNefia.Content.DeferredEvents;
using OpenNefia.Content.EmotionIcon;
using OpenNefia.Content.Factions;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Ranks;
using OpenNefia.Content.Stayers;
using OpenNefia.Content.Talk;
using OpenNefia.Content.UI;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;

namespace OpenNefia.Content.Home
{
    public interface IHomeSystem : IEntitySystem
    {
        int CalcItemValue(EntityUid entity, ItemComponent? itemComp = null);
        int CalcFurnitureValue(EntityUid entity, ItemComponent? itemComp = null);
        IEnumerable<(ItemComponent item, int value)> CalcMostValuableItems(IMap map, int amount = 10);
        int CalcTotalHomeValue(IMap map);
        int CalcTotalFurnitureValue(IMap map);
        int SumTotalValue(int baseValue, int furnitureValue, int homeValue);
        HomeRank CalcRank(IMap map, MapHomeComponent? mapHome = null);
        HomeRank UpdateRank(IMap map);
    }

    public sealed class HomeSystem : EntitySystem, IHomeSystem
    {
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly ITagSystem _tags = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IDeferredEventsSystem _deferredEvents = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly IStayersSystem _stayers = default!;
        [Dependency] private readonly IEmotionIconSystem _emoicons = default!;
        [Dependency] private readonly ITalkSystem _talk = default!;
        [Dependency] private readonly IRankSystem _ranks = default!;

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

        public IEnumerable<(ItemComponent item, int value)> CalcMostValuableItems(IMap map, int amount = 10)
        {
            return _lookup.EntityQueryInMap<ItemComponent>(map)
                .Select(item => (item, CalcItemValue(item.Owner, item)))
                .OrderByDescending(tuple => tuple.Item2)
                .Take(amount);
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

        public HomeRank UpdateRank(IMap map)
        {
            var newRank = CalcRank(map);
            var oldRank = _ranks.GetRank(Protos.Rank.Home);

            if (newRank.RankExperience != oldRank.Experience)
            {
                Color color;
                // 1st rank is better than 10th rank.

                if (newRank.RankExperience < oldRank.Experience)
                    color = UiColors.MesGreen;
                else
                    color = UiColors.MesPurple;

                _mes.Display(Loc.GetString("Elona.Home.Rank.Change",
                    ("furnitureRank", newRank.FurnitureValue / 100),
                    ("homeRank", newRank.HomeValue / 100),
                    ("oldRank", oldRank.Place),
                    ("newRank", newRank.RankPlace),
                    ("title", _ranks.GetRankTitle(Protos.Rank.Home, newRank.RankPlace))), color);
            }

            _ranks.SetRank(Protos.Rank.Home, newRank.RankExperience);
            return newRank;
        }
    }

    public sealed record HomeRank(int RankExperience, int BaseValue, int HomeValue, int FurnitureValue)
    {
        public int RankPlace => RankExperience / Rank.ExpPerRankPlace;
    }
}