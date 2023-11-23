using OpenNefia.Core.GameObjects;
using System.Diagnostics.Contracts;

namespace OpenNefia.Content.Dialog
{
    /// <summary>
    /// Reference to a property inside an active <see cref="IEntitySystem"/>, so that
    /// it can be addressed via YAML in a declarative way.
    /// </summary>
    public struct EntitySystemPropertyRef
    {
        public EntitySystemPropertyRef(Type systemType, string propertyName)
        {
            SystemType = systemType;
            PropertyName = propertyName;
        }

        public Type SystemType { get; }
        public string PropertyName { get; }

        /// <inheritdoc />
        public bool Equals(EntitySystemPropertyRef other)
        {
            return SystemType == other.SystemType && PropertyName == other.PropertyName;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is EntitySystemPropertyRef id && Equals(id);
        }

        [Pure]
        public bool IsValid()
        {
            return SystemType.FullName != null && SystemType.IsAssignableTo(typeof(IEntitySystem));
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return HashCode.Combine(SystemType.GetHashCode(), string.GetHashCode(PropertyName));
        }

        /// <summary>
        ///     Check for equality by value between two objects.
        /// </summary>
        public static bool operator ==(EntitySystemPropertyRef a, EntitySystemPropertyRef b)
        {
            return a.SystemType == b.SystemType && a.PropertyName == b.PropertyName;
        }

        /// <summary>
        ///     Check for inequality by value between two objects.
        /// </summary>
        public static bool operator !=(EntitySystemPropertyRef a, EntitySystemPropertyRef b)
        {
            return !(a == b);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"EntitySystem@{SystemType.FullName!}:{PropertyName}";
        }

        /// <inheritdoc />
        public int CompareTo(EntitySystemPropertyRef other)
        {
            var comp1 = SystemType.FullName!.CompareTo(other.SystemType.FullName!);
            if (comp1 != 0) return comp1;
            return PropertyName.CompareTo(other.PropertyName);
        }
    }
}
