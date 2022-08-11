using NetVips;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.GameObjects
{
    /// <summary>
    /// System for querying and executing <see cref="Verb"/>s, which are 
    /// player-initated actions targeting an entity.
    /// </summary>
    public interface IVerbSystem : IEntitySystem
    {
        /// <summary>
        /// Gets the set of verbs applicable to this entity from sending
        /// a "get verbs" event to it.
        /// </summary>
        SortedSet<Verb> GetLocalVerbs(EntityUid source, EntityUid target);

        bool CanUseVerbOn(EntityUid source, EntityUid target, VerbRequest verbReq);
        bool CanUseVerbOn(EntityUid source, EntityUid target, string verbType);
        bool CanUseAnyVerbOn(EntityUid source, EntityUid target, ISet<string> verbTypes);
        bool TryGetVerb(EntityUid source, EntityUid target, VerbRequest verbReq, [NotNullWhen(true)] out Verb? verb);
        bool TryGetVerb(EntityUid source, EntityUid target, string verbType, [NotNullWhen(true)] out Verb? verb);
        Verb? GetVerbOrNull(EntityUid source, EntityUid target, VerbRequest verbReq);
        Verb? GetVerbOrNull(EntityUid source, EntityUid target, string verbType);
    }

    public class VerbSystem : EntitySystem, IVerbSystem
    {
        public SortedSet<Verb> GetLocalVerbs(EntityUid source, EntityUid target)
        {
            var verbs = new SortedSet<Verb>();

            var getVerbsEvent = new GetVerbsEventArgs(source, target);
            RaiseEvent(target, getVerbsEvent);
            verbs.AddRange(getVerbsEvent.OutVerbs);

            return verbs;
        }

        public bool CanUseVerbOn(EntityUid source, EntityUid target, VerbRequest verbReq)
            => TryGetVerb(source, target, verbReq, out _);

        public bool CanUseVerbOn(EntityUid source, EntityUid target, string verbType)
            => CanUseVerbOn(source, target, new VerbRequest(verbType));

        public bool CanUseAnyVerbOn(EntityUid source, EntityUid target, ISet<string> verbTypes)
        {
            var verbs = GetLocalVerbs(source, target);
            return verbs.Any(verb => verbTypes.Contains(verb.VerbType));
        }

        public bool TryGetVerb(EntityUid source, EntityUid target, VerbRequest verbReq, [NotNullWhen(true)] out Verb? verb)
        {
            verb = GetVerbOrNull(source, target, verbReq);
            return verb != null;
        }

        public bool TryGetVerb(EntityUid source, EntityUid target, string verbType, [NotNullWhen(true)] out Verb? verb)
            => TryGetVerb(source, target, new VerbRequest(verbType), out verb);

        public Verb? GetVerbOrNull(EntityUid source, EntityUid target, VerbRequest verbReq)
        {
            var verbs = GetLocalVerbs(source, target);
            return verbs.FirstOrDefault(v => v.VerbType == verbReq.VerbType);
        }

        public Verb? GetVerbOrNull(EntityUid source, EntityUid target, string verbType)
            => GetVerbOrNull(source, target, new VerbRequest(verbType));
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
        public readonly SortedSet<Verb> OutVerbs = new();

        public readonly EntityUid Source;
        public readonly EntityUid Target;

        public GetVerbsEventArgs(EntityUid source, EntityUid target)
        {
            Source = source;
            Target = target;
        }
    }
}
