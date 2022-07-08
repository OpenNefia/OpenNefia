using OpenNefia.Content.Charas;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Dialog;
using OpenNefia.Content.EmotionIcon;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Fame;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;

namespace OpenNefia.Content.GameObjects
{
    public interface IActionBashSystem : IEntitySystem
    {
        TurnResult DoBash(EntityUid uid, MapCoordinates bashPos);
    }

    public sealed class ActionBashSystem : EntitySystem, IActionBashSystem
    {
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IStatusEffectSystem _statusEffects = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly ITargetingSystem _targeting = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IDamageSystem _damage = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IDialogSystem _dialog = default!;
        [Dependency] private readonly IKarmaSystem _karma = default!;
        [Dependency] private readonly IEmotionIconSystem _emoIcons = default!;

        public override void Initialize()
        {
            SubscribeComponent<CharaComponent, EntityBashedEventArgs>(HandleBashed, priority: EventPriorities.VeryLow);
        }

        public TurnResult DoBash(EntityUid uid, MapCoordinates bashPos)
        {
            // >>>>>>>> shade2/action.hsp:388     if map(x,y,5)!0{ ..
            foreach (var ent in _lookup.GetLiveEntitiesAtCoords(bashPos))
            {
                var ev = new EntityBashedEventArgs(uid);
                if (Raise(ent.Owner, ev))
                    return ev.TurnResult;
            }

            _mes.Display(Loc.GetString("Elona.Bash.Air", ("basher", uid)));
            _audio.Play(Protos.Sound.Miss, bashPos);
            return TurnResult.Succeeded;
            // <<<<<<<< shade2/action.hsp:467     goto *turn_end ..
        }

        private void HandleBashed(EntityUid target, CharaComponent _, EntityBashedEventArgs args)
        {
            if (args.Handled)
                return;

            if (!_statusEffects.HasEffect(target, Protos.StatusEffect.Sleep))
            {
                if (_factions.IsPlayer(args.Basher) && _factions.GetRelationTowards(target, args.Basher) >= Relation.Neutral)
                {
                    if (!_targeting.PromptReallyAttack(args.Basher, target))
                    {
                        args.Handle(TurnResult.Aborted);
                        return;
                    }
                }

                _audio.Play(Protos.Sound.Bash1, target);

                if (_statusEffects.HasEffect(target, Protos.StatusEffect.Choking))
                {
                    _audio.Play(Protos.Sound.Bash1, target);
                    _mes.Display(Loc.GetString("Elona.Bash.Choking.Execute", ("basher", args.Basher), ("target", target)));
                    var result = _damage.DamageHP(target, _skills.Level(args.Basher, Protos.Skill.AttrStrength) * 5, args.Basher);
                    if (!result.WasKilled)
                    {
                        _mes.Display(Loc.GetString("Elona.Bash.Choking.Spits", ("target", target)));
                        _mes.Display(Loc.GetString("Elona.Bash.Choking.Dialog", ("target", target)));
                        _statusEffects.Remove(target, Protos.StatusEffect.Choking);
                        _dialog.ModifyImpression(target, 10);
                    }
                }
                else
                {
                    _mes.Display(Loc.GetString("Elona.Bash.Execute", ("basher", args.Basher), ("target", target)));
                    _factions.ActHostileTowards(args.Basher, target);
                }
            }
            else
            {
                _audio.Play(Protos.Sound.Bash1, target);
                _mes.Display(Loc.GetString("Elona.Bash.Execute", ("basher", args.Basher), ("target", target)));
                _mes.Display(Loc.GetString("Elona.Bash.DisturbsSleep", ("basher", args.Basher), ("target", target)));
                _karma.ModifyKarma(args.Basher, -1);
                _emoIcons.SetEmotionIcon(target, EmotionIcons.Angry, 4);
            }

            _statusEffects.Remove(target, Protos.StatusEffect.Sleep);
            args.Handle(TurnResult.Succeeded);
        }
    }

    public sealed class EntityBashedEventArgs : TurnResultEntityEventArgs
    {
        public EntityUid Basher { get; }

        public EntityBashedEventArgs(EntityUid basher)
        {
            Basher = basher;
        }
    }
}