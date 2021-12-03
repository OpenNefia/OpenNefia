using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Serialization;

namespace OpenNefia.Core.UI
{
    [Serializable]
    public struct Keybind : IComparable, IComparable<Keybind>, IEquatable<Keybind>, ISelfSerialize, IKeybind
    {
        public readonly string KeybindName;

        public bool IsShiftDelayed => false;

        public Keybind(string name)
        {
            KeybindName = name;
        }

        public static implicit operator Keybind(string name)
        {
            return new(name);
        }

        public override readonly string ToString()
        {
            return $"Keybind({KeybindName})";
        }

        #region Code for easy equality and sorting.

        public readonly int CompareTo(object? obj)
        {
            if (!(obj is Keybind func))
            {
                return 1;
            }
            return CompareTo(func);
        }

        public readonly int CompareTo(Keybind other)
        {
            return string.Compare(KeybindName, other.KeybindName, StringComparison.InvariantCultureIgnoreCase);
        }

        // Could maybe go dirty and optimize these on the assumption that they're singletons.
        public override readonly bool Equals(object? obj)
        {
            return obj is Keybind func && Equals(func);
        }

        public readonly bool Equals(Keybind other)
        {
            return other.KeybindName == KeybindName;
        }

        public override readonly int GetHashCode()
        {
            return KeybindName.GetHashCode();
        }

        public static bool operator ==(Keybind a, Keybind b)
        {
            return a.KeybindName == b.KeybindName;
        }

        public static bool operator !=(Keybind a, Keybind b)
        {
            return !(a == b);
        }

        #endregion

        public void Deserialize(string value)
        {
            this = new Keybind(value);
        }

        public readonly string Serialize()
        {
            return KeybindName;
        }
    }
}