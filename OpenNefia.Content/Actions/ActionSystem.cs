using CSharpRepl.Services.Roslyn.Formatting;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Spells;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Levels;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Effects;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Content.Effects.New;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Content.Damage;

namespace OpenNefia.Content.Actions
{
    public interface IActionSystem : IEntitySystem
    {
        bool HasAction(EntityUid caster, PrototypeId<ActionPrototype> action);
        TurnResult Invoke(EntityUid caster, PrototypeId<ActionPrototype> action, EntityUid? target = null);
        string LocalizeActionDescription(ActionPrototype proto, EntityUid casterEntity, EntityUid effect);
    }

    public sealed class ActionSystem : EntitySystem, IActionSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly ILevelSystem _levels = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly INewEffectSystem _newEffects = default!;
        [Dependency] private readonly IStatusEffectSystem _statusEffects = default!;
        [Dependency] private readonly IDamageSystem _damages = default!;

        public override void Initialize()
        {
            SubscribeEntity<BeforeActionEffectInvokedEvent>(BeforeInvoke_ProcConfusionBlindness);
        }

        /// <summary>
        /// NOTE: In 1.22, the type of action (tgSelf, tgSelfOnly) was checked to proc blindness.
        /// Here, it is proc'd if the target is not the caster themselves.
        /// </summary>
        private void BeforeInvoke_ProcConfusionBlindness(EntityUid uid, BeforeActionEffectInvokedEvent args)
        {
            if (args.Handled)
                return;

            // >>>>>>>> elona122/shade2/proc.hsp:1549 	if (skillTg(efId)!tgSelf)&(skillTg(efId)!tgSelfOn ...
            if (IsAlive(args.Target) && args.Target.Value == args.Caster)
                return;

            if (_statusEffects.HasEffect(args.Caster, Protos.StatusEffect.Confusion)
                || _statusEffects.HasEffect(args.Caster, Protos.StatusEffect.Blindness))
            {
                if (_rand.OneIn(5))
                {
                    _mes.Display(Loc.GetString("Elona.Action.Failure.ShakesHead", ("entity", args.Caster)), entity: args.Caster);
                    args.Handle(TurnResult.Failed);
                }
            }
            // <<<<<<<< elona122/shade2/proc.hsp:1552 		} ...
        }

        public bool HasAction(EntityUid caster, PrototypeId<ActionPrototype> action)
        {
            var proto = _protos.Index(action);
            return _skills.HasSkill(caster, proto.SkillID);
        }

        public TurnResult Invoke(EntityUid caster, PrototypeId<ActionPrototype> actionID, EntityUid? target = null)
        {
            // >>>>>>>> shade2/proc.hsp:1539 *action ...
            // TODO death word
            if (!_protos.TryIndex(actionID, out var action)
                || !_protos.TryIndex(action.SkillID, out var skill))
                return TurnResult.Aborted;

            if (!HasAction(caster, actionID))
                return TurnResult.Aborted;

            if (!_newEffects.TrySpawnEffect(action.EffectID, out var effect))
                return TurnResult.Aborted;

            var power = CalcActionPower(action, caster);

            var effectCommon = new EffectCommonArgs()
            {
                EffectSource = EffectSources.Action,
                CurseState = CurseStates.CurseState.Normal,
                Power = power,
                MaxRange = action.MaxRange
            };
            var effectArgs = EffectArgSet.Make(effectCommon);

            EntityCoordinates? targetCoords;

            if (IsAlive(target))
            {
                targetCoords = Spatial(target.Value).Coordinates;
            }
            else
            {
                if (!_newEffects.TryGetEffectTarget(caster, effect.Value, effectArgs, out var targetPair))
                    return TurnResult.Aborted;
                target = targetPair.Target;
                targetCoords = targetPair.Coords;
            }

            var ev = new BeforeActionEffectInvokedEvent(caster, target, targetCoords);
            RaiseEvent(effect.Value, ev);
            if (ev.Handled)
                return ev.TurnResult;

            if (!_damages.DoStaminaCheck(caster, action.StaminaCost, skill.RelatedSkill))
            {
                _mes.Display(Loc.GetString("Elona.Common.TooExhausted", ("entity", caster)));
                return TurnResult.Failed;
            }

            var result = _newEffects.Apply(caster, target, targetCoords, effect.Value, args: effectArgs);

            if (IsAlive(effect))
                EntityManager.DeleteEntity(effect.Value);

            return result;
            // <<<<<<<< shade2/proc.hsp:1557 	return true ..
        }

        public int CalcActionPower(ActionPrototype spell, EntityUid caster)
        {
            // >>>>>>>> elona122/shade2/calculation.hsp:864 	if id>=rangeSdata{ ...
            // NOTE: In vanilla, action power was calculated by multipying the enum
            // value of the skill's type (bolt, arrow, ball, etc.) and
            // adding 10. This means bolt spells were the least powerful and
            // ball spells were more powerful. This is changed here (and in
            // omake overhaul) to be the skill level of the action or the
            // character's level.
            if (!_gameSession.IsPlayer(caster))
            {
                return _levels.GetLevel(caster) * 10 + 50;
            }

            if (_skills.TryGetKnown(caster, spell.SkillID, out var skill))
            {
                return skill.Level.Buffed * 10 + 50;
            }

            return 100;
            // <<<<<<<< elona122/shade2/calculation.hsp:867 		} ...
        }

        public string LocalizeActionDescription(ActionPrototype proto, EntityUid caster, EntityUid effect)
        {
            var description = string.Empty;
            if (Loc.TryGetPrototypeString(proto.SkillID, "Description", out var desc))
                description = desc;

            var power = CalcActionPower(proto, caster);
            var skillLevel = _skills.Level(caster, proto.SkillID);
            var maxRange = proto.MaxRange;

            var ev = new GetEffectDescriptionEvent(caster, power, skillLevel, maxRange, description);
            RaiseEvent(effect, ev);

            return ev.OutDescription;
        }
    }

    [EventUsage(EventTarget.Effect)]
    public sealed class BeforeActionEffectInvokedEvent : TurnResultEntityEventArgs
    {
        public BeforeActionEffectInvokedEvent(EntityUid caster, EntityUid? target, EntityCoordinates? coords)
        {
            Caster = caster;
            Target = target;
            Coords = coords;
        }

        public EntityUid Caster { get; }
        public EntityUid? Target { get; }
        public EntityCoordinates? Coords { get; }
    }
}