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
    
    /// <summary>
    /// A single "map object" that is rendered at a tile position on a map, and can be
    /// memorized to be shown in the unseen fog of war. Must be serializable.
    /// </summary>
    [DataDefinition]
    public sealed class MapObjectMemory
    {
        /// <summary>
        /// Entity that generated this memory.
        /// </summary>
        [DataField]
        public EntityUid ObjectUid;

        /// <summary>
        /// Shows or hides the memory.
        /// </summary>
        [DataField]
        public bool IsVisible;

        /// <summary>
        /// Index in the chip atlas containing the sprite to render.
        /// If null, do not render a chip.
        /// </summary>
        [DataField]
        public string? AtlasIndex = null;

        /// <summary>
        /// Offset in physical pixels that is added to the map position during rendering.
        /// For sprites like paintings that have a Y offset from the ground.
        /// </summary>
        // TODO needs to be Vector2 and virtual pixels
        [DataField]
        public Vector2i ScreenOffset;

        /// <summary>
        /// Rotation of the chip.
        /// </summary>
        [DataField]
        public float Rotation;

        /// <summary>
        /// Color of the chip.
        /// </summary>
        [DataField]
        public Color Color;

        /// <summary>
        /// Type of shadow to render. Can be a spot or drop (fancy) shadow.
        /// </summary>
        [DataField]
        public ShadowType ShadowType = ShadowType.None;

        /// <summary>
        /// Ordering of this memory relative to others. Lower is higher priority.
        /// </summary>
        [DataField]
        public int ZOrder;

        /// <summary>
        /// If true, this memory will be hidden when the player cannot
        /// see the tile it is on.
        /// </summary>
        [DataField]
        public bool HideWhenOutOfSight;

        /// <summary>
        /// Rotation of the shadow if it's a <see cref="ShadowType.DropShadow"/>.
        /// Some item chips override this amount.
        /// </summary>
        [DataField]
        public float ShadowRotationRads { get; set; } = 0.15f;

        /// <summary>
        /// Drawables attached to this entity.
        /// </summary>
        [DataField]
        public List<IEntityDrawable> Drawables { get; set; } = new();

        /// <summary>
        /// Location of the memory on the map.
        /// </summary>
        [DataField]
        internal MapCoordinates Coords;

        /// <summary>
        /// Internal ID of the memory for bookkeeping.
        /// </summary>
        [DataField]
        internal int Index;

        /// <summary>
        /// State of this memory for bookkeeping. Memory that has
        /// been cleared can be reused without needing to reallocate anything.
        /// </summary>
        [DataField]
        internal MemoryState State;
    }
}
