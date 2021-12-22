using System.Collections.Generic;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Containers
{
    /// <summary>
    /// Base container class that all container inherit from.
    /// </summary>
    public abstract class BaseContainer : IContainer
    {
        /// <inheritdoc />
        public abstract IReadOnlyList<EntityUid> ContainedEntities { get; }

        /// <inheritdoc />
        public abstract string ContainerType { get; }

        /// <inheritdoc />
        public bool Deleted { get; private set; }

        /// <inheritdoc />
        public ContainerId ID { get; internal set; } = default!; // Make sure you set me in init

        /// <inheritdoc />
        public IContainerManager Manager { get; internal set; } = default!; // Make sure you set me in init

        /// <inheritdoc />
        [DataField("occludes")]
        public bool OccludesLight { get; set; } = true;

        /// <inheritdoc />
        public EntityUid Owner => Manager.OwnerUid;

        /// <inheritdoc />
        [DataField]
        public bool ShowContents { get; set; }

        /// <summary>
        /// DO NOT CALL THIS METHOD DIRECTLY!
        /// You want <see cref="IContainerManager.MakeContainer{T}(string)" /> instead.
        /// </summary>
        protected BaseContainer() { }

        /// <inheritdoc />
        public bool Insert(EntityUid toinsert, IEntityManager? entMan = null)
        {
            DebugTools.Assert(!Deleted);
            IoCManager.Resolve(ref entMan);

            //Verify we can insert into this container
            if (!CanInsert(toinsert, entMan))
                return false;

            var transform = entMan.GetComponent<SpatialComponent>(toinsert);

            // CanInsert already checks nullability of Parent (or container forgot to call base that does)
            if (ContainerHelpers.TryGetContainerMan(toinsert, out var containerManager) 
                && !ContainerHelpers.RemoveEntity(containerManager.OwnerUid, toinsert, containerManager: containerManager))
                return false; // Can't remove from existing container, can't insert.

            // Attach to parent first so we can check IsInContainer more easily.
            transform.AttachParent(entMan.GetComponent<SpatialComponent>(Owner));
            InternalInsert(toinsert);

            // spatially move the object to the location of the container. If you don't want this functionality, the
            // calling code can save the local position before calling this function, and apply it afterwords.
            transform.LocalPosition = Vector2i.Zero;

            return true;
        }

        /// <inheritdoc />
        public virtual bool CanInsert(EntityUid toinsert, IEntityManager? entMan = null)
        {
            DebugTools.Assert(!Deleted);

            // cannot insert into itself.
            if (Owner == toinsert)
                return false;

            IoCManager.Resolve(ref entMan);

            // no, you can't put maps into containers
            if (entMan.HasComponent<MapComponent>(toinsert))
                return false;

            // Crucial, prevent circular insertion.
            return !entMan.GetComponent<SpatialComponent>(toinsert).ContainsEntity(entMan.GetComponent<SpatialComponent>(Owner));

            //Improvement: Traverse the entire tree to make sure we are not creating a loop.
        }

        /// <inheritdoc />
        public bool Remove(EntityUid toremove, IEntityManager? entMan = null)
        {
            DebugTools.Assert(!Deleted);
            DebugTools.AssertNotNull(Manager);
            DebugTools.AssertNotNull(toremove);
            IoCManager.Resolve(ref entMan);
            DebugTools.Assert(entMan.EntityExists(toremove));

            if (!CanRemove(toremove)) return false;
            InternalRemove(toremove);

            ContainerHelpers.AttachParentToContainerOrGrid(entMan.GetComponent<SpatialComponent>(toremove));
            return true;
        }

        /// <inheritdoc />
        public void ForceRemove(EntityUid toRemove, IEntityManager? entMan = null)
        {
            DebugTools.Assert(!Deleted);
            DebugTools.AssertNotNull(Manager);
            DebugTools.AssertNotNull(toRemove);
            IoCManager.Resolve(ref entMan);
            DebugTools.Assert(entMan.EntityExists(toRemove));

            InternalRemove(toRemove);
        }

        /// <inheritdoc />
        public virtual bool CanRemove(EntityUid toremove)
        {
            DebugTools.Assert(!Deleted);
            return Contains(toremove);
        }

        /// <inheritdoc />
        public abstract bool Contains(EntityUid contained);

        /// <inheritdoc />
        public virtual void Shutdown()
        {
            Manager.InternalContainerShutdown(this);
            Deleted = true;
        }

        /// <summary>
        /// Implement to store the reference in whatever form you want
        /// </summary>
        /// <param name="toinsert"></param>
        protected virtual void InternalInsert(EntityUid toinsert, IEntityManager? entMan = null)
        {
            DebugTools.Assert(!Deleted);
            IoCManager.Resolve(ref entMan);

            entMan.EventBus.RaiseLocalEvent(Owner, new EntInsertedIntoContainerEventArgs(toinsert, this));
            entMan.EventBus.RaiseEvent(EventSource.Local, new UpdateContainerOcclusionEvent(toinsert));
        }

        /// <summary>
        /// Implement to remove the reference you used to store the entity
        /// </summary>
        /// <param name="toremove"></param>
        protected virtual void InternalRemove(EntityUid toremove, IEntityManager? entMan = null)
        {
            DebugTools.Assert(!Deleted);
            DebugTools.AssertNotNull(Manager);
            DebugTools.AssertNotNull(toremove);
            IoCManager.Resolve(ref entMan);
            DebugTools.Assert(entMan.EntityExists(toremove));

            entMan.EventBus.RaiseLocalEvent(Owner, new EntRemovedFromContainerEventArgs(toremove, this));
            entMan.EventBus.RaiseEvent(EventSource.Local, new UpdateContainerOcclusionEvent(toremove));
        }
    }
}
