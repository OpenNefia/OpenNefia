using System;
using System.Diagnostics.Contracts;

namespace OpenNefia.Core.Areas
{
    /// <summary>
    /// Strong type wrapper around a string identifier for an area floor instance.
    /// TODO: Maybe combine with <see cref="AreaId"/> into an "AreaAndFloorId" location?
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public readonly struct AreaFloorId : IEquatable<AreaFloorId>, IComparable<AreaFloorId>
    {
        readonly string _id;
        readonly int _floorNumber;

        public const string DefaultFloorName = "Default";

        /// <summary>
        ///    The floor number considered "floor 1" by the engine.
        /// </summary>
        public const int DefaultFloorNumber = 1;

        public string ID => _id;
        public int FloorNumber => _floorNumber;

        /// <summary>
        ///     An Invalid area floor ID you can compare against.
        /// </summary>
        public static readonly AreaFloorId Invalid = new("", 0);

        /// <summary>
        ///     Default floor ID.
        /// </summary>
        public static readonly AreaFloorId Default = new(DefaultFloorName, DefaultFloorNumber);

        /// <summary>
        ///     Creates an instance of this structure.
        /// </summary>
        public AreaFloorId(string id, int floorNumber)
        {
            _id = id;
            _floorNumber = floorNumber;
        }

        [Pure]
        public bool IsValid()
        {
            return _id != string.Empty;
        }

        /// <inheritdoc />
        public bool Equals(AreaFloorId other)
        {
            return _id == other._id && _floorNumber == other._floorNumber;
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
            return HashCode.Combine(string.GetHashCode(_id), _floorNumber.GetHashCode());
        }

        /// <summary>
        ///     Check for equality by value between two objects.
        /// </summary>
        public static bool operator ==(AreaFloorId a, AreaFloorId b)
        {
            return a._id == b._id && a._floorNumber == b._floorNumber;
        }

        /// <summary>
        ///     Check for inequality by value between two objects.
        /// </summary>
        public static bool operator !=(AreaFloorId a, AreaFloorId b)
        {
            return !(a == b);
        }

        public AreaFloorId WithFloorNumber(int floorNumber)
        {
            return new(ID, floorNumber);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(AreaFloorId)}(id={_id}, floorNumber={_floorNumber})";
        }

        /// <inheritdoc />
        public int CompareTo(AreaFloorId other)
        {
            var comp1 = _id.CompareTo(other._id);
            if (comp1 != 0) return comp1;
            return _floorNumber.CompareTo(other._floorNumber);
        }
    }
}
