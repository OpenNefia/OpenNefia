﻿using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization;

namespace OpenNefia.Core.Maps
{
    /// <summary>
    ///     A set of coordinates relative to another entity.
    /// </summary>
    [PublicAPI]
    [Serializable]
    public readonly struct EntityCoordinates : IEquatable<EntityCoordinates>
    {
        public static readonly EntityCoordinates Invalid = new(EntityUid.Invalid, Vector2i.Zero);

        /// <summary>
        ///     ID of the entity that this position is relative to.
        /// </summary>
        public readonly EntityUid EntityId;

        /// <summary>
        ///     Position in the entity's local space.
        /// </summary>
        public readonly Vector2i Position;

        /// <summary>
        ///     Location of the X axis local to the entity.
        /// </summary>
        public int X => Position.X;

        /// <summary>
        ///     Location of the Y axis local to the entity.
        /// </summary>
        public int Y => Position.Y;

        /// <summary>
        ///     Constructs a new instance of <see cref="EntityCoordinates"/>.
        /// </summary>
        /// <param name="entityId">ID of the entity that this position is relative to.</param>
        /// <param name="position">Position in the entity's local space.</param>
        public EntityCoordinates(EntityUid entityId, Vector2i position)
        {
            EntityId = entityId;
            Position = position;
        }

        public EntityCoordinates(EntityUid entityId, int x, int y)
        {
            EntityId = entityId;
            Position = new Vector2i(x, y);
        }

        /// <summary>
        ///     Verifies that this set of coordinates can be currently resolved to a location.
        /// </summary>
        /// <param name="entityManager">Entity Manager containing the entity Id.</param>
        /// <returns><see langword="true" /> if this set of coordinates can be currently resolved to a location, otherwise <see langword="false" />.</returns>
        public bool IsValid(IEntityManager entityManager)
        {
            if (!EntityId.IsValid() || !entityManager.EntityExists(EntityId))
                return false;

            if (!float.IsFinite(Position.X) || !float.IsFinite(Position.Y))
                return false;

            return true;
        }

        /// <summary>
        ///     Transforms this set of coordinates from the entity's local space to the map space.
        /// </summary>
        /// <param name="entityManager">Entity Manager containing the entity Id.</param>
        /// <returns></returns>
        public MapCoordinates ToMap(IEntityManager entityManager)
        {
            if(!IsValid(entityManager))
                return MapCoordinates.Nullspace;

            var transform = entityManager.GetEntity(EntityId).Spatial;
            var worldPos = (Vector2i)transform.WorldMatrix.Transform(Position);
            return new MapCoordinates(worldPos, transform.MapID);
        }

        /// <summary>
        ///    Transform this set of coordinates from the entity's local space to the map space.
        /// </summary>
        /// <param name="entityManager">Entity Manager containing the entity Id.</param>
        /// <returns></returns>
        public Vector2 ToMapPos(IEntityManager entityManager)
        {
            return ToMap(entityManager).Position;
        }

        /// <summary>
        ///    Creates EntityCoordinates given an entity and some MapCoordinates.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">If <see cref="entity"/> is not on the same map as the <see cref="coordinates"/>.</exception>
        public static EntityCoordinates FromMap(Entity entity, MapCoordinates coordinates)
        {
            if(entity.Spatial.MapID != coordinates.MapId)
                throw new InvalidOperationException("Entity is not on the same map!");

            var localPos = (Vector2i)entity.Spatial.InvWorldMatrix.Transform(coordinates.Position);
            return new EntityCoordinates(entity.Uid, localPos);
        }

        /// <summary>
        ///    Creates EntityCoordinates given an entity Uid and some MapCoordinates.
        /// </summary>
        /// <param name="entityManager">Entity Manager containing the entity Id.</param>
        /// <param name="entityUid"></param>
        /// <param name="coordinates"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">If <see cref="entityUid"/> is not on the same map as the <see cref="coordinates"/>.</exception>
        public static EntityCoordinates FromMap(IEntityManager entityManager, EntityUid entityUid, MapCoordinates coordinates)
        {
            var entity = entityManager.GetEntity(entityUid);

            return FromMap(entity, coordinates);
        }

        /// <summary>
        ///    Creates a set of EntityCoordinates given some MapCoordinates.
        /// </summary>
        /// <param name="mapManager"></param>
        /// <param name="coordinates"></param>
        public static EntityCoordinates FromMap(IMapManager mapManager, MapCoordinates coordinates)
        {
            var mapId = coordinates.MapId;
            var mapEntity = mapManager.GetMapEntity(mapId);

            return new EntityCoordinates(mapEntity.Uid, coordinates.Position);
        }

        /// <summary>
        ///     Converts this set of coordinates to Vector2i.
        /// </summary>
        /// <param name="entityManager"></param>
        /// <param name="mapManager"></param>
        /// <returns></returns>
        public Vector2i ToVector2i(IEntityManager entityManager)
        {
            if(!IsValid(entityManager))
                return new Vector2i();

            var (x, y) = ToMapPos(entityManager);

            return new Vector2i((int)Math.Floor(x), (int)Math.Floor(y));
        }

        /// <summary>
        ///     Returns the Map Id these coordinates are on.
        ///     If the relative entity is not valid, returns <see cref="MapId.Nullspace"/> instead.
        /// </summary>
        /// <param name="entityManager"></param>
        /// <returns>Map Id these coordinates are on or <see cref="MapId.Nullspace"/></returns>
        public MapId GetMapId(IEntityManager entityManager)
        {
            return !IsValid(entityManager) ? MapId.Nullspace : GetEntity(entityManager).Spatial.MapID;
        }

        /// <summary>
        ///     Returns a reference to the relative entity.
        /// </summary>
        /// <param name="entityManager"></param>
        /// <returns>Relative entity or throws if entity id doesn't exist</returns>
        public Entity GetEntity(IEntityManager entityManager)
        {
            return entityManager.GetEntity(EntityId);
        }

        /// <summary>
        ///     Attempt to get the relative entity, returning whether or not the entity was gotten.
        /// </summary>
        /// <param name="entityManager"></param>
        /// <param name="entity">The relative entity or null if not valid</param>
        /// <returns>True if a value was returned, false otherwise.</returns>
        public bool TryGetEntity(IEntityManager entityManager, [NotNullWhen(true)] out Entity? entity)
        {
            entity = null;
            return IsValid(entityManager) && entityManager.TryGetEntity(EntityId, out entity);
        }

        /// <summary>
        /// Offsets the position by a given vector. This happens in local space.
        /// </summary>
        /// <param name="position">The vector to offset by local to the entity.</param>
        /// <returns>Newly offset coordinates.</returns>
        public EntityCoordinates Offset(Vector2i position)
        {
            return new(EntityId, Position + position);
        }

        /// <summary>
        ///     Compares two sets of coordinates to see if they are in range of each other.
        /// </summary>
        /// <param name="entityManager">Entity Manager containing the two entity Ids.</param>
        /// <param name="otherCoordinates">Other set of coordinates to use.</param>
        /// <param name="range">maximum distance between the two sets of coordinates.</param>
        /// <returns>True if the two points are within a given range.</returns>
        public bool InRange(IEntityManager entityManager, EntityCoordinates otherCoordinates, float range)
        {
            if (!IsValid(entityManager) || !otherCoordinates.IsValid(entityManager))
                return false;

            if (EntityId == otherCoordinates.EntityId)
                return (otherCoordinates.Position - Position).LengthSquared < range * range;

            var mapCoordinates = ToMap(entityManager);
            var otherMapCoordinates = otherCoordinates.ToMap(entityManager);

            return mapCoordinates.InRange(otherMapCoordinates, range);
        }

        /// <summary>
        ///     Tries to calculate the distance between two sets of coordinates.
        /// </summary>
        /// <param name="entityManager"></param>
        /// <param name="otherCoordinates"></param>
        /// <param name="distance"></param>
        /// <returns>True if it was possible to calculate the distance</returns>
        public bool TryDistance(IEntityManager entityManager, EntityCoordinates otherCoordinates, out float distance)
        {
            distance = 0f;

            if (!IsValid(entityManager) || !otherCoordinates.IsValid(entityManager))
                return false;

            if (EntityId == otherCoordinates.EntityId)
            {
                distance = (Position - otherCoordinates.Position).Length;
                return true;
            }

            var mapCoordinates = ToMap(entityManager);
            var otherMapCoordinates = otherCoordinates.ToMap(entityManager);

            return mapCoordinates.TryDistance(otherMapCoordinates, out distance);
        }

        #region IEquatable

        /// <inheritdoc />
        public bool Equals(EntityCoordinates other)
        {
            return EntityId.Equals(other.EntityId) && Position.Equals(other.Position);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is EntityCoordinates other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(EntityId, Position);
        }

        /// <summary>
        ///     Check for equality by value between two objects.
        /// </summary>
        public static bool operator ==(EntityCoordinates left, EntityCoordinates right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Check for inequality by value between two objects.
        /// </summary>
        public static bool operator !=(EntityCoordinates left, EntityCoordinates right)
        {
            return !left.Equals(right);
        }

        #endregion

        #region Operators

        /// <summary>
        ///     Returns the sum for both coordinates but only if they have the same relative entity.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the relative entities aren't the same</exception>
        public static EntityCoordinates operator +(EntityCoordinates left, EntityCoordinates right)
        {
            if(left.EntityId != right.EntityId)
                throw new ArgumentException("Can't sum EntityCoordinates with different relative entities.");

            return new EntityCoordinates(left.EntityId, left.Position + right.Position);
        }

        /// <summary>
        ///     Returns the difference for both coordinates but only if they have the same relative entity.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the relative entities aren't the same</exception>
        public static EntityCoordinates operator -(EntityCoordinates left, EntityCoordinates right)
        {
            if(left.EntityId != right.EntityId)
                throw new ArgumentException("Can't subtract EntityCoordinates with different relative entities.");

            return new EntityCoordinates(left.EntityId, left.Position - right.Position);
        }

        /// <summary>
        ///     Returns the multiplication of both coordinates but only if they have the same relative entity.
        /// </summary>
        /// <exception cref="ArgumentException">When the relative entities aren't the same</exception>
        public static EntityCoordinates operator *(EntityCoordinates left, EntityCoordinates right)
        {
            if(left.EntityId != right.EntityId)
                throw new ArgumentException("Can't multiply EntityCoordinates with different relative entities.");

            return new EntityCoordinates(left.EntityId, left.Position * right.Position);
        }

        /// <summary>
        ///     Scales the coordinates by a given factor.
        /// </summary>
        /// <exception cref="ArgumentException">When the relative entities aren't the same</exception>
        public static EntityCoordinates operator *(EntityCoordinates left, int right)
        {
            return new(left.EntityId, left.Position * right);
        }

        #endregion

        /// <summary>
        /// Deconstructs the object into it's fields.
        /// </summary>
        /// <param name="entId">ID of the entity that this position is relative to.</param>
        /// <param name="localPos">Position in the entity's local space.</param>
        public void Deconstruct(out EntityUid entId, out Vector2 localPos)
        {
            entId = EntityId;
            localPos = Position;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"EntId={EntityId}, X={Position.X:N2}, Y={Position.Y:N2}";
        }
    }
}
