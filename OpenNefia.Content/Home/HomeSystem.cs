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
using OpenNefia.Core.Utility;
using OpenNefia.Content.World;
using OpenNefia.Content.Loot;
using OpenNefia.Content.Pickable;

namespace OpenNefia.Content.Home
{
    public sealed record class HomeRankItem(ValueComponent Item, int Value);

    public interface IHomeSystem : IEntitySystem
    {
        IReadOnlyList<MapId> ActiveHomeIDs { get; }

        /// <summary>
        /// Sets the player's home and clears all other homes.
        /// </summary>
        /// <param name="map"></param>
        void SetHome(IMap map);

        /// <summary>
        /// Adds a map to the list of the player's homes.
        /// </summary>
        /// <param name="map"></param>
        void AddHome(IMap map);

        /// <summary>
        /// Removes a map from the list of the player's homes.
        /// </summary>
        /// <param name="map"></param>
        void RemoveHome(IMap map);

        int CalcItemValue(EntityUid entity, ValueComponent? valueComp = null);
        int CalcFurnitureValue(EntityUid entity, ValueComponent? valueComp = null);
        IEnumerable<HomeRankItem> CalcMostValuableItems(IMap map, int amount = 10);
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

        [RegisterSaveData("Elona.HomeSystem.ActiveHomeIDs")]
        private List<MapId> _activeHomeIDs { get; } = new List<MapId>();
        public IReadOnlyList<MapId> ActiveHomeIDs =>_activeHomeIDs;

        public override void Initialize()
        {
            Initialize_Areas();
            SubscribeComponent<AreaHomeComponent, AreaMapEnterEvent>(UpdateMaxItemLimit, priority: EventPriorities.High);
            SubscribeComponent<AreaHomeComponent, AreaMapEnterEvent>(WelcomeHome, priority: EventPriorities.Low);
            SubscribeEntity<MapOnTimePassedEvent>(UpdateHomeOnTimePassed);
            SubscribeEntity<MapEnterEvent>(UpdateHomeOnMapEntered);
            SubscribeEntity<AfterItemPickedUpEvent>(UpdateHomeOnItemPickedUp);
            SubscribeEntity<AfterItemDroppedEvent>(UpdateHomeOnItemDropped);
        }

        public void SetHome(IMap map)
        {
            _activeHomeIDs.Clear();
            _activeHomeIDs.Add(map.Id);
        }

        public void AddHome(IMap map)
        {
            if (!_activeHomeIDs.Contains(map.Id))
                _activeHomeIDs.Add(map.Id);
        }

        public void RemoveHome(IMap map)
        {
            _activeHomeIDs.Remove(map.Id);
        }

        private void UpdateHomeOnTimePassed(EntityUid mapUid, ref MapOnTimePassedEvent ev)
        {
            // >>>>>>>> elona122/shade2/main.hsp:600 	if gArea=areaHome		: gosub *house_update ...
            var map = GetMap(mapUid);
            if (_activeHomeIDs.Contains(map.Id))
                UpdateRank(map);
            // <<<<<<<< elona122/shade2/main.hsp:600 	if gArea=areaHome		: gosub *house_update ...
        }

        private void UpdateHomeOnMapEntered(EntityUid uid, MapEnterEvent args)
        {
            // >>>>>>>> elona122/shade2/map.hsp:2127 	if gArea=areaHome		: gosub *house_update ...
            if (_activeHomeIDs.Contains(args.Map.Id))
                UpdateRank(args.Map);
            // <<<<<<<< elona122/shade2/map.hsp:2127 	if gArea=areaHome		: gosub *house_update ...
        }

        private void UpdateHomeOnItemPickedUp(EntityUid uid, AfterItemPickedUpEvent ev)
        {
            // >>>>>>>> elona122/shade2/action.hsp:277 		if gArea=areaHome		:if mode=mode_main:gosub *hou ...
            var map = GetMap(ev.Picker);
            if (_activeHomeIDs.Contains(map.Id))
                UpdateRank(map);
            // <<<<<<<< elona122/shade2/action.hsp:277 		if gArea=areaHome		:if mode=mode_main:gosub *hou ...
        }

        private void UpdateHomeOnItemDropped(EntityUid uid, AfterItemDroppedEvent ev)
        {
            // >>>>>>>> elona122/shade2/action.hsp:304 	if gArea=areaHome		: if mode=mode_main:gosub *hou ...
            var map = GetMap(ev.Picker);
            if (_activeHomeIDs.Contains(map.Id))
                UpdateRank(map);
            // <<<<<<<< elona122/shade2/action.hsp:304 	if gArea=areaHome		: if mode=mode_main:gosub *hou ...
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

        private bool CanWelcome(EntityUid ent)
        {
            return EntityManager.IsAlive(ent)
                && HasComp<CharaComponent>(ent)
                && !HasComp<Roles.RoleSpecialComponent>(ent)
                && !HasComp<Roles.RoleAdventurerComponent>(ent)
                && !_parties.IsInPlayerParty(ent)
                && (HasComp<ServantComponent>(ent)
                    || _factions.GetRelationToPlayer(ent) == Relation.Neutral
                    || _stayers.IsStaying(ent, StayingTags.Ally));
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
                .Where(spatial => CanWelcome(spatial.Owner)))
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

        public IEnumerable<HomeRankItem> CalcMostValuableItems(IMap map, int amount = 10)
        {
            return _lookup.EntityQueryInMap<ValueComponent>(map)
                .Select(value => new HomeRankItem(value, CalcItemValue(value.Owner, value)))
                .OrderByDescending(tuple => tuple.Value)
                .Take(amount);
        }

        public int CalcTotalHomeValue(IMap map)
        {
            return CalcMostValuableItems(map).Select(tuple => tuple.Value).Sum();
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
            if (!TryArea(map, out var area))
                return new HomeRank(10000, 0, 0, 0, 0);

            var baseValue = 0;
            if (Resolve(area.AreaEntityUid, ref areaHome))
                baseValue = areaHome.HomeRankPoints;

            var homeValue = Math.Clamp(CalcTotalHomeValue(map), 0, 10000);
            var furnitureValue = Math.Clamp(CalcTotalFurnitureValue(map), 0, 10000);
            var totalValue = SumTotalValue(baseValue, homeValue, furnitureValue);
            var homeRank = Math.Max(10000 - totalValue, 100);

            return new HomeRank(homeRank, baseValue, homeValue, furnitureValue, totalValue);
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

    public sealed record HomeRank(int RankExperience, int BaseValue, int HomeValue, int FurnitureValue, int TotalValue)
    {
        public int RankPlace => RankExperience / Rank.ExpPerRankPlace;
    }
}