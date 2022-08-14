using OpenNefia.Content.GameController;
using OpenNefia.Content.Logic;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.UI.Hud;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameController;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.UI;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Content.Activity
{
    public interface IActivitySystem : IEntitySystem
    {
        void InterruptUsing(EntityUid itemBeingUsed);
        void RemoveActivity(EntityUid entity, ActivityHolderComponent? activityHolder = null);
        void InterruptActivity(EntityUid entity, ActivityHolderComponent? activityHolder = null);
        bool TryGetActivity(EntityUid entity, [NotNullWhen(true)] out ActivityComponent? activityEnt, ActivityHolderComponent? activityHolder = null);
        bool HasAnyActivity(EntityUid entity, ActivityHolderComponent? activityHolder = null);
        bool HasActivity(EntityUid entity, PrototypeId<EntityPrototype> id, ActivityHolderComponent? activityHolder = null);
        bool StartActivity(EntityUid entity, EntityUid activity, int? turns = null, ActivityHolderComponent? activityHolder = null);
        bool StartActivity(EntityUid entity, EntityUid activity, [NotNullWhen(true)] out ActivityComponent? result, int? turns = null, ActivityHolderComponent? activityHolder = null);
        bool StartActivity(EntityUid entity, PrototypeId<EntityPrototype> activity, int? turns = null, ActivityHolderComponent? activityHolder = null);
        bool StartActivity(EntityUid entity, PrototypeId<EntityPrototype> activity, [NotNullWhen(true)] out ActivityComponent? result, int? turns = null, ActivityHolderComponent? activityHolder = null);
    }

    public sealed class ActivitySystem : EntitySystem, IActivitySystem
    {
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly IAudioManager _audio = default!;
        [Dependency] private readonly IHudLayer _hud = default!;
        [Dependency] private readonly IDynamicTypeFactory _dynTypeFactory = default!;
        [Dependency] private readonly IFieldLayer _field = default!;
        [Dependency] private readonly IGameController _gameController = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IPlayerQuery _playerQuery = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;

        public override void Initialize()
        {
            SubscribeComponent<ActivityComponent, OnActivityStartEvent>(UpdateAutoTurnWidget, priority: EventPriorities.Lowest);
            SubscribeComponent<ActivityComponent, OnActivityInterruptedEvent>(HandleActivityInterruptAction, priority: EventPriorities.Low);
            SubscribeComponent<ActivityHolderComponent, EntityPassTurnEventArgs>(UpdateActivity, priority: EventPriorities.High);
        }

        private void UpdateActivity(EntityUid uid, ActivityHolderComponent component, EntityPassTurnEventArgs args)
        {
            if (args.Handled)
                return;

            if (_gameSession.IsPlayer(uid) && TryGetActivity(uid, out var activity, component))
            {
                ProcActivityInterrupted(uid, activity);
            }

            if (TryGetActivity(uid, out activity, component))
            {
                PassActivityTurn(uid, activity);
                if (_config.GetCVar(CCVars.AnimeAutoTurnSpeed) != AutoTurnSpeed.Highest
                    && activity.AnimationWait > 0)
                {
                    _field.RefreshScreen();
                }

                args.Handle(TurnResult.Succeeded);
                return;
            }
        }

        private void HandleActivityInterruptAction(EntityUid uid, ActivityComponent component, OnActivityInterruptedEvent args)
        {
            if (!_gameSession.IsPlayer(component.Actor) || component.OnInterrupt == null)
                return;

            switch (component.OnInterrupt.Value)
            {
                case ActivityInterruptAction.Stop:
                default:
                    args.OutCancelActivity = true;
                    break;
                case ActivityInterruptAction.Prompt:
                    _field.RefreshScreen();
                    args.OutCancelActivity = _playerQuery.YesOrNo(Loc.GetString("Elona.Activity.Cancel.Prompt", ("activity", uid), ("actor", component.Actor)));
                    break;
                case ActivityInterruptAction.Ignore:
                    args.OutCancelActivity = false;
                    break;
            }
        }

        private void PassActivityTurn(EntityUid actor, ActivityComponent? activityComp = null)
        {
            if (activityComp == null)
            {
                if (!TryGetActivity(actor, out activityComp))
                    return;
            }

            if (_gameSession.IsPlayer(actor))
            {
                if (activityComp.AnimationWait > 0)
                {
                    if (_config.GetCVar(CCVars.AnimeAutoTurnSpeed) == AutoTurnSpeed.Normal
                        && _hud.TryGetWidget<HudAutoTurnWidget>(out var widget))
                    {
                        _gameController.Wait((float)activityComp.AnimationWait / 1000);
                        widget.PassTurn();
                    }

                    if (!activityComp.CanScroll)
                    {
                        // TODO scrolling
                    }
                }
            }

            if (activityComp.TurnsRemaining <= 0)
            {
                var evFinish = new OnActivityFinishEvent(activityComp);
                RaiseEvent(activityComp.Owner, evFinish);
                RemoveActivity(actor);
                return;
            }

            var ev = new OnActivityPassTurnEvent(activityComp);
            RaiseEvent(activityComp.Owner, ev);

            activityComp.TurnsRemaining--;
        }

        private void ProcActivityInterrupted(EntityUid uid, ActivityComponent activity)
        {
            if (!activity.WasInterrupted)
                return;

            activity.WasInterrupted = false;

            var ev = new OnActivityInterruptedEvent();
            RaiseEvent(activity.Owner, ev);

            if (ev.OutCancelActivity)
            {
                _mes.Display(Loc.GetString("Elona.Activity.Cancel.Normal", ("actor", uid), ("activity", activity.Owner)));
                RemoveActivity(uid);
            }
        }

        public void InterruptUsing(EntityUid item)
        {
            // TODO
        }

        public void RemoveActivity(EntityUid entity, ActivityHolderComponent? activityHolder = null)
        {
            if (!Resolve(entity, ref activityHolder))
                return;

            if (!TryGetActivity(entity, out var activityComp, activityHolder))
                return;

            var ev = new OnActivityCleanupEvent(activityComp);
            RaiseEvent(activityComp.Owner, ev);

            activityHolder.Container.ForceRemove(activityComp.Owner);
            EntityManager.DeleteEntity(activityComp.Owner);

            if (_gameSession.IsPlayer(entity) && _hud.TryGetWidget<HudAutoTurnWidget>(out var widget, out var instance))
            {
                instance.DrawFlags = WidgetDrawFlags.Never;
            }
        }

        public bool TryGetActivity(EntityUid entity, [NotNullWhen(true)] out ActivityComponent? activityComp, ActivityHolderComponent? activityHolder = null)
        {
            if (!Resolve(entity, ref activityHolder))
            {
                activityComp = null;
                return false;
            }

            var activityEnt = activityHolder.Container.ContainedEntity;

            if (activityEnt != null && !IsAlive(activityEnt))
            {
                activityHolder.Container.ForceRemove(activityEnt.Value);
                activityEnt = null;
            }

            return TryComp(activityEnt, out activityComp);
        }

        public void InterruptActivity(EntityUid entity, ActivityHolderComponent? activityHolder = null)
        {
            if (!TryGetActivity(entity, out var activity))
                return;

            activity.WasInterrupted = true;
        }

        public bool HasAnyActivity(EntityUid entity, ActivityHolderComponent? activityHolder = null)
        {
            return TryGetActivity(entity, out _, activityHolder);
        }

        public bool HasActivity(EntityUid entity, PrototypeId<EntityPrototype> id, ActivityHolderComponent? activityHolder = null)
        {
            if (!TryGetActivity(entity, out var activityComp, activityHolder))
                return false;

            return MetaData(activityComp.Owner)?.EntityPrototype?.GetStrongID() == id;
        }

        public bool StartActivity(EntityUid entity, EntityUid activity, [NotNullWhen(true)] out ActivityComponent? result, int? turns = null, ActivityHolderComponent? activityHolder = null)
        {
            if (!Resolve(entity, ref activityHolder))
            {
                result = null;
                return false;
            }

            if (HasAnyActivity(entity, activityHolder))
                RemoveActivity(entity, activityHolder);

            var activityComp = EnsureComp<ActivityComponent>(activity);

            activityComp.Actor = entity;
            activityHolder.Container.Insert(activity);

            if (turns != null)
            {
                activityComp.TurnsRemaining = turns.Value;
            }
            else
            {
                var evTurns = new CalcActivityTurnsEvent(activityComp.DefaultTurns);
                RaiseEvent(activity, evTurns);
                activityComp.TurnsRemaining = evTurns.OutTurns;
            }

            var evStart = new OnActivityStartEvent(activityComp);
            RaiseEvent(activity, evStart);
            if (evStart.Cancelled)
            {
                RemoveActivity(entity, activityHolder);
                result = null;
                return false;
            }

            result = activityComp;
            return true;
        }

        public bool StartActivity(EntityUid entity, EntityUid activity, int? turns = null, ActivityHolderComponent? activityHolder = null)
            => StartActivity(entity, activity, out _, turns, activityHolder);

        public bool StartActivity(EntityUid entity, PrototypeId<EntityPrototype> activityId, [NotNullWhen(true)] out ActivityComponent? result, int? turns = null, ActivityHolderComponent? activityHolder = null)
        {
            if (!Resolve(entity, ref activityHolder))
            {
                result = null;
                return false;
            }

            var activityProto = _protos.Index(activityId);
            if (!activityProto.Components.HasComponent<ActivityComponent>())
            {
                Logger.ErrorS("activity", $"Entity prototype {activityId} does not have a {nameof(ActivityComponent)}!");
                result = null;
                return false;
            }

            var activity = EntityManager.SpawnEntity(activityId, new MapCoordinates(MapId.Global, Vector2i.Zero));
            if (!activity.IsValid())
            {
                Logger.ErrorS("activity", $"Failed to create entity {activityId}!");
                result = null;
                return false;
            }

            return StartActivity(entity, activity, out result, turns, activityHolder);
        }

        public bool StartActivity(EntityUid entity, PrototypeId<EntityPrototype> activityId, int? turns = null, ActivityHolderComponent? activityHolder = null)
            => StartActivity(entity, activityId, out _, turns, activityHolder);

        private void UpdateAutoTurnWidget(EntityUid activity, ActivityComponent component, OnActivityStartEvent args)
        {
            if (args.Cancelled)
                return;

            var autoTurnAnimType = component.AutoTurnAnimationType;
            BaseAutoTurnAnim? anim = null;

            if (autoTurnAnimType != null)
            {
                if (!typeof(BaseAutoTurnAnim).IsAssignableFrom(autoTurnAnimType))
                {
                    Logger.ErrorS("activity", $"{autoTurnAnimType} does not inherit from ${nameof(BaseAutoTurnAnim)})");
                    return;
                }

                anim = (BaseAutoTurnAnim)_dynTypeFactory.CreateInstance(autoTurnAnimType);
                anim.Initialize();
            }

            var actor = component.Actor;

            if (_gameSession.IsPlayer(actor) && _config.GetCVar(CCVars.AnimeAutoTurnSpeed) != AutoTurnSpeed.Highest)
            {
                var animWait = component.AnimationWait;
                if (animWait > 0)
                {
                    if (_hud.TryGetWidget<HudAutoTurnWidget>(out var autoTurnWidget, out var instance))
                    {
                        autoTurnWidget.AutoTurnAnimation = anim;
                        instance.DrawFlags = WidgetDrawFlags.Always;
                    }
                }
            }
            else if (anim != null)
            {
                var soundId = anim.Sound.GetSound();
                if (soundId != null)
                    _audio.Play(soundId.Value, actor);
            }
        }
    }

    public sealed class OnActivityInterruptedEvent
    {
        public bool OutCancelActivity { get; set; } = false;
    }

    public sealed class CalcActivityTurnsEvent : EntityEventArgs
    {
        public int OutTurns { get; set; }

        public CalcActivityTurnsEvent(int turns)
        {
            OutTurns = turns;
        }
    }

    public sealed class OnActivityStartEvent : CancellableEntityEventArgs
    {
        public ActivityComponent Activity { get; }

        public OnActivityStartEvent(ActivityComponent activity)
        {
            Activity = activity;
        }
    }

    public sealed class OnActivityPassTurnEvent : HandledEntityEventArgs
    {
        public ActivityComponent Activity { get; }

        public OnActivityPassTurnEvent(ActivityComponent activity)
        {
            Activity = activity;
        }
    }

    public sealed class OnActivityFinishEvent : EntityEventArgs
    {
        public ActivityComponent Activity { get; }

        public OnActivityFinishEvent(ActivityComponent activity)
        {
            Activity = activity;
        }
    }

    public sealed class OnActivityCleanupEvent : EntityEventArgs
    {
        public ActivityComponent Activity { get; }

        public OnActivityCleanupEvent(ActivityComponent activity)
        {
            Activity = activity;
        }
    }
}