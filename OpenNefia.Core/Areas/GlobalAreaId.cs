using System;
using System.Diagnostics.Contracts;

namespace OpenNefia.Core.Areas
{
    /// <summary>
    /// Strong type wrapper around a string identifier for a global area.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct GlobalAreaId : IEquatable<GlobalAreaId>, IComparable<GlobalAreaId>
    {
        readonly string _id;

        /// <summary>
        ///     An Invalid area floor ID you can compare against.
        /// </summary>
        public static readonly GlobalAreaId Invalid = new("");

        /// <summary>
        ///     Creates an instance of this structure.
        /// </summary>
        public GlobalAreaId(string id)
        {
            _id = id;
        }

        [Pure]
        public bool IsValid()
        {
            return _id != string.Empty;
        }

        /// <inheritdoc />
        public bool Equals(GlobalAreaId other)
        {
            return _id == other._id;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is GlobalAreaId id && Equals(id);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return string.GetHashCode(_id);
        }

        /// <summary>
        ///     Check for equality by value between two objects.
        /// </summary>
        public static bool operator ==(GlobalAreaId a, GlobalAreaId b)
        {
            return a._id == b._id;
        }

        /// <summary>
        ///     Check for inequality by value between two objects.
        /// </summary>
        public static bool operator !=(GlobalAreaId a, GlobalAreaId b)
        {
            return !(a == b);
        }

        /// <summary>
        ///     Explicit conversion of <see cref="GlobalAreaId"/> to string. This should only be used in special
        ///     cases like serialization. Do NOT use this in content.
        /// </summary>
        public static explicit operator string(GlobalAreaId self)
        {
            return self._id;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return _id.ToString();
        }

        /// <inheritdoc />
        public int CompareTo(GlobalAreaId other)
        {
            return _id.CompareTo(other._id);
        }
    }
}
