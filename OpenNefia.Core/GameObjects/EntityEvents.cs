using System;

namespace OpenNefia.Core.GameObjects
{
    public interface IEntityEventSubscriber { }

    public delegate void EntityEventHandler<in T>(T ev);
    public delegate void EntityEventRefHandler<T>(ref T ev);

    [Serializable]
    public abstract class EntityEventArgs { }

    [Serializable]
    public abstract class HandledEntityEventArgs : EntityEventArgs
    {
        /// <summary>
        ///     If this message has already been "handled" by a previous system.
        /// </summary>
        public bool Handled { get; set; }
    }

    [Serializable]
    public abstract class TurnResultEntityEventArgs : HandledEntityEventArgs
    {
        /// <summary>
        ///     Turn result of this event.
        /// </summary>
        public TurnResult TurnResult { get; set; }

        public void Handle(TurnResult turnResult)
        {
            Handled = true;
            TurnResult = turnResult;
        }
    }

    [Serializable]
    public abstract class CancellableEntityEventArgs : EntityEventArgs
    {
        /// <summary>
        ///     Whether this even has been cancelled.
        /// </summary>
        public bool Cancelled { get; private set; }

        /// <summary>
        ///     Cancels the event.
        /// </summary>
        public void Cancel() => Cancelled = true;

        /// <summary>
        ///     Uncancels the event. Don't call this unless you know what you're doing.
        /// </summary>
        public void Uncancel() => Cancelled = false;
    }
}
