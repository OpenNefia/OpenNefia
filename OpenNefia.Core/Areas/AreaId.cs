namespace OpenNefia.Core.Areas
{
    [Serializable]
    public readonly struct AreaId : IEquatable<AreaId>
    {
        public static readonly AreaId Nullspace = new(0);
        public static readonly AreaId FirstId = new(1);

        internal readonly int Value;

        public AreaId(int value)
        {
            Value = value;
        }

        /// <inheritdoc />
        public bool Equals(AreaId other)
        {
            return Value == other.Value;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is AreaId id && Equals(id);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Value;
        }

        public static bool operator ==(AreaId a, AreaId b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(AreaId a, AreaId b)
        {
            return !(a == b);
        }

        public static explicit operator int(AreaId self)
        {
            return self.Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
