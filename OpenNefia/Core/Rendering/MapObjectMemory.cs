using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;

namespace OpenNefia.Core.Rendering
{
    public enum ShadowType
    {
        None,
        Normal,
        DropShadow
    }

    internal enum MemoryState
    {
        Added,
        InUse,
        Removed
    }

    [Serializable]
    public class MapObjectMemory
    {
        public EntityUid ObjectUid;
        public bool IsVisible;
        public string ChipIndex = string.Empty;
        public Vector2i ScreenOffset;
        public float Rotation;
        public Color Color;
        public ShadowType ShadowType;

        internal int Index;
        internal MapCoordinates Coords;
        internal int ZOrder;
        internal MemoryState State;
    }
}
