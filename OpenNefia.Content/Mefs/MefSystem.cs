using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Content.World;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.Mefs
{
    public interface IMefSystem : IEntitySystem
    {
        EntityUid? SpawnMef(PrototypeId<EntityPrototype> protoId,
            EntityCoordinates coordinates, 
            GameTimeSpan? duration = null, 
            int power = 50,
            EntityUid? spawnedBy = null,
            EntityGenArgSet? args = null);
        EntityUid? SpawnMef(PrototypeId<EntityPrototype> protoId,
            MapCoordinates coordinates, 
            GameTimeSpan? duration = null, 
            int power = 50,
            EntityUid? spawnedBy = null,
            EntityGenArgSet? args = null);
        EntityUid? SpawnMef(PrototypeId<EntityPrototype> protoId, 
            EntityUid entity, 
            GameTimeSpan? duration = null,
            int power = 50,
            EntityUid? spawnedBy = null,
            EntityGenArgSet? args = null);
    }

    public sealed class MefSystem : EntitySystem, IMefSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;

        public override void Initialize()
        {
            SubscribeComponent<MefComponent, EntityPositionChangedEvent>(Mef_DeleteOthers);
            SubscribeEntity<MapBeforeTurnBeginEventArgs>(UpdateMefs, priority: EventPriorities.VeryHigh - 1000);
            SubscribeEntity<MapOnTimePassedEvent>(DecrementMefDurations, priority: EventPriorities.VeryHigh - 1000);
            SubscribeEntity<MapRenewGeometryEvent>(MapRenewGeometry_ClearMefs, priority: EventPriorities.VeryHigh);
        }

        private void MapRenewGeometry_ClearMefs(EntityUid uid, MapRenewGeometryEvent args)
        {
            // >>>>>>>> elona122/shade2/map_func.hsp:281 	repeat maxMapEf ...
            var map = GetMap(uid);
            foreach (var mef in _lookup.EntityQueryInMap<MefComponent>(map).ToList())
            {
                EntityManager.DeleteEntity(mef.Owner);
            }
            // <<<<<<<< elona122/shade2/map_func.hsp:283 	loop ...
        }

        /// <summary>
        /// Delete other mefs on the same space as the new mef.
        /// </summary>
        private void Mef_DeleteOthers(EntityUid uid, MefComponent component, ref EntityPositionChangedEvent args)
        {
            foreach (var mef in _lookup.EntityQueryLiveEntitiesAtCoords<MefComponent>(args.NewPosition).ToList())
            {
                if (mef.Owner != uid)
                    EntityManager.DeleteEntity(mef.Owner);
            }
        }

        private void UpdateMefs(EntityUid uid, MapBeforeTurnBeginEventArgs ev)
        {
            // >>>>>>>> elona122/shade2/main.hsp:520 	sound	=false ...
            var map = GetMap(uid);

            foreach (var mef in _lookup.EntityQueryInMap<MefComponent>(map).ToList())
            {
                var ev2 = new MefBeforeMapTurnBeginEventArgs(mef, map);
                RaiseEvent(mef.Owner, ev2);
                if (!IsAlive(mef.Owner))
                    continue;

                if (mef.UpdateType == MefUpdateType.MapTurnsPassed)
                {
                    mef.TimeRemaining -= GameTimeSpan.FromMinutes(1);
                    if (mef.TimeRemaining <= GameTimeSpan.Zero)
                    {
                        var ev3 = new MefTimerExpiredEvent(mef);
                        RaiseEvent(mef.Owner, ev3);
                        EntityManager.DeleteEntity(mef.Owner);
                    }
                }
            }
            // <<<<<<<< elona122/shade2/main.hsp:547 	if sound!false:snd sound ...
        }

        private void DecrementMefDurations(EntityUid uid, ref MapOnTimePassedEvent args)
        {
            foreach (var mef in _lookup.EntityQueryInMap<MefComponent>(args.Map).ToList())
            {
                if (mef.UpdateType == MefUpdateType.TimePassed)
                {
                    mef.TimeRemaining -= args.TotalTimePassed;
                    if (mef.TimeRemaining <= GameTimeSpan.Zero)
                    {
                        var ev = new MefTimerExpiredEvent(mef);
                        RaiseEvent(mef.Owner, ev);
                        EntityManager.DeleteEntity(mef.Owner);
                    }
                }
            }
        }

        public EntityUid? SpawnMef(PrototypeId<EntityPrototype> protoId, 
            EntityCoordinates coordinates, 
            GameTimeSpan? duration = null, 
            int power = 50,
            EntityUid? spawnedBy = null,
            EntityGenArgSet? args = null)
        {
            var mef = _entityGen.SpawnEntity(protoId, coordinates, 1, args);
            return AfterMefSpawned(mef, protoId, duration,power,  spawnedBy);
        }

        public EntityUid? SpawnMef(PrototypeId<EntityPrototype> protoId,
            MapCoordinates coordinates, 
            GameTimeSpan? duration = null, 
            int power = 50,
            EntityUid? spawnedBy = null,
            EntityGenArgSet? args = null)
        {
            var mef = _entityGen.SpawnEntity(protoId, coordinates, 1, args);
            return AfterMefSpawned(mef, protoId, duration, power, spawnedBy);
        }

        public EntityUid? SpawnMef(PrototypeId<EntityPrototype> protoId,
            EntityUid entity,
            GameTimeSpan? duration, 
            int power = 50,
            EntityUid? spawnedBy = null,
            EntityGenArgSet? args = null)
        {
            var mef = _entityGen.SpawnEntity(protoId, entity, 1, args);
            return AfterMefSpawned(mef, protoId, duration, power, spawnedBy);
        }

        private EntityUid? AfterMefSpawned(EntityUid? mef, PrototypeId<EntityPrototype> protoID, GameTimeSpan? duration, int power, EntityUid? spawnedBy)
        {
            if (!IsAlive(mef))
            {
                return null;
            }

            if (!TryComp<MefComponent>(mef.Value, out var mefComp))
            {
                Logger.ErrorS("mef", $"Spawned mef {protoID} has no {nameof(MefComponent)}! ({mef.Value})");
                return mef.Value;
            }

            if (duration != null)
                mefComp.TimeRemaining = duration.Value;
            else
                mefComp.UpdateType = MefUpdateType.LastForever;

            mefComp.Power = power;
            mefComp.SpawnedBy = spawnedBy;
            return mef;
        }
    }

    [EventUsage(EventTarget.Normal)]
    public sealed class MefBeforeMapTurnBeginEventArgs : HandledEntityEventArgs
    {
        public MefComponent Mef { get; }
        public IMap Map { get; }

        public MefBeforeMapTurnBeginEventArgs(MefComponent mef, IMap map)
        {
            this.Mef = mef;
            this.Map = map;
        }
    }

    [EventUsage(EventTarget.Normal)]
    public sealed class MefTimerExpiredEvent : HandledEntityEventArgs
    {
        public MefComponent Mef { get; }

        public MefTimerExpiredEvent(MefComponent mef)
        {
            Mef = mef;
        }
    }
}