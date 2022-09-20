using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    public class TagComponent : Component, ISerializationHooks
    {
        [DataField("tags")]
        private readonly HashSet<PrototypeId<TagPrototype>> _tags = new();

        public IReadOnlySet<PrototypeId<TagPrototype>> Tags => _tags;

        protected override void Initialize()
        {
            base.Initialize();

            foreach (var tag in _tags)
            {
                GetTagOrThrow(tag);
            }
        }

        private TagPrototype GetTagOrThrow(PrototypeId<TagPrototype> id, IPrototypeManager? manager = null)
        {
            manager ??= IoCManager.Resolve<IPrototypeManager>();
            return manager.Index(id);
        }

        /// <summary>
        ///     Tries to add a tag if it doesn't already exist.
        /// </summary>
        /// <param name="id">The tag to add.</param>
        /// <returns>true if it was added, false if it already existed.</returns>
        /// <exception cref="UnknownPrototypeException">
        ///     Thrown if no <see cref="TagPrototype"/> exists with the given id.
        /// </exception>
        public bool AddTag(PrototypeId<TagPrototype> id)
        {
            GetTagOrThrow(id);
            var added = _tags.Add(id);

            if (added)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Tries to add the given tags if they don't already exist.
        /// </summary>
        /// <param name="ids">The tags to add.</param>
        /// <returns>true if any tags were added, false if they all already existed.</returns>
        /// <exception cref="UnknownPrototypeException">
        ///     Thrown if one of the ids represents an unregistered <see cref="TagPrototype"/>.
        /// </exception>
        public bool AddTags(params PrototypeId<TagPrototype>[] ids)
        {
            return AddTags(ids.AsEnumerable());
        }

        /// <summary>
        ///     Tries to add the given tags if they don't already exist.
        /// </summary>
        /// <param name="ids">The tags to add.</param>
        /// <returns>true if any tags were added, false if they all already existed.</returns>
        /// <exception cref="UnknownPrototypeException">
        ///     Thrown if one of the ids represents an unregistered <see cref="TagPrototype"/>.
        /// </exception>
        public bool AddTags(IEnumerable<PrototypeId<TagPrototype>> ids)
        {
            var count = _tags.Count;
            var prototypeManager = IoCManager.Resolve<IPrototypeManager>();

            foreach (var id in ids)
            {
                GetTagOrThrow(id, prototypeManager);
                _tags.Add(id);
            }

            return _tags.Count > count;
        }

        /// <summary>
        ///     Checks if a tag has been added.
        /// </summary>
        /// <param name="id">The tag to check for.</param>
        /// <returns>true if it exists, false otherwise.</returns>
        /// <exception cref="UnknownPrototypeException">
        ///     Thrown if no <see cref="TagPrototype"/> exists with the given id.
        /// </exception>
        public bool HasTag(PrototypeId<TagPrototype> id)
        {
            GetTagOrThrow(id);
            return _tags.Contains(id);
        }

        /// <summary>
        ///     Checks if all of the given tags have been added.
        /// </summary>
        /// <param name="ids">The tags to check for.</param>
        /// <returns>true if they all exist, false otherwise.</returns>
        /// <exception cref="UnknownPrototypeException">
        ///     Thrown if one of the ids represents an unregistered <see cref="TagPrototype"/>.
        /// </exception>
        public bool HasAllTags(params PrototypeId<TagPrototype>[] ids)
        {
            return HasAllTags(ids.AsEnumerable());
        }

        /// <summary>
        ///     Checks if all of the given tags have been added.
        /// </summary>
        /// <param name="ids">The tags to check for.</param>
        /// <returns>true if they all exist, false otherwise.</returns>
        /// <exception cref="UnknownPrototypeException">
        ///     Thrown if one of the ids represents an unregistered <see cref="TagPrototype"/>.
        /// </exception>
        public bool HasAllTags(IEnumerable<PrototypeId<TagPrototype>> ids)
        {
            var prototypeManager = IoCManager.Resolve<IPrototypeManager>();

            foreach (var id in ids)
            {
                GetTagOrThrow(id, prototypeManager);

                if (!_tags.Contains(id))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///     Checks if any of the given tags have been added.
        /// </summary>
        /// <param name="ids">The tags to check for.</param>
        /// <returns>true if any of them exist, false otherwise.</returns>
        /// <exception cref="UnknownPrototypeException">
        ///     Thrown if one of the ids represents an unregistered <see cref="TagPrototype"/>.
        /// </exception>
        public bool HasAnyTag(params PrototypeId<TagPrototype>[] ids)
        {
            return HasAnyTag(ids.AsEnumerable());
        }

        /// <summary>
        ///     Checks if any of the given tags have been added.
        /// </summary>
        /// <param name="ids">The tags to check for.</param>
        /// <returns>true if any of them exist, false otherwise.</returns>
        /// <exception cref="UnknownPrototypeException">
        ///     Thrown if one of the ids represents an unregistered <see cref="TagPrototype"/>.
        /// </exception>
        public bool HasAnyTag(IEnumerable<PrototypeId<TagPrototype>> ids)
        {
            var prototypeManager = IoCManager.Resolve<IPrototypeManager>();

            foreach (var id in ids)
            {
                GetTagOrThrow(id, prototypeManager);

                if (_tags.Contains(id))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Tries to remove a tag if it exists.
        /// </summary>
        /// <param name="id">The tag to remove.</param>
        /// <returns>
        ///     true if it was removed, false otherwise even if it didn't exist.
        /// </returns>
        /// <exception cref="UnknownPrototypeException">
        ///     Thrown if no <see cref="TagPrototype"/> exists with the given id.
        /// </exception>
        public bool RemoveTag(PrototypeId<TagPrototype> id)
        {
            GetTagOrThrow(id);
            return _tags.Remove(id);
        }

        /// <summary>
        ///     Tries to remove all of the given tags if they exist.
        /// </summary>
        /// <param name="ids">The tags to remove.</param>
        /// <returns>
        ///     true if it was removed, false otherwise even if they didn't exist.
        /// </returns>
        /// <exception cref="UnknownPrototypeException">
        ///     Thrown if one of the ids represents an unregistered <see cref="TagPrototype"/>.
        /// </exception>
        public bool RemoveTags(params PrototypeId<TagPrototype>[] ids)
        {
            return RemoveTags(ids.AsEnumerable());
        }

        /// <summary>
        ///     Tries to remove all of the given tags if they exist.
        /// </summary>
        /// <param name="ids">The tags to remove.</param>
        /// <returns>true if any tag was removed, false otherwise.</returns>
        /// <exception cref="UnknownPrototypeException">
        ///     Thrown if one of the ids represents an unregistered <see cref="TagPrototype"/>.
        /// </exception>
        public bool RemoveTags(IEnumerable<PrototypeId<TagPrototype>> ids)
        {
            var count = _tags.Count;
            var prototypeManager = IoCManager.Resolve<IPrototypeManager>();

            foreach (var id in ids)
            {
                GetTagOrThrow(id, prototypeManager);
                _tags.Remove(id);
            }

            return _tags.Count < count;
        }
    }
}
