using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.GameObjects
{
    public abstract partial class EntitySystem
    {
        /// <summary>
        /// Raises an event.
        /// </summary>
        /// <returns>True if something handled the event, or the target entity is dead after the event was fired.</returns>
        protected bool Raise<T>(EntityUid uid, T args)
            where T : HandledEntityEventArgs
        {
            RaiseEvent(uid, args);
            return args.Handled || !EntityManager.IsAlive(uid);
        }

        protected bool Raise<T1, T2>(EntityUid uid, T1 args, T2 propagateTo)
            where T1 : TurnResultEntityEventArgs
            where T2 : TurnResultEntityEventArgs
        {
            RaiseEvent(uid, args);
            
            if (args.Handled || !EntityManager.IsAlive(uid))
            {
                propagateTo.Handled = true;
                propagateTo.TurnResult = args.TurnResult;
                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected T GetComp<T>(EntityUid uid) 
            where T: class, IComponent
        {
            return EntityManager.GetComponent<T>(uid);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool TryComp<T>(EntityUid uid, [NotNullWhen(true)] out T? component)
            where T : class, IComponent
        {
            return EntityManager.TryGetComponent(uid, out component);
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

        protected bool TryMap(EntityUid uid, [NotNullWhen(true)] out IMap? map, IMapManager? mapMan = null, SpatialComponent? spatial = null)
        {
            IoCManager.Resolve(ref mapMan);

            if (!Resolve(uid, ref spatial))
            {
                map = null;
                return false;
            }

            return mapMan.TryGetMap(spatial.MapID, out map);
        }
    }
}
