using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Effects.New.EffectAreas;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Formulae;
using OpenNefia.Content.Factions;
using OpenNefia.Content.CurseStates;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Core.Rendering;
using OpenNefia.Content.Rendering;
using OpenNefia.Content.Sanity;
using OpenNefia.Content.Maps;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Qualities;
using OpenNefia.Core.Game;
using OpenNefia.Content.Resists;
using OpenNefia.Content.Mefs;
using OpenNefia.Content.World;
using OpenNefia.Content.Mount;
using OpenNefia.Content.Feats;
using OpenNefia.Core.Log;
using OpenNefia.Content.Combat;
using System.ComponentModel;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.EntityGen;

namespace OpenNefia.Content.Effects.New.EffectDamage
{
    public sealed partial class VanillaEffectDamageSystem : EntitySystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IDamageSystem _damages = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IFormulaEngine _formulas = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly ICommonEffectsSystem _commonEffects = default!;
        [Dependency] private readonly IStatusEffectSystem _statusEffects = default!;
        [Dependency] private readonly IMapDrawablesManager _mapDrawables = default!;
        [Dependency] private readonly ISanitySystem _sanities = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly INewEffectSystem _newEffects = default!;
        [Dependency] private readonly IElementSystem _elements = default!;
        [Dependency] private readonly IMefSystem _mefs = default!;
        [Dependency] private readonly IMountSystem _mounts = default!;
        [Dependency] private readonly IFeatsSystem _feats = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;

        public override void Initialize()
        {
            Initialize_Preconditions();

            SubscribeComponent<EffectDamageMessageComponent, ApplyEffectDamageEvent>(ApplyDamage_DamageMessage, priority: EventPriorities.High - 1000);
            SubscribeComponent<EffectDamageTouchComponent, ApplyEffectDamageEvent>(Apply_Touch, priority: EventPriorities.High - 10000);
            SubscribeComponent<EffectDamageStatusEffectsComponent, ApplyEffectDamageEvent>(Apply_StatusEffects, priority: EventPriorities.High - 5000);

            SubscribeComponent<EffectDamageElementalComponent, GetEffectAnimationParamsEvent>(GetAnimParams_Elemental);
            SubscribeComponent<EffectDamageElementalComponent, ApplyEffectDamageEvent>(ApplyDamage_Elemental);
            SubscribeComponent<EffectDamageElementalComponent, ApplyEffectTileDamageEvent>(ApplyTileDamage_Elemental);

            SubscribeComponent<EffectDamageHealingComponent, ApplyEffectDamageEvent>(ApplyDamage_Healing);
            SubscribeComponent<EffectDamageHealMPComponent, ApplyEffectDamageEvent>(ApplyDamage_HealMP);
            SubscribeComponent<EffectDamageMPComponent, ApplyEffectDamageEvent>(ApplyDamage_DamageMP);
            SubscribeComponent<EffectDamageHealSanityComponent, ApplyEffectDamageEvent>(ApplyDamage_HealSanity);

            SubscribeComponent<EffectSummonComponent, ApplyEffectDamageEvent>(ApplyDamage_Summon);
            SubscribeComponent<EffectSummonCharaComponent, EffectSummonEvent>(Summon_Chara);

            SubscribeComponent<EffectDamageMefComponent, ApplyEffectDamageEvent>(ApplyDamage_Mef);
            SubscribeComponent<EffectDamageMObjComponent, ApplyEffectDamageEvent>(ApplyDamage_MObj);

            SubscribeComponent<EffectTileDamageElementalComponent, ApplyEffectTileDamageEvent>(ApplyTileDamage_Elemental, priority: EventPriorities.High);
        }

        private void Apply_Touch(EntityUid uid, EffectDamageTouchComponent component, ApplyEffectDamageEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:1832 	if cCastStyle(cc)!0{ ...
            if (!IsAlive(args.InnerTarget))
                return;

            var tense = _damages.GetDamageMessageTense(args.InnerTarget.Value);
            var noAttackText = false;

            var rootKey = component.MessageKey;
            if (rootKey != null)
            {
                noAttackText = true;
                var elementStyle = Loc.GetPrototypeString(component.ElementID, "Ego");
                var meleeStyle = CompOrNull<UnarmedDamageTextComponent>(args.Source)?.DamageTextType ?? "Elona.Default";

                var endKey = tense == DamageHPMessageTense.Active ? "Other" : "Ally";

                var localeArgs = new LocaleArg[]
                {
                    ("source", args.Source),
                    ("target", args.InnerTarget.Value),
                    ("elementStyle", elementStyle),
                    ("meleeStyle", (string)meleeStyle)
                };

                if (Loc.TryGetString(rootKey.Value.With(endKey), out var message, localeArgs))
                {
                    _mes.Display(message, entity: args.InnerTarget.Value);
                }
                else if (Loc.TryGetString(rootKey.Value, out message, localeArgs))
                {
                    _mes.Display(message, entity: args.InnerTarget.Value);
                }
            }
            // <<<<<<<< elona122/shade2/proc.hsp:1841 		} ...

            // >>>>>>>> elona122/shade2/proc.hsp:1849 	dmgHP tc,role(dice1,dice2,bonus),cc,ele,eleP ...
            if (component.ApplyDamage)
            {
                var extraArgs = new DamageHPExtraArgs()
                {
                    MessageTense = tense,
                    NoAttackText = noAttackText,
                    AttackerIsMessageSubject = noAttackText
                };
                var damageType = new ElementalDamageType(component.ElementID, args.ElementalPower);
                _damages.DamageHP(args.InnerTarget.Value, args.Damage, args.Source, damageType, extraArgs);
            }
            // <<<<<<<< elona122/shade2/proc.hsp:1849 	dmgHP tc,role(dice1,dice2,bonus),cc,ele,eleP ...

            args.Success();
        }

        private void Apply_StatusEffects(EntityUid uid, EffectDamageStatusEffectsComponent component, ApplyEffectDamageEvent args)
        {
            if (!IsAlive(args.InnerTarget))
                return;

            var vars = _newEffects.GetEffectDamageFormulaArgs(uid, args);
            vars["elementalPower"] = args.ElementalPower;

            foreach (var statusEffect in component.StatusEffects)
            {
                var turns = (int)_formulas.Calculate(statusEffect.Power, vars, 10);
                _statusEffects.Apply(args.InnerTarget.Value, statusEffect.ID, turns);
            }

            args.Success();
        }

        private void GetAnimParams_Elemental(EntityUid uid, EffectDamageElementalComponent component, GetEffectAnimationParamsEvent args)
        {
            if (component.Element != null && _protos.TryIndex(component.Element.Value, out var eleProto))
            {
                args.OutColor = eleProto.Color;
                args.OutSound = eleProto.Sound?.GetSound();
            }
        }

        private void ApplyDamage_DamageMessage(EntityUid uid, EffectDamageMessageComponent component, ApplyEffectDamageEvent args)
        {
            if (!IsAlive(args.InnerTarget))
                return;

            var tense = _damages.GetDamageMessageTense(args.InnerTarget.Value);
            if (_newEffects.TryGetEffectDamageMessage(args.Source, args.InnerTarget.Value, component.RootKey, out var message, tense))
            {
                _mes.Display(message, color: component.Color, entity: args.InnerTarget.Value);
            }
        }

        private void ApplyDamage_Elemental(EntityUid uid, EffectDamageElementalComponent component, ApplyEffectDamageEvent args)
        {
            if (args.InnerTarget == null)
                return;

            var tense = _damages.GetDamageMessageTense(args.InnerTarget.Value);

            var extraArgs = new DamageHPExtraArgs()
            {
                MessageTense = tense,
                NoAttackText = true,
                // The bolt/ball/dart is the one doing the striking, not the player (directly at least)
                // "You hit the putit and {transform} him..."
                //   vs.
                // "The bolt hits the putit and {transforms} him..."
                AttackerIsMessageSubject = false
            };
            IDamageType? damageType = null;
            if (component.Element != null)
                damageType = new ElementalDamageType(component.Element.Value, args.ElementalPower);
            _damages.DamageHP(args.InnerTarget.Value, args.Damage, args.Source, damageType, extraArgs);

            args.Success();
        }

        private void ApplyTileDamage_Elemental(EntityUid uid, EffectDamageElementalComponent component, ApplyEffectTileDamageEvent args)
        {
            if (component.Element != null)
                _elements.DamageTile(args.CoordsMap, component.Element.Value, args.Source);
            args.OutEffectWasObvious = true;
        }

        private void ApplyDamage_Healing(EntityUid uid, EffectDamageHealingComponent healingComp, ApplyEffectDamageEvent args)
        {
            if (!IsAlive(args.InnerTarget) || !HasComp<SkillsComponent>(args.InnerTarget.Value))
                return;

            // >>>>>>>> elona122/shade2/proc.hsp:1817 	if efId=spHealLight 	:if sync(tc):txt lang(name(t ...
            if (_newEffects.TryGetEffectDamageMessage(args.Source, args.InnerTarget.Value, healingComp.MessageKey, out var message))
                _mes.Display(message, entity: args.InnerTarget.Value);

            _commonEffects.Heal(args.InnerTarget.Value, args.Damage);

            if (args.Args.CurseState == CurseState.Blessed)
                _statusEffects.Heal(args.InnerTarget.Value, Protos.StatusEffect.Sick, 5 + _rand.Next(5));
            _commonEffects.MakeSickIfCursed(args.InnerTarget.Value, args.Args.CurseState, 3);

            var anim = new HealMapDrawable(Protos.Asset.HealEffect, Protos.Sound.Heal1);
            _mapDrawables.Enqueue(anim, args.InnerTarget.Value);

            args.Success();
            // <<<<<<<< elona122/shade2/proc.hsp:1826 	call *anime,(animeId=aniHeal) ...
        }

        private void ApplyDamage_HealMP(EntityUid uid, EffectDamageHealMPComponent component, ApplyEffectDamageEvent args)
        {
            if (!IsAlive(args.InnerTarget) || !HasComp<SkillsComponent>(args.InnerTarget.Value))
                return;

            // >>>>>>>> elona122/shade2/proc.hsp:2790 	case actRestoreMP ...
            if (_newEffects.TryGetEffectDamageMessage(args.Source, args.InnerTarget.Value, component.MessageKey, out var message))
                _mes.Display(message, entity: args.InnerTarget.Value);

            _damages.HealMP(args.InnerTarget.Value, args.Damage);

            var anim = new HealMapDrawable(Protos.Asset.HealEffect, Protos.Sound.Heal1);
            _mapDrawables.Enqueue(anim, args.InnerTarget.Value);

            args.Success();
            // <<<<<<<< elona122/shade2/proc.hsp:2796 	swbreak ...
        }

        private void ApplyDamage_DamageMP(EntityUid uid, EffectDamageMPComponent component, ApplyEffectDamageEvent args)
        {
            if (!IsAlive(args.InnerTarget) || !HasComp<SkillsComponent>(args.InnerTarget.Value))
                return;

            // >>>>>>>> elona122/shade2/proc.hsp:2790 	case actRestoreMP ...
            if (_newEffects.TryGetEffectDamageMessage(args.Source, args.InnerTarget.Value, component.MessageKey, out var message))
                _mes.Display(message, entity: args.InnerTarget.Value);

            _damages.DamageMP(args.InnerTarget.Value, args.Damage);

            args.Success();
            // <<<<<<<< elona122/shade2/proc.hsp:2796 	swbreak ...
        }

        private void ApplyDamage_HealSanity(EntityUid uid, EffectDamageHealSanityComponent healComp, ApplyEffectDamageEvent args)
        {
            if (!IsAlive(args.InnerTarget) || !HasComp<SanityComponent>(args.InnerTarget.Value))
                return;

            // >>>>>>>> shade2/proc.hsp:1758 		if (cc=pc)or(cRelation(cc)>=cNeutral){ ...
            var anim = new HealMapDrawable(Protos.Asset.HealEffect, Protos.Sound.Heal1, rotDelta: 5);
            _mapDrawables.Enqueue(anim, args.InnerTarget.Value);
            if (_newEffects.TryGetEffectDamageMessage(args.Source, args.InnerTarget.Value, healComp.MessageKey, out var message))
                _mes.Display(message, entity: args.InnerTarget.Value);
            _sanities.HealInsanity(args.InnerTarget.Value, args.Args.Power / 10);
            _statusEffects.Heal(args.InnerTarget.Value, Protos.StatusEffect.Insanity, 9999);

            args.Success();
            // <<<<<<<< shade2/proc.hsp:1768 			} ..
        }

        private void ApplyDamage_Summon(EntityUid uid, EffectSummonComponent component, ApplyEffectDamageEvent args)
        {
            if (_gameSession.IsPlayer(args.Source)
                && TryComp<MapCharaGenComponent>(args.TargetMap.MapEntityUid, out var mapCharaGen))
            {
                if (mapCharaGen.CurrentCharaCount + 100 > MapCharaGenConsts.MaxOtherCharaCount)
                    return;
            }

            var formulaArgs = _newEffects.GetEffectDamageFormulaArgs(uid, args);
            formulaArgs["finalDamage"] = args.Damage;

            var summonCount = (int)(double.Round(_formulas.Calculate(component.SummonCount, formulaArgs) / args.AffectedTileCount));
            var obvious = false;

            for (var attempts = 0; attempts < 100; attempts++)
            {
                if (summonCount <= 0)
                    break;

                var ev = new EffectSummonEvent(args.Source, args.TargetCoords, args.Damage);
                RaiseEvent(uid, ev);
                if (!IsAlive(ev.OutSummonedEntity))
                    continue;

                summonCount--;
                attempts = 0;
                obvious = true;
            }

            _mes.Display(Loc.GetString(component.MessageKey, ("source", args.Source)), entity: args.Source);

            args.OutEffectWasObvious = obvious;
            args.Success();
        }

        private void Summon_Chara(EntityUid uid, EffectSummonCharaComponent component, EffectSummonEvent args)
        {
            if (component.Choices.Count == 0)
                return;

            var choice = _rand.Pick(component.Choices);

            var filter = choice.CharaFilter;

            if (!choice.NoOverrideLevelAndQuality)
            {
                filter.MinLevel = _randomGen.CalcObjectLevel(args.SummonPower);
                filter.Quality = Quality.Normal;
            }

            var chara = _charaGen.GenerateChara(args.TargetCoords.ToMap(EntityManager), filter);
            if (IsAlive(chara))
            {
                if (!component.CanBeSameTypeAsCaster)
                {
                    if (MetaData(args.Source)?.EntityPrototype?.ID == MetaData(chara.Value)?.EntityPrototype?.ID)
                    {
                        EntityManager.DeleteEntity(chara.Value);
                        return;
                    }
                }

                args.Handle(chara.Value);
            }
        }

        private void ApplyDamage_Mef(EntityUid uid, EffectDamageMefComponent component, ApplyEffectDamageEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:2973 	if efId=actEtherGround	:addMef x,y,mefEther,20,rn ...
            var formulaArgs = _newEffects.GetEffectDamageFormulaArgs(uid, args);

            int? turns = null;
            if (component.Turns != null)
                turns = (int)_formulas.Calculate(component.Turns.Value, formulaArgs, 10f);

            var power = args.Damage;

            var mef = _mefs.SpawnMef(component.MefID, args.TargetCoordsMap,
                duration: turns != null ? GameTimeSpan.FromMinutes(turns.Value) : null,
                power: power,
                spawnedBy: args.Source);

            if (!IsAlive(mef))
                return;

            args.Success();
            // <<<<<<<< elona122/shade2/proc.hsp:2977 	if efId=spMist		:addMef x,y,mefMist,30,8+rnd(15+e ...
        }

        private void ApplyDamage_MObj(EntityUid uid, EffectDamageMObjComponent component, ApplyEffectDamageEvent args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:3298 	case actDropMine ...
            if (HasComp<MapTypeWorldMapComponent>(args.SourceMap.MapEntityUid)
                || _lookup.EntityQueryLiveEntitiesAtCoords<MObjComponent>(args.SourceCoordsMap).Any())
                return;

            var mobj = _entityGen.SpawnEntity(component.MObjID, args.SourceCoordsMap);
            if (!IsAlive(mobj))
                return;

            // TODO IMObjSystem
            if (TryComp<MObjComponent>(mobj.Value, out var mobjComp))
                mobjComp.Source = args.Source;

            _mes.Display(Loc.GetString("Elona.Effect.MObj.Drops", ("source", args.Source), ("target", args.InnerTarget), ("mobj", mobj.Value)));
            args.Success();
            // <<<<<<<< elona122/shade2/proc.hsp:3303 	swbreak ...
        }

        private void ApplyTileDamage_Elemental(EntityUid uid, EffectTileDamageElementalComponent component, ApplyEffectTileDamageEvent args)
        {
            _elements.DamageTile(args.CoordsMap, component.Element, args.Source);
            args.OutEffectWasObvious = true;
        }
    }

    [EventUsage(EventTarget.Effect)]
    public sealed class EffectSummonEvent : HandledEntityEventArgs
    {
        public EffectSummonEvent(EntityUid source, EntityCoordinates targetCoords, int summonPower)
        {
            Source = source;
            TargetCoords = targetCoords;
            SummonPower = summonPower;
        }

        public EntityUid Source { get; }
        public EntityCoordinates TargetCoords { get; }
        public int SummonPower { get; }

        public EntityUid? OutSummonedEntity { get; set; } = null;

        public void Handle(EntityUid entity)
        {
            Handled = true;
            OutSummonedEntity = entity;
        }
    }
}