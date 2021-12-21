﻿using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenNefia.Core.GameObjects
{
    /// <summary>
    ///     Stores the position and orientation of the entity.
    /// </summary>
    public sealed class SpatialComponent : Component
    {
        [DataField("parent")]
        private EntityUid _parent;
        [DataField("pos")]
        private Vector2i _localPosition = Vector2i.Zero; // holds offset from grid, or offset from parent

        private Matrix3 _localMatrix = Matrix3.Identity;
        private Matrix3 _invLocalMatrix = Matrix3.Identity;

        private Vector2? _nextPosition;
        private Angle? _nextRotation;

        private Vector2 _prevPosition;
        private Angle _prevRotation;

        private readonly SortedSet<EntityUid> _children = new();

        [Dependency] private readonly IMapManager _mapManager = default!;

        public override string Name => "Spatial";

        /// <summary>
        ///     Returns the index of the map which this object is on
        /// </summary>
        public MapId MapID { get; private set; }

        private bool _mapIdInitialized;

        /// <summary>
        ///     Reference to the transform of the container of this object if it exists, can be nested several times.
        /// </summary>
        public SpatialComponent? Parent
        {
            get => !_parent.IsValid() ? null : Owner.EntityManager.GetComponent<SpatialComponent>(_parent);
            internal set
            {
                if (value != null)
                {
                    AttachParent(value);
                }
                else
                {
                    AttachToMap();
                }
            }
        }

        /// <summary>
        /// The UID of the parent entity that this entity is attached to.
        /// </summary>
        public EntityUid ParentUid
        {
            get => _parent;
            set => Parent = Owner.EntityManager.GetComponent<SpatialComponent>(value);
        }

        /// <summary>
        ///     Matrix for transforming points from local to world space.
        /// </summary>
        public Matrix3 WorldMatrix
        {
            get
            {
                if (_parent.IsValid())
                {
                    var parentMatrix = Parent!.WorldMatrix;
                    var myMatrix = GetLocalMatrix();
                    Matrix3.Multiply(ref myMatrix, ref parentMatrix, out var result);
                    return result;
                }

                return GetLocalMatrix();
            }
        }

        /// <summary>
        ///     Matrix for transforming points from world to local space.
        /// </summary>
        public Matrix3 InvWorldMatrix
        {
            get
            {
                if (_parent.IsValid())
                {
                    var matP = Parent!.InvWorldMatrix;
                    var myMatrix = GetLocalMatrixInv();
                    Matrix3.Multiply(ref matP, ref myMatrix, out var result);
                    return result;
                }

                return GetLocalMatrixInv();
            }
        }

        /// <summary>
        ///     Current position offset of the entity relative to the world.
        ///     Can de-parent from its parent if the parent is a grid.
        /// </summary>
        public Vector2i WorldPosition
        {
            get
            {
                if (_parent.IsValid())
                {
                    // parent coords to world coords (recurses up all parents)
                    return (Vector2i)Parent!.WorldMatrix.Transform(_localPosition);
                }
                else
                {
                    return Vector2i.Zero;
                }
            }
            set => SetWorldPosition(value, false);
        }

        public void SetWorldPosition(Vector2i value, bool noEvents = false)
        {
            if (!_parent.IsValid())
            {
                DebugTools.Assert("Parent is invalid while attempting to set WorldPosition - did you try to move root node?");
                return;
            }

            // world coords to parent coords
            var newPos = (Vector2i)Parent!.InvWorldMatrix.Transform(value);

            SetLocalPosition(newPos, noEvents);
        }

        /// <summary>
        ///     Position offset of this entity relative to its parent.
        /// </summary>
        public EntityCoordinates Coordinates
        {
            get
            {
                var valid = _parent.IsValid();
                return new EntityCoordinates(valid ? _parent : OwnerUid, valid ? LocalPosition : Vector2i.Zero);
            }
            // NOTE: This setter must be callable from before initialize (inheriting from AttachParent's note)
            set => SetCoordinates(value, false);
        }

        /// <summary>
        /// If true, this entity cannot be moved over. This also causes
        /// collision events to be fired when the entity is moved into.
        /// </summary>
        [DataField]
        public bool IsSolid { get; set; }

        /// <summary>
        /// If true, this entity blocks field of view.
        /// </summary>
        [DataField]
        public bool IsOpaque { get; set; }

        [DataField]
        public Direction Direction { get; set; } = Direction.North;

        public void SetCoordinates(EntityCoordinates value, bool noEvents = false)
        {
            if (value == Coordinates)
                return;

            var sameParent = value.EntityId == _parent;

            var oldPosition = Coordinates;
            _localPosition = value.Position;
            var changedParent = false;

            if (!sameParent)
            {
                changedParent = true;
                var newParent = Owner.EntityManager.GetComponent<SpatialComponent>(value.EntityId);

                DebugTools.Assert(newParent != this,
                    $"Can't parent a {nameof(SpatialComponent)} to itself.");

                // That's already our parent, don't bother attaching again.

                var oldParent = Parent;
                var uid = OwnerUid;
                oldParent?._children.Remove(uid);
                newParent._children.Add(uid);

                // offset position from world to parent
                _parent = value.EntityId;
                ChangeMapId(newParent.MapID);

                var entParentChangedMessage = new EntParentChangedEvent(Owner, oldParent?.Owner);
                Owner.EntityManager.EventBus.RaiseLocalEvent(OwnerUid, ref entParentChangedMessage);
            }

            // These conditions roughly emulate the effects of the code before I changed things,
            //  in regards to when to rebuild matrices.
            // This may not in fact be the right thing.
            if (changedParent || oldPosition.Position != Coordinates.Position)
                RebuildMatrices();

            if (!noEvents && oldPosition.Position != Coordinates.Position)
            {
                var moveEvent = new EntPositionChangedEvent(Owner, oldPosition, Coordinates, this);
                Owner.EntityManager.EventBus.RaiseLocalEvent(OwnerUid, ref moveEvent);
            }
        }

        /// <summary>
        ///     Current position offset of the entity relative to the world.
        ///     This is effectively a more complete version of <see cref="WorldPosition"/>
        /// </summary>
        public MapCoordinates MapPosition => new(WorldPosition, MapID);

        /// <summary>
        ///     Local offset of this entity relative to its parent
        ///     (<see cref="Parent"/> if it's not null, to <see cref="GridID"/> otherwise).
        /// </summary>
        public Vector2i LocalPosition
        {
            get => _localPosition;
            set => SetLocalPosition(value, false);
        }

        public void SetLocalPosition(Vector2i value, bool noEvents = false)
        {
            if (_localPosition == value)
                return;

            // Set _nextPosition to null to break any on-going lerps if this is done in a client side prediction.
            _nextPosition = null;

            var oldPos = Coordinates;
            _localPosition = value;
            RebuildMatrices();

            if (!noEvents)
            {
                var moveEvent = new EntPositionChangedEvent(Owner, oldPos, Coordinates, this);
                Owner.EntityManager.EventBus.RaiseLocalEvent(OwnerUid, ref moveEvent);
            }
        }

        public IEnumerable<SpatialComponent> Children =>
            _children.Select(u => Owner.EntityManager.GetEntity(u).Spatial);

        public IEnumerable<EntityUid> ChildEntityUids => _children;

        public int ChildCount => _children.Count;

        protected override void Initialize()
        {
            base.Initialize();

            // Children MAY be initialized here before their parents are.
            // We do this whole dance to handle this recursively,
            // setting _mapIdInitialized along the way to avoid going to the IMapComponent every iteration.
            static MapId FindMapIdAndSet(SpatialComponent p)
            {
                if (p._mapIdInitialized)
                {
                    return p.MapID;
                }

                MapId value;
                if (p._parent.IsValid())
                {
                    value = FindMapIdAndSet((SpatialComponent)p.Parent!);
                }
                else
                {
                    // second level node, terminates recursion up the branch of the tree
                    if (p.Owner.TryGetComponent(out MapComponent? mapComp))
                    {
                        value = mapComp.MapId;
                    }
                    else
                    {
                        throw new InvalidOperationException("Transform node does not exist inside scene tree!");
                    }
                }

                p.MapID = value;
                p._mapIdInitialized = true;
                return value;
            }

            if (!_mapIdInitialized)
            {
                FindMapIdAndSet(this);

                _mapIdInitialized = true;
            }

            // Has to be done if _parent is set from ExposeData.
            if (_parent.IsValid())
            {
                // Note that _children is a SortedSet<EntityUid>,
                // so duplicate additions (which will happen) don't matter.
                ((SpatialComponent)Parent!)._children.Add(OwnerUid);
            }
        }

        protected override void Startup()
        {
            base.Startup();

            // Keep the cached matrices in sync with the fields.
            RebuildMatrices();
        }

        /// <summary>
        /// Detaches this entity from its parent.
        /// </summary>
        public void AttachToMap()
        {
            // nothing to do
            var oldParent = Parent;
            if (oldParent == null)
            {
                return;
            }

            var mapPos = MapPosition;

            Entity newMapEntity;
            if (_mapManager.MapExists(mapPos.MapId))
            {
                newMapEntity = _mapManager.GetMapEntity(mapPos.MapId);
            }
            else
            {
                DetachParentToNull();
                return;
            }

            // this would be a no-op
            var oldParentEnt = oldParent.Owner;
            if (newMapEntity == oldParentEnt)
            {
                return;
            }

            AttachParent(newMapEntity);

            // Technically we're not moving, just changing parent.
            SetWorldPosition(mapPos.Position, noEvents: true);
        }

        public void DetachParentToNull()
        {
            var oldParent = Parent;
            if (oldParent == null)
            {
                return;
            }

            var oldConcrete = (SpatialComponent)oldParent;
            var uid = OwnerUid;
            oldConcrete._children.Remove(uid);

            _parent = EntityUid.Invalid;
            var entParentChangedMessage = new EntParentChangedEvent(Owner, oldParent?.Owner);
            Owner.EntityManager.EventBus.RaiseLocalEvent(OwnerUid, ref entParentChangedMessage);
            var oldMapId = MapID;
            MapID = MapId.Nullspace;

            // Does it even make sense to call these since this is called purely from OnRemove right now?
            RebuildMatrices();
            MapIdChanged(oldMapId);
        }

        /// <summary>
        /// Sets another entity as the parent entity, maintaining world position.
        /// </summary>
        /// <param name="newParent"></param>
        public void AttachParent(SpatialComponent newParent)
        {
            //NOTE: This function must be callable from before initialize

            // don't attach to something we're already attached to
            if (ParentUid == newParent.OwnerUid)
                return;

            DebugTools.Assert(newParent != this,
                $"Can't parent a {nameof(SpatialComponent)} to itself.");

            // offset position from world to parent, and set
            Coordinates = new EntityCoordinates(newParent.OwnerUid, (Vector2i)newParent.InvWorldMatrix.Transform(WorldPosition));
        }

        internal void ChangeMapId(MapId newMapId)
        {
            if (newMapId == MapID)
                return;

            var oldMapId = MapID;

            MapID = newMapId;
            MapIdChanged(oldMapId);
            UpdateChildMapIdsRecursive(MapID, Owner.EntityManager);
        }

        private void UpdateChildMapIdsRecursive(MapId newMapId, IEntityManager entMan)
        {
            foreach (var child in _children)
            {
                var concrete = entMan.GetComponent<SpatialComponent>(child);
                var old = concrete.MapID;

                concrete.MapID = newMapId;
                concrete.MapIdChanged(old);

                if (concrete.ChildCount != 0)
                {
                    concrete.UpdateChildMapIdsRecursive(newMapId, entMan);
                }
            }
        }

        private void MapIdChanged(MapId oldId)
        {
            Owner.EntityManager.EventBus.RaiseLocalEvent(OwnerUid, new EntMapIdChangedMessage(Owner, oldId));
        }

        public void AttachParent(Entity parent)
        {
            var transform = parent.Spatial;
            AttachParent(transform);
        }

        /// <summary>
        ///     Finds the transform of the entity located on the map itself
        /// </summary>
        public SpatialComponent GetMapTransform()
        {
            if (Parent != null) //If we are not the final transform, query up the chain of parents
            {
                return Parent.GetMapTransform();
            }

            return this;
        }

        /// <summary>
        ///     Returns whether the entity of this transform contains the entity argument
        /// </summary>
        public bool ContainsEntity(SpatialComponent entityTransform)
        {
            if (entityTransform.Parent == null) //Is the entity the scene root
            {
                return false;
            }

            if (this == entityTransform.Parent) //Is this the direct container of the entity
            {
                return true;
            }
            else
            {   
                //Recursively search up the entities containers for this object
                return ContainsEntity(entityTransform.Parent);
            }
        }

        public Matrix3 GetLocalMatrix()
        {
            return _localMatrix;
        }

        public Matrix3 GetLocalMatrixInv()
        {
            return _invLocalMatrix;
        }

        private void RebuildMatrices()
        {
            var pos = _localPosition;

            if (!_parent.IsValid()) // Root Node
                pos = Vector2i.Zero;

            var posMat = Matrix3.CreateTranslation(pos);

            _localMatrix = posMat;

            var posImat = Matrix3.Invert(posMat);

            _invLocalMatrix = posImat;
        }
    }

    /// <summary>
    ///     Raised whenever an entity moves.
    ///     There is no guarantee it will be raised if they move in worldspace, only when moved relative to their parent.
    /// </summary>
    public readonly struct EntPositionChangedEvent
    {
        public EntPositionChangedEvent(Entity sender, EntityCoordinates oldPos, EntityCoordinates newPos, SpatialComponent component)
        {
            Sender = sender;
            OldPosition = oldPos;
            NewPosition = newPos;
            Component = component;
        }

        public readonly Entity Sender;
        public readonly EntityCoordinates OldPosition;
        public readonly EntityCoordinates NewPosition;
        public readonly SpatialComponent Component;
    }    

    /// <summary>
    ///     Raised when an entity parent is changed.
    /// </summary>
    public struct EntParentChangedEvent
    {
        /// <summary>
        ///     Entity that was adopted. The transform component has a property with the new parent.
        /// </summary>
        public Entity Entity { get; }

        /// <summary>
        ///     Old parent that abandoned the Entity.
        /// </summary>
        public Entity? OldParent { get; }

        /// <summary>
        ///     Creates a new instance of <see cref="EntParentChangedEvent"/>.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="oldParent"></param>
        public EntParentChangedEvent(Entity entity, Entity? oldParent)
        {
            Entity = entity;
            OldParent = oldParent;
        }
    }
}