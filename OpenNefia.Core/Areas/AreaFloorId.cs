using System;
using System.Diagnostics.Contracts;

namespace OpenNefia.Core.Areas
{
    /// <summary>
    /// Strong type wrapper around a string identifier for a container instance.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct AreaFloorId : IEquatable<AreaFloorId>, IComparable<AreaFloorId>
    {
        readonly string _id;

        /// <summary>
        ///     An Invalid area floor ID you can compare against.
        /// </summary>
        public static readonly AreaFloorId Invalid = new("");

        /// <summary>
        ///     Creates an instance of this structure.
        /// </summary>
        public AreaFloorId(string id)
        {
            _id = id;
        }

        [Pure]
        public bool IsValid()
        {
            return _id != string.Empty;
        }

        /// <inheritdoc />
        public bool Equals(AreaFloorId other)
        {
            return _id == other._id;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is AreaFloorId id && Equals(id);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return string.GetHashCode(_id);
        }

        /// <summary>
        ///     Check for equality by value between two objects.
        /// </summary>
        public static bool operator ==(AreaFloorId a, AreaFloorId b)
        {
            return a._id == b._id;
        }

        /// <summary>
        ///     Check for inequality by value between two objects.
        /// </summary>
        public static bool operator !=(AreaFloorId a, AreaFloorId b)
        {
            return !(a == b);
        }

        /// <summary>
        ///     Explicit conversion of <see cref="AreaFloorId"/> to string. This should only be used in special
        ///     cases like serialization. Do NOT use this in content.
        /// </summary>
        public static explicit operator string(AreaFloorId self)
        {
            return self._id;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return _id.ToString();
        }

        /// <inheritdoc />
        public int CompareTo(AreaFloorId other)
        {
            return _id.CompareTo(other._id);
        }
    }
}
