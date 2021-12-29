using OpenNefia.Core.Serialization;
using DrawDepthTag = OpenNefia.Core.GameObjects.DrawDepth;

namespace OpenNefia.Content.DrawDepth
{
    /// <summary>
    /// Sprite Z-ordering for content entities.
    /// </summary>
    [ConstantsFor(typeof(DrawDepthTag))]
    public enum DrawDepth
    {
        Feats = DrawDepthTag.Default,

        Mefs = DrawDepthTag.Default + 2,

        Items = DrawDepthTag.Default + 4,

        Characters = DrawDepthTag.Default + 6,

        /// <summary>
        ///     Explosions, fire, melee swings. Whatever.
        /// </summary>
        Effects = DrawDepthTag.Default + 8,

        /// <summary>
        ///    Use this selectively if it absolutely needs to be drawn above (almost) everything else. Examples include
        ///    the pointing arrow, the drag & drop ghost-entity, and some debug tools.
        /// </summary>
        Overlays = DrawDepthTag.Default + 10,
    }
}
