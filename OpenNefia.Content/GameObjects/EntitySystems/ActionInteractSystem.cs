using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using OpenNefia.Content.Charas;
using OpenNefia.Content.Combat;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Dialog;
using OpenNefia.Content.EmotionIcon;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Fame;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.UserInterface;

namespace OpenNefia.Content.GameObjects
{
    public interface IActionInteractSystem : IEntitySystem
    {
        TurnResult PromptInteract(EntityUid source);
        TurnResult InteractWith(EntityUid source, EntityUid target);
    }

    public sealed partial class ActionInteractSystem : EntitySystem, IActionInteractSystem
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
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;

        public override void Initialize()
        {
            SubscribeEntity<GetInteractActionsEvent>(GetDefaultInteractActions, EventPriorities.Highest);
        }

        private void GetDefaultInteractActions(EntityUid uid, GetInteractActionsEvent args)
        {
            if (!HasComp<CharaComponent>(uid))
                return;

            if (!_factions.IsPlayer(uid))
            {
                if (!_statusEffects.HasEffect(args.Source, Protos.StatusEffect.Confusion))
                {
                    args.OutInteractActions.Add(new(Loc.GetString("Elona.Interact.Actions.Talk"), InteractAction_Talk));
                    args.OutInteractActions.Add(new(Loc.GetString("Elona.Interact.Actions.Attack"), InteractAction_Attack));
                }
                if (!HasComp<EscortComponent>(uid))
                {
                    if (_parties.IsInPlayerParty(uid))
                    {
                        args.OutInteractActions.Add(new(Loc.GetString("Elona.Interact.Actions.Inventory"), InteractAction_Inventory));
                    }
                    else
                    {
                        args.OutInteractActions.Add(new(Loc.GetString("Elona.Interact.Actions.Give"), InteractAction_Give));
                    }
                    // TODO bring out
                    if (_parties.IsInPlayerParty(uid))
                    {
                        args.OutInteractActions.Add(new(Loc.GetString("Elona.Interact.Actions.Appearance"), InteractAction_Appearance));
                    }
                }
                args.OutInteractActions.Add(new(Loc.GetString("Elona.Interact.Actions.TeachWords"), InteractAction_TeachWords));
                args.OutInteractActions.Add(new(Loc.GetString("Elona.Interact.Actions.ChangeTone"), InteractAction_ChangeTone));

                // TODO show house

                if (HasComp<SandBaggedComponent>(uid))
                {
                    args.OutInteractActions.Add(new(Loc.GetString("Elona.Interact.Actions.Release"), InteractAction_Release));
                }
            }

            args.OutInteractActions.Add(new(Loc.GetString("Elona.Interact.Actions.Name"), InteractAction_Name));

            if (_config.GetCVar(CVars.DebugDevelopmentMode))
            {
                args.OutInteractActions.Add(new(Loc.GetString("Elona.Interact.Actions.Info"), InteractAction_Info));
            }
        }

        public TurnResult PromptInteract(EntityUid player)
        {
            var dir = _uiManager.Query<DirectionPrompt, DirectionPrompt.Args, DirectionPrompt.Result>(new(player, Loc.GetString("Elona.Interact.Query.Direction")));

            if (!dir.HasValue)
            {
                _mes.Display(Loc.GetString("Elona.Targeting.NoTargetInDirection"));
                return TurnResult.Aborted;
            }

            var target = _lookup.GetBlockingEntity(dir.Value.Coords);

            if (target == null)
            {
                _mes.Display(Loc.GetString("Elona.Targeting.NoTargetInDirection"));
                return TurnResult.Aborted;
            }

            return InteractWith(player, target.Owner);
        }

        public TurnResult InteractWith(EntityUid source, EntityUid target)
        {
            var ev = new GetInteractActionsEvent(source);
            RaiseEvent(target, ev);
            var actions = ev.OutInteractActions;

            if (actions.Count == 0)
            {
                _mes.Display(Loc.GetString("Elona.Interact.NoInteractActions", ("target", target)));
                return TurnResult.Aborted;
            }

            var promptArgs = new Prompt<InteractAction>.Args(actions)
            {
                QueryText = Loc.GetString("Elona.Interact.Query.Action", ("target", target)),
                IsCancellable = true
            };
            var result = _uiManager.Query<Prompt<InteractAction>, Prompt<InteractAction>.Args, PromptChoice<InteractAction>>(promptArgs);

            if (!result.HasValue)
                return TurnResult.Aborted;

            return result.Value.ChoiceData.Interact(source, target);
        }
    }

    public delegate TurnResult InteractActionDelegate(EntityUid source, EntityUid target);

    public sealed class InteractAction : IPromptFormattable
    {
        public InteractAction(string name, InteractActionDelegate interact)
        {
            Name = name;
            Interact = interact;
        }

        public string Name { get; }
        public InteractActionDelegate Interact { get; }

        public string FormatForPrompt()
        {
            return Name;
        }
    }

    public sealed class GetInteractActionsEvent : EntityEventArgs
    {
        public EntityUid Source { get; }

        public IList<InteractAction> OutInteractActions { get; } = new List<InteractAction>();

        public GetInteractActionsEvent(EntityUid source)
        {
            Source = source;
        }
    }
}