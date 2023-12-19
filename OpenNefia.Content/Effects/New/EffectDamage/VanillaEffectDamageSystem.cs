using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
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
using OpenNefia.Content.Skills;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Damage;
using NetVips;
using OpenNefia.Content.Effects.New.EffectAreas;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Formulae;
using OpenNefia.Content.Combat;
using OpenNefia.Content.Factions;
using OpenNefia.Core;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using OpenNefia.Content.CurseStates;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Core.Rendering;
using OpenNefia.Content.Rendering;
using OpenNefia.Content.Sanity;
using System.IO.Pipelines;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Maps;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Qualities;
using OpenNefia.Core.Game;

namespace OpenNefia.Content.Effects.New.EffectDamage
{
    public sealed class VanillaEffectDamageSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IDamageSystem _damages = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly ISpatialSystem _spatials = default!;
        [Dependency] private readonly IFormulaEngine _formulaEngine = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly ICommonEffectsSystem _commonEffects = default!;
        [Dependency] private readonly ICurseStateSystem _curseStates = default!;
        [Dependency] private readonly IStatusEffectSystem _statusEffects = default!;
        [Dependency] private readonly IMapDrawablesManager _mapDrawables = default!;
        [Dependency] private readonly ISanitySystem _sanities = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;

        public override void Initialize()
        {
            SubscribeComponent<EffectBaseDamageDiceComponent, ApplyEffectDamageEvent>(ApplyDamage_Dice, priority: EventPriorities.VeryHigh - 10000);
            SubscribeComponent<EffectDamageRelationsComponent, ApplyEffectDamageEvent>(ApplyDamage_Relations, priority: EventPriorities.VeryHigh - 2000);
            SubscribeComponent<EffectDamageControlMagicComponent, ApplyEffectDamageEvent>(ApplyDamage_ControlMagic, priority: EventPriorities.VeryHigh - 1000);

            SubscribeComponent<EffectDamageElementalComponent, GetEffectAnimationParamsEvent>(GetAnimParams_Elemental);
            SubscribeComponent<EffectDamageElementalComponent, ApplyEffectDamageEvent>(ApplyDamage_Elemental);

            SubscribeComponent<EffectDamageHealingComponent, ApplyEffectDamageEvent>(ApplyDamage_Healing);
            SubscribeComponent<EffectDamageHealSanityComponent, ApplyEffectDamageEvent>(ApplyDamage_HealSanity);

            SubscribeComponent<EffectSummonComponent, ApplyEffectDamageEvent>(ApplyDamage_Summon);
            SubscribeComponent<EffectSummonCharaComponent, EffectSummonEvent>(Summon_Chara);
        }

        private IDictionary<string, double> GetEffectDamageFormulaArgs(EntityUid uid, EntityUid source, EntityUid? target, EntityCoordinates sourceCoords, EntityCoordinates targetCoords, EffectArgSet args)
        {
            var result = new Dictionary<string, double>();

            result["power"] = args.Power;
            result["skillLevel"] = args.SkillLevel;
            result["casterLevel"] = _levels.GetLevel(source);
            result["targetLevel"] = target != null ? _levels.GetLevel(target.Value) : 0;
            if (sourceCoords.TryDistanceFractional(EntityManager, targetCoords, out var dist))
            {
                result["distance"] = dist;
            }

            return result;
        }

        private void ApplyDamage_Dice(EntityUid uid, EffectBaseDamageDiceComponent component, ApplyEffectDamageEvent args)
        {
            if (args.Handled)
                return;

            var formulaArgs = GetEffectDamageFormulaArgs(uid, args.Source, args.InnerTarget, args.SourceCoords, args.TargetCoords, args.Args);

            var diceX = int.Max((int)_formulaEngine.Calculate(component.DiceX, formulaArgs, 1f), 1);
            var diceY = int.Max((int)_formulaEngine.Calculate(component.DiceY, formulaArgs, 1f), 1);
            var bonus = (int)_formulaEngine.Calculate(component.Bonus, formulaArgs, 0f);
            args.OutElementalPower = (int)_formulaEngine.Calculate(component.ElementPower, formulaArgs, 0f);

            var dice = new Dice(diceX, diceY, bonus);
            var baseDamage = dice.Roll(_rand);

            formulaArgs["baseDamage"] = baseDamage;

            args.OutDamage = (int)_formulaEngine.Calculate(component.FinalDamage, formulaArgs, baseDamage);
        }

        /// <summary>
        /// Filter effect targets by relation.
        /// </summary>
        private void ApplyDamage_Relations(EntityUid uid, EffectDamageRelationsComponent component, ApplyEffectDamageEvent args)
        {
            if (args.Handled)
                return;

            // Null check instead of liveness
            // (entity might be dead and player could revive them)
            if (args.InnerTarget == null)
            {
                args.Handle(TurnResult.Failed);
                return;
            }

            var relation = _factions.GetRelationTowards(args.Source, args.InnerTarget.Value);
            if (!component.ValidRelations.Includes(relation))
            {
                args.Handle(TurnResult.Failed);
                return;
            }
        }

        private enum ControlMagicStatus
        {
            Success,
            Partial,
            Failure
        }
        private record class ControlMagicResult(ControlMagicStatus Status, int NewDamage);

        private ControlMagicResult ProcControlMagic(EntityUid source, EntityUid target, int damage)
        {
            if (!_skills.TryGetKnown(source, Protos.Skill.ControlMagic, out var controlMagicLv)
                || !_parties.IsInSameParty(source, target))
            {
                return new(ControlMagicStatus.Failure, damage);
            }

            if (controlMagicLv.Level.Buffed * 5 > _rand.Next(damage + 1))
            {
                damage = 0;
            }
            else
            {
                damage = _rand.Next(damage * 100 / (100 + controlMagicLv.Level.Buffed * 10) + 1);
            }

            return new(damage <= 0 ? ControlMagicStatus.Success : ControlMagicStatus.Partial, damage);
        }

        private void ApplyDamage_ControlMagic(EntityUid uid, EffectDamageControlMagicComponent component, ApplyEffectDamageEvent args)
        {
            if (args.Handled || args.InnerTarget == null)
                return;

            var result = ProcControlMagic(args.Source, args.InnerTarget.Value, args.OutDamage);
            args.OutDamage = result.NewDamage;

            switch (result.Status)
            {
                case ControlMagicStatus.Success:
                    _mes.Display(Loc.GetString("Elona.Magic.ControlMagic.PassesThrough", ("target", args.InnerTarget)), entity: args.InnerTarget);
                    args.Handle(TurnResult.Succeeded);
                    break;
                case ControlMagicStatus.Partial:
                    _skills.GainSkillExp(args.Source, Protos.Skill.ControlMagic, 30, 2);
                    break;
                case ControlMagicStatus.Failure:
                default:
                    break;
            }
        }

        private void GetAnimParams_Elemental(EntityUid uid, EffectDamageElementalComponent component, GetEffectAnimationParamsEvent args)
        {
            if (component.Element != null && _protos.TryIndex(component.Element.Value, out var eleProto))
            {
                args.OutColor = eleProto.Color;
                args.OutSound = eleProto.Sound?.GetSound();
            }
        }

        private bool TryGetEffectDamageMessage(EntityUid innerTarget, LocaleKey rootKey, [NotNullWhen(true)] out string? message, DamageHPMessageTense? tense = null)
        {
            tense ??= _damages.GetDamageMessageTense(innerTarget);
            var endKey = tense == DamageHPMessageTense.Active ? "Other" : "Ally";
            if (Loc.TryGetString(rootKey.With(endKey), out message, ("entity", innerTarget)))
            {
                return true;
            }

            if (Loc.TryGetString(rootKey, out message, ("entity", innerTarget)))
            {
                return true;
            }

            message = null;
            return false;
        }

        /// <summary>
        /// Shows a message like "The bolt hits the putit and" or "The bolt hits the putit.".
        /// The rest of the message is displayed in <see cref="IDamageSystem.DamageHP"/>.
        /// </summary>
        private void DisplayEffectDamageMessage(EntityUid uid, EntityUid innerTarget, DamageHPMessageTense? tense = null)
        {
            if (TryComp<EffectDamageMessageComponent>(uid, out var damageMessage)
                && TryGetEffectDamageMessage(innerTarget, damageMessage.RootKey, out var message, tense))
            {
                _mes.Display(message, entity: innerTarget);
            }
        }

        private void ApplyDamage_Elemental(EntityUid uid, EffectDamageElementalComponent component, ApplyEffectDamageEvent args)
        {
            if (args.Handled || args.InnerTarget == null)
                return;

            var tense = _damages.GetDamageMessageTense(args.InnerTarget.Value);
            DisplayEffectDamageMessage(uid, args.InnerTarget.Value, tense);

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
                damageType = new ElementalDamageType(component.Element.Value, args.OutElementalPower);
            _damages.DamageHP(args.InnerTarget.Value, args.OutDamage, args.Source, damageType, extraArgs);
        }

        private void ApplyDamage_Healing(EntityUid uid, EffectDamageHealingComponent healingComp, ApplyEffectDamageEvent args)
        {
            if (args.Handled || args.InnerTarget == null)
                return;

            // >>>>>>>> elona122/shade2/proc.hsp:1817 	if efId=spHealLight 	:if sync(tc):txt lang(name(t ...
            if (TryGetEffectDamageMessage(args.InnerTarget.Value, healingComp.MessageKey, out var message))
                _mes.Display(message, entity: args.InnerTarget.Value);

            _commonEffects.Heal(args.InnerTarget.Value, args.OutDamage);

            if (args.Args.CurseState == CurseState.Blessed)
                _statusEffects.Heal(args.InnerTarget.Value, Protos.StatusEffect.Sick, 5 + _rand.Next(5));
            _commonEffects.MakeSickIfCursed(args.InnerTarget.Value, args.Args.CurseState, 3);

            var anim = new HealMapDrawable(Protos.Asset.HealEffect, Protos.Sound.Heal1);
            _mapDrawables.Enqueue(anim, args.InnerTarget.Value);

            args.Handle(TurnResult.Succeeded);
            // <<<<<<<< elona122/shade2/proc.hsp:1826 	call *anime,(animeId=aniHeal) ...
        }

        private void ApplyDamage_HealSanity(EntityUid uid, EffectDamageHealSanityComponent healComp, ApplyEffectDamageEvent args)
        {
            if (args.Handled || args.InnerTarget == null)
                return;

            // >>>>>>>> shade2/proc.hsp:1758 		if (cc=pc)or(cRelation(cc)>=cNeutral){ ...
            var anim = new HealMapDrawable(Protos.Asset.HealEffect, Protos.Sound.Heal1, rotDelta: 5);
            _mapDrawables.Enqueue(anim, args.InnerTarget.Value);
            if (TryGetEffectDamageMessage(args.InnerTarget.Value, healComp.MessageKey, out var message))
                _mes.Display(message, entity: args.InnerTarget.Value);
            _sanities.HealInsanity(args.InnerTarget.Value, args.Args.Power / 10);
            _statusEffects.Heal(args.InnerTarget.Value, Protos.StatusEffect.Insanity, 9999);
            // <<<<<<<< shade2/proc.hsp:1768 			} ..
        }

        private void ApplyDamage_Summon(EntityUid uid, EffectSummonComponent component, ApplyEffectDamageEvent args)
        {
            if (_gameSession.IsPlayer(args.Source)
                && TryComp<MapCharaGenComponent>(args.TargetMap.MapEntityUid, out var mapCharaGen))
            {
                if (mapCharaGen.CurrentCharaCount + 100 > MapCharaGenConsts.MaxOtherCharaCount)
                {
                    // TODO combine nothing happens messages
                    _mes.Display(Loc.GetString("Elona.Common.NothingHappens"));
                    args.Args.CommonArgs.OutEffectWasObvious = false;
                    args.Handle(TurnResult.Failed);
                    return;
                }
            }

            var formulaArgs = GetEffectDamageFormulaArgs(uid, args.Source, args.InnerTarget, args.SourceCoords, args.TargetCoords, args.Args);
            formulaArgs["finalDamage"] = args.OutDamage;

            var summonCount = (int)(double.Round(_formulaEngine.Calculate(component.SummonCount, formulaArgs) / args.AffectedTileCount));
            var obvious = false;

            for (var attempts = 0; attempts < 100; attempts++)
            {
                if (summonCount <= 0)
                    break;

                var ev = new EffectSummonEvent(args.Source, args.TargetCoords, args.OutDamage);
                RaiseEvent(uid, ev);
                if (!IsAlive(ev.OutSummonedEntity))
                    continue;

                summonCount--;
                attempts = 0;
                obvious = true;
            }

            _mes.Display(Loc.GetString(component.MessageKey, ("source", args.Source)), entity: args.Source);

            args.Args.CommonArgs.OutEffectWasObvious = obvious;
            args.Handle(TurnResult.Succeeded);
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