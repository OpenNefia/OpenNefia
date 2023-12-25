using OpenNefia.Content.Activity;
using OpenNefia.Content.Buffs;
using OpenNefia.Content.Cargo;
using OpenNefia.Content.CurseStates;
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Feats;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Hunger;
using OpenNefia.Content.Identify;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Items;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Materials;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Pickable;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Quests;
using OpenNefia.Content.Sanity;
using OpenNefia.Content.Skills;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Content.UI;
using OpenNefia.Content.VanillaAI;
using OpenNefia.Content.Weight;
using OpenNefia.Content.World;
using OpenNefia.Core;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;

using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Fame;
using OpenNefia.Content.Dialog;

namespace OpenNefia.Content.Food
{
    public interface IFoodSystem : IEntitySystem
    {
        bool IsAboutToRot(EntityUid ent, FoodComponent? food = null);
        bool IsCooked(EntityUid ent, FoodComponent? food = null);
        void SetRottedState(EntityUid ent, bool setImage = true, FoodComponent? food = null);
        void SpoilFoodInMap(IMap map);
        void SpoilFood(EntityUid ent, FoodComponent? food = null);

        string GetFoodName(EntityUid ent, FoodComponent? food = null);
        PrototypeId<ChipPrototype> GetFoodChip(PrototypeId<FoodTypePrototype> foodType, int foodQuality);
        void MakeDish(EntityUid item, int foodQuality, FoodComponent? food = null);
        bool IsHumanFlesh(EntityUid entity, FoodComponent? food = null);

        void EatFood(EntityUid eater, EntityUid food, HungerComponent? hungerComp = null, FoodComponent? foodComp = null);
        void ApplyFoodEffects(EntityUid eater, EntityUid food, HungerComponent? hungerComp = null, FoodComponent? foodComp = null);
    }

    public sealed class FoodSystem : EntitySystem, IFoodSystem
    {
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IDisplayNameSystem _displayNames = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IBuffsSystem _buffs = default!;
        [Dependency] private readonly ISanitySystem _sanity = default!;
        [Dependency] private readonly IHungerSystem _hunger = default!;
        [Dependency] private readonly IStatusEffectSystem _effects = default!;
        [Dependency] private readonly IFeatsSystem _feats = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IWeightSystem _weight = default!;
        [Dependency] private readonly IIdentifySystem _identify = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;
        [Dependency] private readonly IActivitySystem _activities = default!;
        [Dependency] private readonly IDamageSystem _damages = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly IKarmaSystem _karmas = default!;
        [Dependency] private readonly IDialogSystem _dialogs = default!;

        public override void Initialize()
        {
            SubscribeComponent<FoodComponent, GetVerbsEventArgs>(HandleGetVerbs);
            SubscribeComponent<FoodComponent, EntityBeingGeneratedEvent>(HandleGenerated, priority: EventPriorities.High);
            SubscribeComponent<FoodComponent, GetVerbsEventArgs>(HandleGetVerbs);
            SubscribeComponent<FoodComponent, SpoilFoodEvent>(HandleSpoilFood, priority: EventPriorities.VeryLow);
            SubscribeComponent<FoodComponent, AfterItemEatenEvent>(HandleEatFood, priority: EventPriorities.VeryLow);
            SubscribeComponent<FoodComponent, AfterItemPurchasedEvent>(HandlePurchaseItem);
            SubscribeEntity<MapOnTimePassedEvent>(ProcSpoilFoodInMap, priority: EventPriorities.High);
        }

        public const string VerbTypeEat = "Elona.Eat";

        private void HandleGetVerbs(EntityUid uid, FoodComponent food, GetVerbsEventArgs args)
        {
            args.OutVerbs.Add(new Verb(VerbTypeEat, "Eat Food", () => DoEat(args.Source, args.Target)));
        }

        private void HandleGenerated(EntityUid uid, FoodComponent foodComp, ref EntityBeingGeneratedEvent args)
        {
            if (args.GenArgs.TryGet<ItemGenArgs>(out var itemGenArgs))
            {
                if (itemGenArgs.IsShop)
                {
                    if (_rand.OneIn(2))
                    {
                        foodComp.FoodQuality = 0;
                    }
                    else
                    {
                        foodComp.FoodQuality = _rand.Next(3) + 3;
                    }
                }
            }

            if (foodComp.SpoilageInterval != null)
                foodComp.SpoilageDate = _world.State.GameDate + foodComp.SpoilageInterval;

            if (IsCooked(uid, foodComp) && TryComp<ChipComponent>(uid, out var chip))
                chip.ChipID = GetFoodChip(foodComp.FoodType!.Value, foodComp.FoodQuality);
        }

        private TurnResult DoEat(EntityUid eater, EntityUid target)
        {
            if (!_stacks.TrySplit(target, 1, out var split))
                return TurnResult.Failed;

            var activity = EntityManager.SpawnEntity(Protos.Activity.Eating, MapCoordinates.Global);
            Comp<ActivityEatingComponent>(activity).Food = split;
            _activities.StartActivity(eater, activity);

            return TurnResult.Succeeded;
        }

        private void HandleSpoilFood(EntityUid uid, FoodComponent food, SpoilFoodEvent args)
        {
            if (args.Handled)
                return;

            if (TryComp<SpatialComponent>(uid, out var spatial) && spatial.Parent != null)
            {
                if (_parties.IsInPlayerParty(spatial.Parent.Owner))
                {
                    _mes.Display(Loc.GetString("Elona.Food.GetsRotten", ("food", uid)));
                }
            }

            SetRottedState(uid, setImage: true, food);
            args.Handled = true;
        }

        private void HandleEatFood(EntityUid food, FoodComponent foodComp, AfterItemEatenEvent args)
        {
            ApplyFoodEffects(args.Eater, food, null, foodComp);
        }

        public void EatFood(EntityUid eater, EntityUid food, HungerComponent? hungerComp = null, FoodComponent? foodComp = null)
        {
            // TODO unequip first

            var ev = new AfterItemEatenEvent(eater);
            RaiseEvent(food, ev);

            _stacks.Use(food, 1);
        }

        public void ApplyFoodEffects(EntityUid eater, EntityUid food, HungerComponent? hungerComp = null, FoodComponent? foodComp = null)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:1136 	if cc=pc{ ...
            if (!Resolve(eater, ref hungerComp) || !Resolve(food, ref foodComp))
                return;

            ApplyGeneralEatingEffect(eater, foodComp);

            if (_gameSession.IsPlayer(eater))
            {
                _identify.IdentifyItem(food, IdentifyState.Name);
                _mes.Display(_hunger.GetNutritionMessage(hungerComp.Nutrition), UiColors.MesGreen);
            }
            else
            {
                ProcNPCRottenFood(eater, food, hungerComp, foodComp);
            }

            _hunger.VomitIfAnorexic(eater);

            var ev = new AfterFinishedEatingEvent(eater);
            RaiseEvent(food, ev);
            // <<<<<<<< elona122/shade2/proc.hsp:1154 	} ...
        }

        private void ProcNPCRottenFood(EntityUid eater, EntityUid food, HungerComponent hungerComp, FoodComponent foodComp)
        {
            if (!TryComp<QuestClientComponent>(eater, out var questClient) || !TryComp<VanillaAIComponent>(eater, out var ai))
                return;

            var isEatingTradedItem = ai.ItemAboutToUse == food && questClient.WasPassedQuestItem;
            if (!isEatingTradedItem)
                return;

            questClient.WasPassedQuestItem = false;

            if (foodComp.IsRotten)
            {
                _mes.Display(Loc.GetString("Elona.Food.PassedRotten", ("speaker", eater), ("food", food)), entity: eater);
                _damages.DamageHP(eater, 999, damageType: new GenericDamageType("Elona.DamageType.FoodPoisoning"));
                if (!IsAlive(eater) && _factions.GetRelationToPlayer(eater) > Relation.Neutral)
                    _karmas.ModifyKarma(_gameSession.Player, -5);
                else
                    _karmas.ModifyKarma(_gameSession.Player, -1);
                _dialogs.ModifyImpression(eater, -25);
                return;
            }
        }

        private void HandlePurchaseItem(EntityUid uid, FoodComponent component, AfterItemPurchasedEvent args)
        {
            if (component.SpoilageInterval != null)
            {
                component.SpoilageDate = _world.State.GameDate + component.SpoilageInterval.Value;
                if (IsCooked(uid, component))
                {
                    component.SpoilageDate = component.SpoilageDate.Value + GameTimeSpan.FromDays(3);
                }
            }
        }

        private void ProcSpoilFoodInMap(EntityUid uid, ref MapOnTimePassedEvent args)
        {
            if (args.HoursPassed <= 0)
                return;

            SpoilFoodInMap(args.Map);
        }

        public bool IsAboutToRot(EntityUid ent, FoodComponent? food = null)
        {
            if (!EntityManager.IsAlive(ent))
                return false;

            if (!Resolve(ent, ref food))
                return false;

            // TODO make compatible with components instead of ID checks
            if (TryComp<MaterialComponent>(ent, out var material) && material.MaterialID != Protos.Material.Fresh)
                return false;

            if (TryComp<PickableComponent>(ent, out var pickable) && pickable.OwnState > OwnState.NPC)
                return false;

            return !food.IsRotten && food.SpoilageDate != null && food.SpoilageDate < _world.State.GameDate;
        }

        public bool IsCooked(EntityUid ent, FoodComponent? food = null)
        {
            if (!Resolve(ent, ref food, logMissing: false))
                return false;

            return food.FoodType != null && food.FoodQuality > 0;
        }

        public void SetRottedState(EntityUid ent, bool setImage = true, FoodComponent? food = null)
        {
            if (!Resolve(ent, ref food))
                return;

            food.IsRotten = true;
            food.SpoilageDate = null;

            if (setImage && TryComp<ChipComponent>(ent, out var chip))
                chip.ChipID = Protos.Chip.ItemRottenFood;
        }

        public void SpoilFoodInMap(IMap map)
        {
            foreach (var food in _lookup.EntityQueryInMap<FoodComponent>(map))
            {
                if (IsAboutToRot(food.Owner, food))
                    SpoilFood(food.Owner, food);
            }
        }

        public void SpoilFood(EntityUid ent, FoodComponent? food = null)
        {
            if (!Resolve(ent, ref food))
                return;

            var ev = new SpoilFoodEvent();
            Raise(ent, ev);

            if (TryMap(ent, out var map))
            {
                var spatial = Comp<SpatialComponent>(ent);
                map.RefreshTile(spatial.WorldPosition);
            }
        }

        public string GetFoodName(EntityUid ent, FoodComponent? food = null)
        {
            var origin = _displayNames.GetBaseName(ent);
            if (!Resolve(ent, ref food) || food.FoodType == null || !_protos.TryIndex(food.FoodType.Value, out var foodType))
                return origin;

            if (foodType.UsesCharaName)
            {
                if (TryComp<EntityProtoSourceComponent>(ent, out var fromChara))
                {
                    origin = Loc.GetPrototypeString(fromChara.EntityID, "MetaData.Name");
                }
                else
                {
                    origin = Loc.GetPrototypeString(food.FoodType.Value, "DefaultOrigin");
                }
            }

            return Loc.GetPrototypeString(foodType, $"Names.{food.FoodQuality}", ("origin", origin));
        }

        public PrototypeId<ChipPrototype> GetFoodChip(PrototypeId<FoodTypePrototype> foodType, int foodQuality)
        {
            if (!_protos.TryIndex(foodType, out var proto)
                || !proto.ItemChips.TryGetValue(foodQuality, out var chipId))
            {
                Logger.WarningS("food", $"Missing food image for food type {foodType}, quality {foodQuality}");
                chipId = Protos.Chip.ItemDishCharred;
            }
            return chipId;
        }

        public void MakeDish(EntityUid item, int foodQuality, FoodComponent? food = null)
        {
            if (!Resolve(item, ref food) || food.FoodType == null)
                return;

            if (TryComp<ChipComponent>(item, out var chip))
                chip.ChipID = GetFoodChip(food.FoodType.Value, foodQuality);
            if (TryComp<WeightComponent>(item, out var weight))
                weight.Weight.Base = 500;

            if (!food.IsRotten && food.SpoilageInterval != null)
                food.SpoilageDate = _world.State.GameDate + GameTimeSpan.FromDays(3);

            food.FoodQuality = int.Clamp(foodQuality, 1, 9);
        }

        private void ApplyFoodCurseState(EntityUid eater, CurseState curseState, HungerComponent? hunger = null)
        {
            if (!IsAlive(eater) || !Resolve(eater, ref hunger))
                return;

            if (curseState <= CurseState.Cursed)
            {
                hunger.Nutrition -= 1500;

                _mes.Display(Loc.GetString("Elona.Food.EatStatus.Bad", ("eater", eater)), entity: eater);

                _hunger.Vomit(eater);
            }
            else if (curseState == CurseState.Blessed)
            {
                _mes.Display(Loc.GetString("Elona.Food.EatStatus.Good", ("eater", eater)), entity: eater);

                if (_rand.OneIn(5))
                    _buffs.AddBuff(eater, "Elona.Lucky", 100, 500 + _rand.Next(500), eater);

                _sanity.HealInsanity(eater, 2);
            }
        }

        private void ApplyRottenFoodEffects(EntityUid entity, List<ExperienceGain> expGains, ref int nutrition)
        {
            if (CompOrNull<CommonProtectionsComponent>(entity)?.IsProtectedFromRottenFood.Buffed ?? false)
            {
                _mes.Display(Loc.GetString("Elona.Hunger.NotAffectedByRotten"));
                return;
            }

            expGains.Clear();

            void AddExpLoss(PrototypeId<SkillPrototype> id)
            {
                expGains.Add(new ExperienceGain() { SkillID = id, Experience = -100 });
            }

            AddExpLoss(Protos.Skill.AttrStrength);
            AddExpLoss(Protos.Skill.AttrConstitution);
            AddExpLoss(Protos.Skill.AttrCharisma);
            AddExpLoss(Protos.Skill.AttrMagic);
            AddExpLoss(Protos.Skill.AttrDexterity);
            AddExpLoss(Protos.Skill.AttrPerception);
            AddExpLoss(Protos.Skill.AttrLearning);
            AddExpLoss(Protos.Skill.AttrWill);

            nutrition = 1000;

            _effects.Apply(entity, Protos.StatusEffect.Paralysis, 100);
            _effects.Apply(entity, Protos.StatusEffect.Confusion, 200);
        }

        public bool IsHumanFlesh(EntityUid entity, FoodComponent? food = null)
        {
            if (!Resolve(entity, ref food)
                || !TryComp<EntityProtoSourceComponent>(entity, out var protoSource)
                || food.FoodType != Protos.FoodType.Meat)
                return false;

            var proto = _protos.Index(protoSource.EntityID);

            return proto.Components.TryGetComponent<TagComponent>(out var tags) && tags.Tags.Contains(Protos.Tag.CharaMan);
        }

        public void ShowPlayerEatingMessage(EntityUid player, FoodComponent food)
        {
            if (_feats.HasFeat(player, Protos.Feat.EatHuman) && IsHumanFlesh(food.Owner, food))
            {
                _mes.Display(Loc.GetString("Elona.Food.Message.Human.Delicious"));
            }

            if (food.IsRotten)
            {
                _mes.Display(Loc.GetString("Elona.Food.Message.Rotten"));
                return;
            }

            if (food.FoodQuality <= 0 && food.FoodType != null)
            {
                if (!Loc.TryGetPrototypeString(food.FoodType.Value, "UncookedMessage", out var text))
                    text = Loc.GetString("Elona.Food.Message.Uncooked");

                _mes.Display(text);
                return;
            }

            LocaleKey key;
            if (food.FoodQuality <= 3)
                key = "Elona.Food.Message.Quality.Bad";
            else if (food.FoodQuality <= 5)
                key = "Elona.Food.Message.Quality.SoSo";
            else if (food.FoodQuality <= 7)
                key = "Elona.Food.Message.Quality.Good";
            else if (food.FoodQuality <= 9)
                key = "Elona.Food.Message.Quality.Great";
            else
                key = "Elona.Food.Message.Quality.Delicious";

            _mes.Display(Loc.GetString(key));
        }

        private const int MaxFoodEatingEffects = 10;

        private void ApplyGeneralEatingEffect(EntityUid eater, FoodComponent food)
        {
            if (!TryComp<HungerComponent>(eater, out var hunger))
                return;

            var nutrition = food.BaseNutrition;

            // TODO configure this?
            if (HasComp<CargoComponent>(food.Owner))
                nutrition += 2500;

            var expGains = new List<ExperienceGain>();

            if (food.FoodType != null)
            {
                var proto = _protos.Index(food.FoodType.Value);

                expGains.AddRange(proto.ExpGains);

                if (proto.BaseNutrition != null)
                    nutrition = proto.BaseNutrition.Value;
            }

            nutrition = nutrition * (100 + food.FoodQuality * 15) / 100;

            foreach (var expGain in expGains)
            {
                if (expGain.Experience > 0)
                {
                    if (food.FoodQuality < 3)
                    {
                        expGain.Experience /= 2;
                    }
                    else
                    {
                        expGain.Experience = expGain.Experience * (50 * food.FoodQuality * 20) / 100;
                    }
                }
                else
                {
                    if (food.FoodQuality < 3)
                    {
                        expGain.Experience = expGain.Experience * ((3 - food.FoodQuality) * 100 + 100) / 100;
                    }
                    else
                    {
                        expGain.Experience = expGain.Experience * 100 / (food.FoodQuality * 50);
                    }
                }
            }

            if (_gameSession.IsPlayer(eater))
            {
                ShowPlayerEatingMessage(eater, food);
            }
            else
            {
                if (food.IsRotten)
                    _mes.Display(Loc.GetString("Elona.Food.Message.RawGlum", ("entity", eater)));
            }

            expGains.AddRange(food.ExperienceGains);

            if (food.Nutrition != null)
                nutrition = food.Nutrition.Value;

            if (_gameSession.IsPlayer(eater) && food.IsRotten)
                ApplyRottenFoodEffects(eater, expGains, ref nutrition);

            var ev = new BeforeApplyFoodEffectsEvent(eater, nutrition, expGains);
            RaiseEvent(food.Owner, ev);
            nutrition = ev.OutNutrition;
            expGains = ev.OutExpGains;

            foreach (var expGain in expGains.Take(MaxFoodEatingEffects))
            {
                var power = 100;

                if (hunger.Nutrition >= HungerLevels.Normal)
                {
                    var factor = (hunger.Nutrition - HungerLevels.Normal) / 25;
                    power = power * 100 / (100 + factor);
                }

                if (!_gameSession.IsPlayer(eater))
                {
                    power = food.IsRotten ? 500 : 1500;
                }

                if (power > 0)
                    _skills.GainSkillExp(eater, expGain.SkillID, expGain.Experience * power / 100);
            }

            var curseState = CompOrNull<CurseStateComponent>(food.Owner)?.CurseState ?? CurseState.Normal;

            if (curseState >= CurseState.Blessed)
            {
                nutrition = (int)(nutrition * 1.5);
            }
            else if (curseState <= CurseState.Cursed)
            {
                nutrition = (int)(nutrition * 0.5);
            }

            var forceWeightGain = hunger.Nutrition >= HungerLevels.Bloated;
            if (nutrition >= 3000 && (_rand.OneIn(10) || forceWeightGain))
                _weight.ModifyWeight(eater, _rand.Next(3) + 1, forceWeightGain);

            hunger.Nutrition += nutrition;

            var ev2 = new AfterApplyFoodEffectsEvent(eater, nutrition);
            RaiseEvent(food.Owner, ev2);

            ApplyFoodCurseState(eater, curseState, hunger);
        }
    }

    public sealed class AfterFinishedEatingEvent : EntityEventArgs
    {
        public EntityUid Eater { get; }

        public AfterFinishedEatingEvent(EntityUid eater)
        {
            Eater = eater;
        }
    }

    public sealed class BeforeApplyFoodEffectsEvent : EntityEventArgs
    {
        public EntityUid Eater { get; }

        public int OutNutrition { get; set; }
        public List<ExperienceGain> OutExpGains { get; set; }

        public BeforeApplyFoodEffectsEvent(EntityUid eater, int nutrition, List<ExperienceGain> expGains)
        {
            Eater = eater;
            OutNutrition = nutrition;
            OutExpGains = expGains;
        }
    }

    public sealed class AfterApplyFoodEffectsEvent : EntityEventArgs
    {
        public EntityUid Eater { get; }
        public int Nutrition { get; }

        public AfterApplyFoodEffectsEvent(EntityUid eater, int nutrition)
        {
            Eater = eater;
            Nutrition = nutrition;
        }
    }

    public sealed class SpoilFoodEvent : HandledEntityEventArgs
    {
    }
}
