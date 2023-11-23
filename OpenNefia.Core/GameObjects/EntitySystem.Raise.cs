using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using OpenNefia.Core.Areas;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.GameObjects
{
    public abstract partial class EntitySystem
    {
        /// <summary>
        /// Raises an event.
        /// </summary>
        /// <returns>True if something handled the event, or the target entity is dead after the event was fired.</returns>
        protected bool Raise<T>(EntityUid uid, T args, bool broadcast = true, bool ignoreLiveness = false)
            where T : HandledEntityEventArgs
        {
            RaiseEvent(uid, args, broadcast);
            return args.Handled || (!EntityManager.IsAlive(uid) && !ignoreLiveness);
        }

        protected bool Raise<T1, T2>(EntityUid uid, T1 args, T2 propagateTo, bool ignoreLiveness = false)
            where T1 : TurnResultEntityEventArgs
            where T2 : TurnResultEntityEventArgs
        {
            RaiseEvent(uid, args);

            if (args.Handled || (!EntityManager.IsAlive(uid) && !ignoreLiveness))
            {
                propagateTo.Handled = true;
                propagateTo.TurnResult = args.TurnResult;
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsAlive([NotNullWhen(true)] EntityUid? uid)
        {
            return EntityManager.IsAlive(uid);
        }
        
        /// <inheritdoc cref="IEntityManager.GetComponent&lt;T&gt;"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected T Comp<T>(EntityUid uid) where T : class, IComponent
        {
            return EntityManager.GetComponent<T>(uid);
        }

        /// <summary>
        ///     Returns the component of a specific type, or null when it's missing or the entity does not exist.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected T? CompOrNull<T>(EntityUid uid) where T : class, IComponent
        {
            return EntityManager.GetComponentOrNull<T>(uid);
        }

        /// <summary>
        ///     Returns the component of a specific type, or null when it's missing or the entity does not exist.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected T? CompOrNull<T>(EntityUid? uid) where T : class, IComponent
        {
            return uid.HasValue ? EntityManager.GetComponentOrNull<T>(uid.Value) : null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool TryComp<T>([NotNullWhen(true)] EntityUid? uid, [NotNullWhen(true)] out T? component)
            where T : class, IComponent
        {
            if (uid == null)
            {
                component = null;
                return false;
            }
            
            return EntityManager.TryGetComponent(uid.Value, out component);
        }

        /// <summary>
        ///     Returns the <see cref="SpatialComponent"/> on an entity.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown when the entity doesn't exist.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected SpatialComponent Spatial(EntityUid uid)
        {
            return EntityManager.GetComponent<SpatialComponent>(uid);
        }

        /// <summary>
        ///     Returns the <see cref="MetaDataComponent"/> on an entity.
        /// </summary>
        /// <exception cref="KeyNotFoundException">Thrown when the entity doesn't exist.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected MetaDataComponent MetaData(EntityUid uid)
        {
            return EntityManager.GetComponent<MetaDataComponent>(uid);
        }

        protected bool TryProto(EntityUid uid, [NotNullWhen(true)] out EntityPrototype? proto)
        {
            if (!TryComp<MetaDataComponent>(uid, out var metaData))
            {
                proto = null;
                return false;
            }

            proto = metaData.EntityPrototype;
            return proto != null;
        }
        
        protected bool TryProtoID(EntityUid uid, [NotNullWhen(true)] out PrototypeId<EntityPrototype>? protoID)
        {
            if (!TryComp<MetaDataComponent>(uid, out var metaData))
            {
                protoID = null;
                return false;
            }

            protoID = metaData.EntityPrototype?.GetStrongID();
            return protoID != null;
        }

        protected PrototypeId<EntityPrototype>? ProtoIDOrNull(EntityUid uid)
        {
            TryProtoID(uid, out var protoID);
            return protoID;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool HasComp<T>(EntityUid ent)
        {
            return EntityManager.HasComponent<T>(ent);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected T EnsureComp<T>(EntityUid ent)
            where T : Component, new()
        {
            return EntityManager.EnsureComponent<T>(ent);
        }

        protected IMap GetMap(MapCoordinates coords, IMapManager? mapMan = null)
        {
            IoCManager.Resolve(ref mapMan);
            return mapMan.GetMap(coords.MapId);
        }

        protected IMap GetMap(EntityUid uid, IMapManager? mapMan = null)
        {
            IoCManager.Resolve(ref mapMan);
            return mapMan.GetMapOfEntity(uid);
        }

        protected IMap? MapOrNull(EntityUid uid, IMapManager? mapMan = null)
        {
            IoCManager.Resolve(ref mapMan);
            TryMap(uid, out var map, mapMan);
            return map;
        }

        protected bool TryMap(MapCoordinates coords, [NotNullWhen(true)] out IMap? map, IMapManager? mapMan = null)
        {
            IoCManager.Resolve(ref mapMan);
            return mapMan.TryGetMap(coords.MapId, out map);
        }

        protected bool TryMap(EntityUid uid, [NotNullWhen(true)] out IMap? map, IMapManager? mapMan = null)
        {
            IoCManager.Resolve(ref mapMan);
            return mapMan.TryGetMapOfEntity(uid, out map);
        }
        
        protected IArea? AreaOrNull(EntityUid uid, IMapManager? mapMan = null, IAreaManager? areaMan = null)
        {
            IoCManager.Resolve(ref mapMan);
            IoCManager.Resolve(ref areaMan);
            TryArea(uid, out var area, mapMan, areaMan);
            return area;
        }

        protected IArea GetArea(EntityUid uid, IMapManager? mapMan = null, IAreaManager? areaMan = null)
        {
            if (!TryArea(uid, out var area, mapMan, areaMan))
                throw new InvalidOperationException($"No area for entity {uid}");
            return area;
        }

        protected bool TryArea(EntityUid uid, [NotNullWhen(true)] out IArea? area, IMapManager? mapMan = null, IAreaManager? areaMan = null)
        {
            IoCManager.Resolve(ref areaMan);

            if (TryComp<AreaComponent>(uid, out var areaComp))
            {
                area = areaMan.GetArea(areaComp.AreaId);
                return true;
            }

            if (!TryMap(uid, out var map, mapMan))
            {
                area = null;
                return false;
            }

            return areaMan.TryGetAreaOfMap(map, out area);
        }

        protected bool TryArea(IMap map, [NotNullWhen(true)] out IArea? area, IAreaManager? areaMan = null)
        {
            IoCManager.Resolve(ref areaMan);
            return areaMan.TryGetAreaOfMap(map, out area);
        }

        protected bool TryArea(AreaId areaId, [NotNullWhen(true)] out IArea? area, IAreaManager? areaMan = null)
        {
            IoCManager.Resolve(ref areaMan);
            return areaMan.TryGetArea(areaId, out area);
        }
    }
}
