using OpenNefia.Core.Logic;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.GameObjects
{
    /// <summary>
    /// System for querying and executing <see cref="Verb"/>s, which are 
    /// player-initated actions targeting an entity.
    /// </summary>
    public class VerbSystem : EntitySystem
    {
        /// <summary>
        /// Gets the set of verbs applicable to this entity from sending
        /// a "get verbs" event to it.
        /// </summary>
        public SortedSet<Verb> GetLocalVerbs(EntityUid target, EntityUid source)
        {
            var verbs = new SortedSet<Verb>();

            var getVerbsEvent = new GetVerbsEventArgs(source, target);
            RaiseLocalEvent(target, getVerbsEvent);
            verbs.AddRange(getVerbsEvent.Verbs);

            return verbs;
        }

        /// <summary>
        /// Makes an entity execute a verb on a target.
        /// </summary>
        public TurnResult ExecuteVerb(EntityUid source, EntityUid target, Verb verb)
        {
            var ev = new ExecuteVerbEventArgs(source, target, verb);
            RaiseLocalEvent(source, ev);
            return ev.TurnResult;
        }
    }

    /// <summary>
    /// Event for getting the list of verbs that can be applied
    /// to an entity.
    /// </summary>
    public class GetVerbsEventArgs : EntityEventArgs 
    {
        /// <summary>
        /// Valid verbs for this entity.
        /// 
        /// This is a sorted set to be able to use Verb.IComparable,
        /// so that duplicate verbs cannot be added.
        /// </summary>
        public readonly SortedSet<Verb> Verbs = new();

        public readonly EntityUid Source;
        public readonly EntityUid Target;

        public GetVerbsEventArgs(EntityUid source, EntityUid target)
        {
            Source = source;
            Target = target;
        }
    }

    /// <summary>
    /// Event to execute an verb *interactively*.
    /// 
    /// This means opening the UI and asking the player for input.
    /// </summary>
    public class ExecuteVerbEventArgs : TurnResultEntityEventArgs
    {
        public readonly EntityUid Source;
        public readonly EntityUid Target;

        public readonly Verb Verb;

        public ExecuteVerbEventArgs(EntityUid source, EntityUid target, Verb verb)
        {
            this.Source = source;
            this.Target = target;
            this.Verb = verb;
        }
    }
}
