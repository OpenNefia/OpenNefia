using OpenNefia.Content.Charas;
using OpenNefia.Content.DeferredEvents;
using OpenNefia.Content.EmotionIcon;
using OpenNefia.Content.Factions;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.GameObjects.Components;
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
using OpenNefia.Core.SaveGames;
using OpenNefia.Content.Parties;

namespace OpenNefia.Content.Home
{
    public interface IHomeSystem : IEntitySystem
    {
        MapId ActiveHomeID { get; set; }

        int CalcItemValue(EntityUid entity, ValueComponent? valueComp = null);
        int CalcFurnitureValue(EntityUid entity, ValueComponent? valueComp = null);
        IEnumerable<(ValueComponent item, int value)> CalcMostValuableItems(IMap map, int amount = 10);
        int CalcTotalHomeValue(IMap map);
        int CalcTotalFurnitureValue(IMap map);
        int SumTotalValue(int baseValue, int furnitureValue, int homeValue);
        HomeRank CalcRank(IMap map, AreaHomeComponent? mapHome = null);
        HomeRank UpdateRank(IMap map);
    }

    public sealed partial class HomeSystem : EntitySystem, IHomeSystem
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
        [Dependency] private readonly IPartySystem _parties = default!;

        public static readonly AreaFloorId AreaFloorHome = new("Elona.Home", 0);

        // TODO save data requires non-nullable references...
        [RegisterSaveData("Elona.HomeSystem.ActiveHomeID")]
        public MapId ActiveHomeID { get; set; } = MapId.Nullspace;

        public override void Initialize()
        {
            Initialize_Areas();
            SubscribeComponent<AreaHomeComponent, AreaMapEnterEvent>(UpdateMaxItemLimit, priority: EventPriorities.High);
            SubscribeComponent<AreaHomeComponent, AreaMapEnterEvent>(WelcomeHome, priority: EventPriorities.Low);
        }

        /// <summary>
        /// Avoids the problem in vanilla Elona where if you build a second floor in your home and
        /// then upgrade it, the max item count of the new home type is not applied to the existing
        /// floor.
        /// </summary>
        private void UpdateMaxItemLimit(EntityUid uid, AreaHomeComponent areaHome, AreaMapEnterEvent args)
        {
            if (areaHome.MaxItemsOnGround != null
                && TryComp<MapCommonComponent>(args.Map.MapEntityUid, out var mapCommon))
                mapCommon.MaxItemsOnGround = areaHome.MaxItemsOnGround.Value;
        }

        private void WelcomeHome(EntityUid uid, AreaHomeComponent component, AreaMapEnterEvent args)
        {
            _deferredEvents.Enqueue(() => Event_WelcomeHome(args.Map));
        }

        private bool CanWelcome(EntityUid ent, IMap map)
        {
            return EntityManager.IsAlive(ent)
                && HasComp<CharaComponent>(ent)
                && !HasComp<Roles.RoleSpecialComponent>(ent)
                && !HasComp<Roles.RoleAdventurerComponent>(ent)
                && !_parties.IsInPlayerParty(ent)
                && (HasComp<ServantComponent>(ent)
                    || _factions.GetRelationToPlayer(ent) == Relation.Neutral
                    || _stayers.IsStayingInMapGlobal(ent, map));
        }

        private TurnResult Event_WelcomeHome(IMap map)
        {
            var extraTalks = 0;

            void Welcome(EntityUid ent)
            {
                _emoicons.SetEmotionIcon(ent, EmotionIcons.Happy, 20);
                if (!_talk.Say(ent, "Elona.Welcome"))
                    extraTalks++;
            }

            foreach (var spatial in _lookup.GetEntitiesDirectlyIn(map.Id)
                .Where(spatial => CanWelcome(spatial.Owner, map)))
            {
                Welcome(spatial.Owner);
            }

            for (var i = 0; i < extraTalks; i++)
                _mes.Display(Loc.GetString("Elona.Home.WelcomeHome"), UiColors.MesSkyBlue);

            // TODO maid

            return TurnResult.NoResult;
        }

        public int CalcItemValue(EntityUid entity, ValueComponent? valueComp = null)
        {
            if (!Resolve(entity, ref valueComp))
                return 0;

            var value = valueComp.Value.Buffed * _stacks.GetCount(entity);

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

        public int CalcFurnitureValue(EntityUid entity, ValueComponent? valueComp = null)
        {
            if (!Resolve(entity, ref valueComp) || !_tags.HasTag(entity, Protos.Tag.ItemCatFurniture))
                return 0;

            return Math.Clamp(valueComp.Value.Buffed / 50, 50, 500);
        }

        public IEnumerable<(ValueComponent item, int value)> CalcMostValuableItems(IMap map, int amount = 10)
        {
            return _lookup.EntityQueryInMap<ValueComponent>(map)
                .Select(value => (value, CalcItemValue(value.Owner, value)))
                .OrderByDescending(tuple => tuple.Item2)
                .Take(amount);
        }

        public int CalcTotalHomeValue(IMap map)
        {
            return CalcMostValuableItems(map).Select(tuple => tuple.value).Sum();
        }

        public int CalcTotalFurnitureValue(IMap map)
        {
            return _lookup.EntityQueryInMap<ValueComponent>(map)
                .Select(value => CalcFurnitureValue(value.Owner, value))
                .Sum();
        }

        public int SumTotalValue(int baseValue, int furnitureValue, int homeValue)
        {
            return baseValue + furnitureValue + homeValue / 3;
        }

        public HomeRank CalcRank(IMap map, AreaHomeComponent? areaHome = null)
        {
            if (!TryArea(map, out var area) || !Resolve(area.AreaEntityUid, ref areaHome))
                return new HomeRank(0, 0, 0, 0);

            var baseValue = areaHome.HomeRankPoints;
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