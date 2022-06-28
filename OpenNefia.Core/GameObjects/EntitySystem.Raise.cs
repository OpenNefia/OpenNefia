using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using OpenNefia.Core.Log;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.GameObjects
{
    public abstract partial class EntitySystem
    {
        /// <summary>
        /// Raises an event.
        /// </summary>
        /// <returns>True if something handled the event, or the target entity is dead after the event was fired.</returns>
        public bool Raise<T>(EntityUid uid, T args)
            where T : HandledEntityEventArgs
        {
            RaiseEvent(uid, args);
            return args.Handled || !EntityManager.IsAlive(uid);
        }

        public bool Raise<T1, T2>(EntityUid uid, T1 args, T2 propagateTo)
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

        public T GetComp<T>(EntityUid uid) 
            where T: class, IComponent
        {
            return EntityManager.GetComponent<T>(uid);
        }

        public bool TryComp<T>(EntityUid uid, [NotNullWhen(true)] out T? component)
            where T : class, IComponent
        {
            return EntityManager.TryGetComponent(uid, out component);
        }

        public bool HasComp<T>(EntityUid ent)
        {
            return EntityManager.HasComponent<T>(ent);
        }

        public T EnsureComp<T>(EntityUid ent)
            where T : Component, new()
        {
            return EntityManager.EnsureComponent<T>(ent);
        }
    }
}
