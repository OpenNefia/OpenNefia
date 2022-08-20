using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;

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

    [DataDefinition]
    public class MapObjectMemory
    {
        [DataField]
        public EntityUid ObjectUid;

        [DataField]
        public bool IsVisible;

        [DataField]
        public string AtlasIndex = string.Empty;

        [DataField]
        public Vector2i ScreenOffset;

        [DataField]
        public float Rotation;

        [DataField]
        public Color Color;

        [DataField]
        public ShadowType ShadowType = ShadowType.None;

        [DataField]
        public int ZOrder;

        [DataField]
        public bool HideWhenOutOfSight;

        [DataField]
        public float ShadowRotationRads { get; set; } = 0.15f;


        [DataField]
        internal int Index;

        [DataField]
        internal MapCoordinates Coords;

        [DataField]
        internal MemoryState State;
    }
}
