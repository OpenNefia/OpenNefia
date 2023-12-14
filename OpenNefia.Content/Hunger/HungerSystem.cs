using OpenNefia.Content.Damage;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Feats;
using OpenNefia.Content.Food;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Hud;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Mount;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Sleep;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.UI;
using OpenNefia.Content.Visibility;
using OpenNefia.Content.Weight;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Activity;
using OpenNefia.Content.Items;
using OpenNefia.Core;

namespace OpenNefia.Content.Hunger
{
    public interface IHungerSystem : IEntitySystem
    {
        string GetNutritionMessage(int newNutrition);
        bool VomitIfAnorexic(EntityUid entity, HungerComponent? hunger = null);
        void Vomit(EntityUid entity, HungerComponent? hunger = null);
        void CureAnorexia(EntityUid entity, HungerComponent? hunger = null);
    }

    public sealed class HungerSystem : EntitySystem, IHungerSystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IVisibilitySystem _vis = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;
        [Dependency] private readonly IStatusEffectSystem _effects = default!;
        [Dependency] private readonly IWeightSystem _weight = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IDamageSystem _damage = default!;
        [Dependency] private readonly IFeatsSystem _feats = default!;
        [Dependency] private readonly IMountSystem _mounts = default!;
        [Dependency] private readonly ITurnOrderSystem _turnOrder = default!;
        [Dependency] private readonly IActivitySystem _activities = default!;

        public override void Initialize()
        {
            SubscribeComponent<HungerComponent, EntityBeingGeneratedEvent>(InitializeNutrition, priority: EventPriorities.High);
            SubscribeComponent<HungerComponent, GetStatusIndicatorsEvent>(AddStatusIndicator);
            SubscribeComponent<HungerComponent, OnCharaSleepEvent>(HandleCharaSleep);
            SubscribeComponent<HungerComponent, EntityRefreshSpeedEvent>(HandleRefreshSpeed, priority: EventPriorities.VeryHigh);
            SubscribeComponent<HungerComponent, EntityTurnEndingEventArgs>(HandleTurnEnding, priority: EventPriorities.VeryHigh);
        }

        private void AddStatusIndicator(EntityUid uid, HungerComponent hunger, GetStatusIndicatorsEvent ev)
        {
            var nutrition = hunger.Nutrition;
            var nutritionLevel = Math.Clamp(nutrition / 1000, 0, 12);
            if (5 <= nutritionLevel && nutritionLevel <= 9)
                return;

            Color color;
            if (nutrition >= HungerLevels.Bloated)
                color = UiColors.TextBlack;
            else if (nutrition >= HungerLevels.VeryHungry)
            {
                if (nutrition <= HungerLevels.Normal - 1000)
                    color = UiColors.HungerIndicatorHungry;
                else
                    color = UiColors.TextBlack;
            }
            else
                color = UiColors.HungerIndicatorStarving;

            ev.OutIndicators.Add(new() { Text = Loc.GetString($"Elona.Hunger.Indicator.{nutritionLevel}"), Color = color });
        }

        private void InitializeNutrition(EntityUid uid, HungerComponent component, ref EntityBeingGeneratedEvent args)
        {
            // >>>>>>>> shade2/chara.hsp:516 	if rc=pc:cHunger(rc)=9000:else:cHunger(rc)=defAll ..
            if (_gameSession.IsPlayer(uid))
                component.Nutrition = 9000;
            else
                component.Nutrition = HungerLevels.Ally - 1000 + _rand.Next(4000);
            // <<<<<<<< shade2/chara.hsp:516 	if rc=pc:cHunger(rc)=9000:else:cHunger(rc)=defAll ..
        }

        public string GetNutritionMessage(int newNutrition)
        {
            LocaleKey key;
            if (newNutrition >= HungerLevels.Bloated)
                key = "Bloated";
            else if (newNutrition >= HungerLevels.Satisfied)
                key = "Satisfied";
            else if (newNutrition >= HungerLevels.Normal)
                key = "Normal";
            else if (newNutrition >= HungerLevels.Hungry)
                key = "Hungry";
            else if (newNutrition >= HungerLevels.VeryHungry)
                key = "VeryHungry";
            else
                key = "Starving";

            return Loc.GetString(new LocaleKey("Elona.Food.Nutrition").With(key));
        }

        private void HandleCharaSleep(EntityUid uid, HungerComponent hunger, OnCharaSleepEvent args)
        {
            if (hunger.IsAnorexic)
            {
                hunger.AnorexiaCounter -= _rand.Next(6);
            }
            else
            {
                hunger.AnorexiaCounter -= _rand.Next(3);
            }

            if (hunger.AnorexiaCounter < 0)
            {
                CureAnorexia(uid, hunger);
                hunger.AnorexiaCounter = 0;
            }
        }

        private void HandleRefreshSpeed(EntityUid uid, HungerComponent hunger, ref EntityRefreshSpeedEvent args)
        {
            if (!_gameSession.IsPlayer(uid) || _mounts.HasMount(uid))
                return;

            if (hunger.Nutrition < HungerLevels.VeryHungry)
                args.OutSpeedModifier -= 0.3f;
            if (hunger.Nutrition < HungerLevels.Hungry)
                args.OutSpeedModifier -= 0.1f;
        }

        public bool VomitIfAnorexic(EntityUid entity, HungerComponent? hunger = null)
        {
            if (Resolve(entity, ref hunger) && hunger.IsAnorexic)
            {
                Vomit(entity);
                return true;
            }

            return false;
        }

        public void CureAnorexia(EntityUid entity, HungerComponent? hunger = null)
        {
            if (!Resolve(entity, ref hunger) || !hunger.IsAnorexic)
                return;

            hunger.IsAnorexic = false;
            if (_vis.IsInWindowFov(entity) || _parties.IsInPlayerParty(entity))
            {
                _mes.Display(Loc.GetString("Elona.Hunger.Anorexia.RecoversFrom", ("entity", entity)));
                _audio.Play(Protos.Sound.Offer1, entity);
            }
        }

        public void Vomit(EntityUid entity, HungerComponent? hunger = null)
        {
            // >>>>>>>> shade2/chara_func.hsp:1890 #deffunc chara_vomit int c ...
            if (!Resolve(entity, ref hunger))
                return;

            hunger.AnorexiaCounter++;

            if (_vis.IsInWindowFov(entity))
            {
                _audio.Play(Protos.Sound.Vomit, entity);
                _mes.Display(Loc.GetString("Elona.Hunger.Vomits", ("entity", entity)));
            }

            var ev = new BeforeVomitEvent();
            RaiseEvent(entity, ev);

            // TODO food buffs

            if (TryMap(entity, out var map))
            {
                if (!HasComp<MapTypeWorldMapComponent>(map.MapEntityUid))
                {
                    var addVomitChance = 2;
                    foreach (var metaData in _lookup.EntityQueryInMap<MetaDataComponent>(map))
                    {
                        if (metaData.EntityPrototype?.GetStrongID() == Protos.Item.Vomit)
                        {
                            addVomitChance++;
                        }
                    }

                    if (_gameSession.IsPlayer(entity) || _rand.OneIn((int)Math.Pow(addVomitChance, 3)))
                    {
                        var vomit = _entityGen.SpawnEntity(Protos.Item.Vomit, entity);
                        if (IsAlive(vomit) && TryProtoID(entity, out var id))
                            EnsureComp<EntityProtoSourceComponent>(vomit.Value).EntityID = id.Value;
                    }
                }
            }

            if (hunger.IsAnorexic)
            {
                _skills.GainFixedSkillExp(entity, Protos.Skill.AttrStrength, -50);
                _skills.GainFixedSkillExp(entity, Protos.Skill.AttrConstitution, -75);
                _skills.GainFixedSkillExp(entity, Protos.Skill.AttrCharisma, -100);
            }
            else
            {
                if ((_parties.IsInPlayerParty(entity) && hunger.AnorexiaCounter > 10) || (!_parties.IsInPlayerParty(entity) && _rand.OneIn(4)))
                {
                    if (_rand.OneIn(5))
                    {
                        hunger.IsAnorexic = true;
                        if (_vis.IsInWindowFov(entity))
                        {
                            _mes.Display(Loc.GetString("Elona.Hunger.Anorexia.Develops", ("entity", entity)), entity: entity);
                            _audio.Play(Protos.Sound.Offer1, entity);
                        }
                    }
                }
            }

            _effects.Apply(entity, Protos.StatusEffect.Dimming, 100);
            _weight.ModifyWeight(entity, -(1 + _rand.Next(5)));

            if (hunger.Nutrition <= 0)
                _damage.DamageHP(entity, 9999, damageType: new GenericDamageType("Elona.DamageType.Starvation"));

            hunger.Nutrition -= 3000;
            // <<<<<<<< shade2/chara_func.hsp:1927 	return ..
        }

        public const int HungerDecrementAmount = 8;

        public void MakeHungry(EntityUid chara, HungerComponent? hunger = null)
        {
            if (!Resolve(chara, ref hunger))
                return;

            if (_gameSession.IsPlayer(chara))
            {
                if (_feats.HasFeat(chara, Protos.Feat.PermSlowFood) && _rand.OneIn(3))
                    return;

                var oldLevel = hunger.Nutrition / 1000;
                hunger.Nutrition -= HungerDecrementAmount;
                var newLevel = hunger.Nutrition / 1000;

                if (newLevel != oldLevel)
                {
                    if (oldLevel == HungerLevels.VeryHungry / 1000)
                        _mes.Display(Loc.GetString("Elona.Hunger.Status.Starving"));
                    if (oldLevel == HungerLevels.Hungry / 1000)
                        _mes.Display(Loc.GetString("Elona.Hunger.Status.VeryHungry"));
                    if (oldLevel == HungerLevels.Normal / 1000)
                        _mes.Display(Loc.GetString("Elona.Hunger.Status.Hungry"));
                }

                _turnOrder.RefreshSpeed(chara);
            }
            else
            {
                if (!TryMap(chara, out var map) || HasComp<MapTypeWorldMapComponent>(map.MapEntityUid))
                    return;

                hunger.Nutrition -= HungerDecrementAmount * 2;
                if (hunger.Nutrition < HungerLevels.Ally && !hunger.IsAnorexic)
                    hunger.Nutrition = HungerLevels.Ally;
            }
        }

        private void HandleTurnEnding(EntityUid uid, HungerComponent component, EntityTurnEndingEventArgs args)
        {
            MakeHungry(uid, component);

            if (_gameSession.IsPlayer(uid))
            {
                if (component.Nutrition < HungerLevels.Hungry && TryComp<SkillsComponent>(uid, out var skills))
                {
                    skills.CanRegenerateThisTurn = false;
                }

                if (component.Nutrition < HungerLevels.VeryHungry)
                {
                    if (!_activities.HasActivity(uid, Protos.Activity.Eating) && TryComp<SkillsComponent>(uid, out skills))
                    {
                        _audio.Play(Protos.Sound.Fizzle);
                        _damage.DamageHP(uid, _rand.Next(2) + skills.MaxHP / 50, damageType: new GenericDamageType("Elona.DamageType.Hunger"));
                        if (TryComp<TurnOrderComponent>(uid, out var turnOrder) && turnOrder.TotalTurnsTaken % 10 == 0)
                        {
                            _activities.InterruptActivity(uid);
                            if (_rand.OneIn(50))
                            {
                                _weight.ModifyWeight(uid, -1);
                            }
                        }
                    }
                }
            }
        }
    }

    public sealed class BeforeVomitEvent : EntityEventArgs
    {
    }
}