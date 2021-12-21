﻿using System;
using System.Diagnostics.Contracts;

namespace OpenNefia.Core.GameObjects
{
    public readonly struct EntityUid : IEquatable<EntityUid>, IComparable<EntityUid>
    {
        readonly int _uid;

        /// <summary>
        ///     An Invalid entity UID you can compare against.
        /// </summary>
        public static readonly EntityUid Invalid = new(0);

        /// <summary>
        ///     The first entity UID the entityManager should use when the manager is initialized.
        /// </summary>
        public static readonly EntityUid FirstUid = new(1);

        /// <summary>
        ///     Creates an instance of this structure.
        /// </summary>
        public EntityUid(int uid)
        {
            _uid = uid;
        }

        [Pure]
        public bool IsValid()
        {
            return _uid > 0;
        }

        /// <inheritdoc />
        public bool Equals(EntityUid other)
        {
            return _uid == other._uid;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is EntityUid id && Equals(id);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _uid;
        }

        /// <summary>
        ///     Check for equality by value between two objects.
        /// </summary>
        public static bool operator ==(EntityUid a, EntityUid b)
        {
            return a._uid == b._uid;
        }

        /// <summary>
        ///     Check for inequality by value between two objects.
        /// </summary>
        public static bool operator !=(EntityUid a, EntityUid b)
        {
            return !(a == b);
        }

        /// <summary>
        ///     Explicit conversion of EntityId to int. This should only be used in special
        ///     cases like serialization. Do NOT use this in content.
        /// </summary>
        public static explicit operator int(EntityUid self)
        {
            return self._uid;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return _uid.ToString();
        }

        /// <inheritdoc />
        public int CompareTo(EntityUid other)
        {
            return _uid.CompareTo(other._uid);
        }
    }
}
