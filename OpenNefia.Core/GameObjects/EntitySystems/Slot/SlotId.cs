namespace OpenNefia.Core.GameObjects
{
    [Serializable]
    public readonly struct SlotId : IEquatable<SlotId>
    {
        public static readonly SlotId Invalid = new(0);

        internal readonly int Value;

        public SlotId(int value)
        {
            Value = value;
        }

        /// <inheritdoc />
        public bool Equals(SlotId other)
        {
            return Value == other.Value;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is SlotId id && Equals(id);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Value;
        }

        public static bool operator ==(SlotId a, SlotId b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(SlotId a, SlotId b)
        {
            return !(a == b);
        }

        public static explicit operator int(SlotId self)
        {
            return self.Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
}
}
