using OpenNefia.Content.Combat;
using OpenNefia.Content.Skills;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Content.Logic;
using OpenNefia.Core.Locale;
using OpenNefia.Content.GameObjects;
using OpenNefia.Core.Log;
using OpenNefia.Content.Targetable;
using OpenNefia.Core.Rendering;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.Activity;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Parties;
using OpenNefia.Core.Maps;
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.UI;
using OpenNefia.Content.PCCs;
using OpenNefia.Core.Utility;
using OpenNefia.Core.Random;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Quests;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Content.Mount
{
    public interface IMountSystem : IEntitySystem
    {
        bool IsMounting(EntityUid uid, MountRiderComponent? mountRider = null);
        bool IsBeingMounted(EntityUid uid, MountComponent? mount = null);
        bool TryMount(EntityUid rider, EntityUid mount, MountRiderComponent? mountRiderComp = null, MountComponent? mountComp = null);
        bool TryGetMount(EntityUid rider, [NotNullWhen(true)] out MountComponent? mount, MountRiderComponent? mountRiderComp = null);
        bool TryGetRider(EntityUid mount, [NotNullWhen(true)] out MountRiderComponent? mountRiderComp, MountComponent? mountComp = null);
        bool TryDismount(EntityUid rider, MountRiderComponent? mountRider = null);
    }

    public sealed partial class MountSystem : EntitySystem, IMountSystem
    {
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IRefreshSystem _refresh = default!;
        [Dependency] private readonly IActivitySystem _activities = default!;
        [Dependency] private readonly ITurnOrderSystem _turnOrder = default!;
        [Dependency] private readonly IMapPlacement _mapPlacement = default!;
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IDisplayNameSystem _displayNames = default!;
        [Dependency] private readonly IPCCSystem _pccs = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;

        public override void Initialize()
        {
            SubscribeComponent<MountRiderComponent, CalcPhysicalAttackAccuracyEvent>(HandleCalcAccuracyRider);
            SubscribeComponent<MountComponent, CalcPhysicalAttackAccuracyEvent>(HandleCalcAccuracyMount);
            SubscribeComponent<MountComponent, EntityRefreshEvent>(HandleRefresh);
            SubscribeComponent<MountComponent, GetMapObjectMemoryEventArgs>(HandleGetMemory);
            SubscribeComponent<MountRiderComponent, EntityTurnStartingEventArgs>(TurnStarting_MountRider);
            SubscribeComponent<MountComponent, EntityTurnStartingEventArgs>(TurnStarting_Mount);

            SubscribeComponent<MountRiderComponent, EntityKilledEvent>(Killed_MountRider);
            SubscribeComponent<MountRiderComponent, BeforeEntityDeletedEvent>(BeforeDeleted_MountRider);
            SubscribeComponent<MountComponent, EntityKilledEvent>(Killed_Mount);
            SubscribeComponent<MountComponent, BeforeEntityDeletedEvent>(BeforeDeleted_Mount);

            SubscribeComponent<MountComponent, BeforeMoveEventArgs>(BeforeMove_Mount);
            SubscribeComponent<MountRiderComponent, AfterMoveEventArgs>(AfterMove_MountRider);
            SubscribeComponent<MountComponent, AfterPartyMemberEntersMapEventArgs>(AfterMapEntered_Mount);

            SubscribeComponent<PCCComponent, AfterEntityStartedRidingEvent>(AfterStartRiding_UpdatePCC);
            SubscribeComponent<PCCComponent, AfterEntityStoppedRidingEvent>(AfterStopRiding_UpdatePCC);

            SubscribeEntity<GetInteractActionsEvent>(AddRidingAction); // TODO remove when actions are implemented
        }

        // TODO remove when actions are implemented
        private void AddRidingAction(EntityUid uid, GetInteractActionsEvent args)
        {
            args.OutInteractActions.Add(new("Ride (temp)", InteractAction_Ride));
        }

        // TODO remove when actions are implemented
        private TurnResult InteractAction_Ride(EntityUid source, EntityUid target)
        {
            if (TryGetMount(source, out var mount) && mount.Owner == target && mount.Rider == source)
            {
                return TryDismount(source) ? TurnResult.Succeeded : TurnResult.Aborted;
            }
            else
            {
                return TryMount(source, target) ? TurnResult.Succeeded : TurnResult.Aborted;
            }
        }

        private void Killed_MountRider(EntityUid uid, MountRiderComponent riderComp, ref EntityKilledEvent args)
        {
            // >>>>>>>> elona122/shade2/chara_func.hsp:1733 		if gRider : if tc=gRider : txtMore:txt lang(name ...
            DoDismount(uid);
            riderComp.Mount = null;
            // <<<<<<<< elona122/shade2/chara_func.hsp:1733 		if gRider : if tc=gRider : txtMore:txt lang(name ...
        }

        private void BeforeDeleted_MountRider(EntityUid uid, MountRiderComponent riderComp, ref BeforeEntityDeletedEvent args)
        {
            // >>>>>>>> elona122/shade2/chara_func.hsp:1733 		if gRider : if tc=gRider : txtMore:txt lang(name ...
            DoDismount(uid);
            riderComp.Mount = null;
            // <<<<<<<< elona122/shade2/chara_func.hsp:1733 		if gRider : if tc=gRider : txtMore:txt lang(name ...
        }

        private void Killed_Mount(EntityUid uid, MountComponent mountComp, ref EntityKilledEvent args)
        {
            if (IsAlive(mountComp.Rider))
            {
                DoDismount(mountComp.Rider.Value);
            }
            mountComp.Rider = null;
        }

        private void BeforeDeleted_Mount(EntityUid uid, MountComponent mountComp, ref BeforeEntityDeletedEvent args)
        {
            if (IsAlive(mountComp.Rider))
            {
                DoDismount(mountComp.Rider.Value);
            }
            mountComp.Rider = null;
        }

        /// <summary>
        /// Prevent the mount from moving around on its own.
        /// </summary>
        private void BeforeMove_Mount(EntityUid uid, MountComponent component, BeforeMoveEventArgs args)
        {
            if (args.Handled || !IsBeingMounted(uid))
                return;

            args.Handle(TurnResult.Failed);
        }

        /// <summary>
        /// Update the position of the mount when the rider moves.
        /// </summary>
        private void AfterMove_MountRider(EntityUid uid, MountRiderComponent component, AfterMoveEventArgs args)
        {
            // >>>>>>>> elona122/shade2/proc.hsp:88 	if cc=pc:if gRider!0:cX(gRider)=cX(pc) : cY(gRide ...
            if (IsAlive(component.Mount) && args.NewPosition.TryToEntity(_mapManager, out var coords))
            {
                if (_activities.HasAnyActivity(component.Mount.Value))
                {
                    // >>>>>>>> elona122/shade2/action.hsp:530 	if gRider!0:if cRowAct(gRider)!0:if cActionPeriod ...
                    _mes.Display(Loc.GetString("Elona.Mount.Movement.InterruptActivity", ("rider", uid), ("mount", component.Mount.Value)));
                    _activities.RemoveActivity(component.Mount.Value);
                    // <<<<<<<< elona122/shade2/action.hsp:530 	if gRider!0:if cRowAct(gRider)!0:if cActionPeriod ...
                }

                // TODO should this raise AfterMoveEventArgs or something?
                Spatial(component.Mount.Value).Coordinates = coords;
            }
            // <<<<<<<< elona122/shade2/proc.hsp:88 	if cc=pc:if gRider!0:cX(gRider)=cX(pc) : cY(gRide ...
        }

        private void AfterMapEntered_Mount(EntityUid uid, MountComponent component, AfterPartyMemberEntersMapEventArgs args)
        {
            // >>>>>>>> elona122/shade2/chara.hsp:387 	if gRider=rc:if rc!pc:cX(rc)=cX(pc):cY(rc)=cY(pc) ...
            if (IsAlive(component.Rider))
            {
                var spatialMount = Spatial(uid);
                var spatialRider = Spatial(component.Rider.Value);
                if (spatialMount.MapID == spatialRider.MapID)
                {
                    spatialMount.Coordinates = spatialRider.Coordinates;
                }
            }
            // <<<<<<<< elona122/shade2/chara.hsp:387 	if gRider=rc:if rc!pc:cX(rc)=cX(pc):cY(rc)=cY(pc) ...
        }

        private void TurnStarting_MountRider(EntityUid uid, MountRiderComponent component, EntityTurnStartingEventArgs args)
        {
            if (component.Mount != null && !IsMounting(uid, component))
            {
                Logger.WarningS("riding", $"Removing {uid}'s mount {uid} at turn start as it is no longer valid.");
                DoDismount(uid);
            }
        }

        private void TurnStarting_Mount(EntityUid uid, MountComponent component, EntityTurnStartingEventArgs args)
        {
            if (component.Rider != null && !IsBeingMounted(uid, component))
            {
                Logger.WarningS("riding", $"Removing rider {component.Rider} of mount {uid} at turn start as it is no longer valid.");
                DoDismountMount(component);
            }
        }

        private void DoDismountMount(MountComponent mountComp)
        {
            if (mountComp.Rider != null && !IsAlive(mountComp.Rider))
            {
                Logger.WarningS("riding", $"Removing rider {mountComp.Rider.Value} of mount {mountComp.Owner}");
            }

            var rider = mountComp.Rider;
            mountComp.Rider = null;
            _activities.InterruptActivity(mountComp.Owner);
            _refresh.Refresh(mountComp.Owner);
            _turnOrder.RefreshSpeed(mountComp.Owner);

            var ev = new AfterEntityWasDismountedFromEvent(rider);
            RaiseEvent(mountComp.Owner, ev);
        }

        private void DoDismountRider(MountRiderComponent riderComp)
        {
            if (riderComp.Mount != null && !IsAlive(riderComp.Mount))
            {
                // Display message if mount was killed/deleted.
                _mes.Display(Loc.GetString("Elona.Mount.Stop.DismountCorpse", ("rider", riderComp.Owner), ("mount", riderComp.Mount)));
                Logger.WarningS("riding", $"Unmounting rider {riderComp.Owner} of mount {riderComp.Mount.Value}");
            }

            var mount = riderComp.Mount;
            riderComp.Mount = null;
            _activities.InterruptActivity(riderComp.Owner);
            _refresh.Refresh(riderComp.Owner);
            _turnOrder.RefreshSpeed(riderComp.Owner);

            var ev = new AfterEntityStoppedRidingEvent(mount);
            RaiseEvent(riderComp.Owner, ev);
        }

        private void DoDismount(EntityUid rider, MountRiderComponent? riderComp = null)
        {
            // >>>>>>>> elona122/shade2/chara_func.hsp:249 #deffunc ride_end ...
            if (!Resolve(rider, ref riderComp))
                return;

            // Do not check for liveness here since the mount might have died earlier.
            if (TryComp<MountComponent>(riderComp.Mount, out var mount))
            {
                DoDismountMount(mount);
            }

            DoDismountRider(riderComp);
            // <<<<<<<< elona122/shade2/chara_func.hsp:255 #global  ...
        }

        public bool TryMount(EntityUid rider, EntityUid mount, MountRiderComponent? mountRiderComp = null, MountComponent? mountComp = null)
        {
            // >>>>>>>> elona122/shade2/chara_func.hsp:238 	txt lang(name(c)+"に騎乗した("+name(c)+"の速度: "+cSpeed( ...
            if (!Resolve(rider, ref mountRiderComp) || !Resolve(mount, ref mountComp))
                return false;

            if (!_parties.IsPartyLeaderOf(rider, mount))
            {
                _mes.Display(Loc.GetString("Elona.Mount.Start.Problems.CanOnlyRideAlly", ("rider", rider), ("mount", mount)));
                return false;
            }

            if (IsAlive(mountRiderComp.Mount))
            {
                _mes.Display(Loc.GetString("Elona.Mount.Start.Problems.IsCurrentlyRiding", ("rider", rider), ("mount", mountRiderComp.Mount.Value)));
                return false;
            }

            if (IsAlive(mountComp.Rider))
            {
                _mes.Display(Loc.GetString("Elona.Mount.Start.Problems.IsCurrentlyRiding", ("rider", mountComp.Rider.Value), ("mount", mount)));
                return false;
            }

            if (rider == mount)
            {
                _mes.Display(Loc.GetString("Elona.Mount.Start.Problems.RideSelf", ("rider", rider)));
                return false;
            }

            var ev1 = new BeforeEntityStartsRidingEvent(mount);
            var ev2 = new BeforeEntityMountedOntoEvent(rider);

            RaiseEvent(rider, ev1);
            if (ev1.Cancelled)
                return false;
            RaiseEvent(mount, ev2);
            if (ev2.Cancelled)
                return false;

            _activities.RemoveActivity(rider);
            _activities.RemoveActivity(mount);

            var turnOrder = CompOrNull<TurnOrderComponent>(mount);
            var mountPrevSpeed = turnOrder?.CurrentSpeed ?? 0;

            mountComp.Rider = rider;
            mountRiderComp.Mount = mount;

            _refresh.Refresh(rider);
            _refresh.Refresh(mount);
            _turnOrder.RefreshSpeed(mount);

            var mountNewSpeed = turnOrder?.CurrentSpeed ?? 0;

            _mes.Display(Loc.GetString("Elona.Mount.Start.YouRide",
                ("rider", rider),
                ("mount", mount),
                ("mountPrevSpeed", mountPrevSpeed),
                ("mountNewSpeed", mountNewSpeed)));

            switch (mountComp.Suitability.Buffed)
            {
                case MountSuitability.Normal:
                default:
                    break;
                case MountSuitability.Good:
                    _mes.Display(Loc.GetString("Elona.Mount.Start.Suitability.Good", ("rider", rider), ("mount", mount)));
                    break;
                case MountSuitability.Bad:
                    _mes.Display(Loc.GetString("Elona.Mount.Start.Suitability.Bad", ("rider", rider), ("mount", mount)));
                    break;
            }

            var ev3 = new AfterEntityStartedRidingEvent(mount);
            var ev4 = new AfterEntityMountedOntoEvent(rider);
            RaiseEvent(rider, ev3);
            RaiseEvent(mount, ev4);

            _mes.Display(_displayNames.GetDisplayName(mount) + Loc.Space + Loc.GetString("Elona.Mount.Start.Dialog", ("mount", mount), ("rider", rider)), color: UiColors.MesSkyBlue);

            return true;
            // <<<<<<<< elona122/shade2/chara_func.hsp:246 	if cBit(cNoHorse,gRider):txt lang("この生物はあなたを乗せるには ...
        }

        public bool TryDismount(EntityUid rider, MountRiderComponent? mountRiderComp = null)
        {
            if (!Resolve(rider, ref mountRiderComp) || !IsAlive(mountRiderComp.Mount))
                return false;

            var mount = mountRiderComp.Mount;

            var placeType = CharaPlaceType.Npc;
            if (_parties.IsInPlayerParty(rider))
                placeType = CharaPlaceType.Ally;

            var coords = _mapPlacement.FindFreePositionForChara(Spatial(rider).MapPosition, placeType);
            if (coords == null || !coords.Value.TryToEntity(_mapManager, out var entityCoords))
            {
                return false;
            }

            DoDismount(rider, mountRiderComp);
            Spatial(mount.Value).Coordinates = entityCoords;

            _mes.Display(Loc.GetString("Elona.Mount.Stop.YouDismount",
                ("rider", rider),
                ("mount", mount)));

            return true;
        }

        private void HandleGetMemory(EntityUid uid, MountComponent component, GetMapObjectMemoryEventArgs args)
        {
            if (IsBeingMounted(uid))
                args.OutMemory.IsVisible = false;
        }

        private void HandleRefresh(EntityUid uid, MountComponent component, ref EntityRefreshEvent args)
        {
            if (IsBeingMounted(uid))
            {
                EnsureComp<TargetableComponent>(uid).IsTargetable.Buffed = false;
            }
            else
            {
                if (component.Rider != null)
                {
                    Logger.ErrorS("riding", $"Entity {uid} was mounted by {component.Rider}, but they were dead.");
                }
            }
        }

        public bool TryGetMount(EntityUid rider, [NotNullWhen(true)] out MountComponent? mount, MountRiderComponent? mountRiderComp = null)
        {
            if (!Resolve(rider, ref mountRiderComp, logMissing: false) || !IsAlive(mountRiderComp.Mount))
            {
                mount = null;
                return false;
            }

            return TryComp(mountRiderComp.Mount, out mount);
        }

        public bool TryGetRider(EntityUid mount, [NotNullWhen(true)] out MountRiderComponent? mountRider, MountComponent? mountComp = null)
        {
            if (!Resolve(mount, ref mountComp, logMissing: false) || !IsAlive(mountComp.Rider))
            {
                mountRider = null;
                return false;
            }

            return TryComp(mountComp.Rider, out mountRider);
        }

        public bool IsMounting(EntityUid uid, MountRiderComponent? mountRider = null)
            => TryGetMount(uid, out _, mountRider);

        public bool IsBeingMounted(EntityUid uid, MountComponent? mount = null)
            => TryGetRider(uid, out _, mount);

        private void AddRidingPCCParts(PCCComponent component)
        {
            // TODO allow customizing these in a persistent way
            var pccParts = PCCHelpers.GetGroupedPCCParts(_protos);
            var pccRide = pccParts[PCCPartType.Ride].First();
            var pccRidebk = pccParts[PCCPartType.Ridebk].First();
            component.PCCParts[PCCPartSlots.Ride] = pccRide;
            component.PCCParts[PCCPartSlots.Ridebk] = pccRidebk;
        }

        private void AfterStartRiding_UpdatePCC(EntityUid uid, PCCComponent component, AfterEntityStartedRidingEvent args)
        {
            foreach (var (key, value) in component.PCCParts.ToList())
            {
                if (value.Type == PCCPartType.Ride || value.Type == PCCPartType.Ridebk)
                    component.PCCParts.Remove(key);
            }

            AddRidingPCCParts(component);

            _pccs.RebakePCCImage(uid, component);
        }

        private void AfterStopRiding_UpdatePCC(EntityUid uid, PCCComponent component, AfterEntityStoppedRidingEvent args)
        {
            foreach (var (key, value) in component.PCCParts.ToList())
            {
                if (value.Type == PCCPartType.Ride || value.Type == PCCPartType.Ridebk)
                    component.PCCParts.Remove(key);
            }

            _pccs.RebakePCCImage(uid, component);
        }
    }

    [EventUsage(EventTarget.Normal)]
    public sealed class BeforeEntityStartsRidingEvent : CancellableEntityEventArgs
    {
        public BeforeEntityStartsRidingEvent(EntityUid mount)
        {
            Mount = mount;
        }

        public EntityUid Mount { get; }
    }

    [EventUsage(EventTarget.Normal)]
    public sealed class BeforeEntityMountedOntoEvent : CancellableEntityEventArgs
    {
        public BeforeEntityMountedOntoEvent(EntityUid rider)
        {
            Rider = rider;
        }

        public EntityUid Rider { get; }
    }

    [EventUsage(EventTarget.Normal)]
    public sealed class AfterEntityStartedRidingEvent : EntityEventArgs
    {
        public AfterEntityStartedRidingEvent(EntityUid mount)
        {
            Mount = mount;
        }

        public EntityUid Mount { get; }
    }

    [EventUsage(EventTarget.Normal)]
    public sealed class AfterEntityMountedOntoEvent : EntityEventArgs
    {
        public AfterEntityMountedOntoEvent(EntityUid rider)
        {
            Rider = rider;
        }

        public EntityUid Rider { get; }
    }

    public sealed class AfterEntityStoppedRidingEvent : EntityEventArgs
    {
        public AfterEntityStoppedRidingEvent(EntityUid? mount)
        {
            Mount = mount;
        }

        public EntityUid? Mount { get; }
    }

    public sealed class AfterEntityWasDismountedFromEvent : EntityEventArgs
    {
        public AfterEntityWasDismountedFromEvent(EntityUid? rider)
        {
            Rider = rider;
        }

        public EntityUid? Rider { get; }
    }
}