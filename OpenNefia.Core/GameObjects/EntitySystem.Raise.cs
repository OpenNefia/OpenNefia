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
            RaiseLocalEvent(uid, args);
            return args.Handled || !EntityManager.IsAlive(uid);
        }

        public bool Raise<T1, T2>(EntityUid uid, T1 args, T2 propagateTo)
            where T1 : TurnResultEntityEventArgs
            where T2 : TurnResultEntityEventArgs
        {
            RaiseLocalEvent(uid, args);
            
            if (args.Handled || !EntityManager.IsAlive(uid))
            {
                propagateTo.Handled = true;
                propagateTo.TurnResult = args.TurnResult;
                return false;
            }

            return true;
        }
    }
}
