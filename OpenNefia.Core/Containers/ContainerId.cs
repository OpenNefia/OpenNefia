using System;
using System.Diagnostics.Contracts;

namespace OpenNefia.Core.Containers
{
    /// <summary>
    /// Strong type wrapper around a string identifier for a container instance.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct ContainerId : IEquatable<ContainerId>, IComparable<ContainerId>
    {
        readonly string _id;

        /// <summary>
        ///     An Invalid container ID you can compare against.
        /// </summary>
        public static readonly ContainerId Invalid = new("");

        /// <summary>
        ///     Creates an instance of this structure.
        /// </summary>
        public ContainerId(string id)
        {
            _id = id;
        }

        [Pure]
        public bool IsValid()
        {
            return _id != string.Empty;
        }

        /// <inheritdoc />
        public bool Equals(ContainerId other)
        {
            return _id == other._id;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ContainerId id && Equals(id);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return string.GetHashCode(_id);
        }

        /// <summary>
        ///     Check for equality by value between two objects.
        /// </summary>
        public static bool operator ==(ContainerId a, ContainerId b)
        {
            return a._id == b._id;
        }

        /// <summary>
        ///     Check for inequality by value between two objects.
        /// </summary>
        public static bool operator !=(ContainerId a, ContainerId b)
        {
            return !(a == b);
        }

        /// <summary>
        ///     Explicit conversion of PrototypeId to string. This should only be used in special
        ///     cases like serialization. Do NOT use this in content.
        /// </summary>
        public static explicit operator string(ContainerId self)
        {
            return self._id;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return _id.ToString();
        }

        /// <inheritdoc />
        public int CompareTo(ContainerId other)
        {
            return _id.CompareTo(other._id);
        }
    }
}
