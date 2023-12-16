using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;

namespace OpenNefia.Content.PCCs
{
    /// <summary>
    /// A single instantiated PCC part.
    /// </summary>
    /// <remarks>
    /// Prototypes/theming aren't supported here, mostly because PCC parts
    /// are a big collection of different choices meant to be added to, so
    /// it becomes unclear which parts should replace others.
    /// </remarks>
    [DataDefinition]
    public sealed class PCCPart
    {
        [DataField]
        public PCCPartType Type { get; }

        [DataField]
        public ResourcePath ImagePath { get; } = default!;

        [DataField]
        public Color Color { get; set; } = Color.White;

        [DataField]
        public int ZOrder { get; set; }

        // Zero-arg constructor for serialization
        public PCCPart()
        {
        }

        public PCCPart(PCCPartType type, ResourcePath imagePath, Color? color = null, int? zOrder = null)
        {
            Type = type;
            ImagePath = imagePath;
            Color = color ?? Color.White;
            ZOrder = zOrder ?? PCCHelpers.GetPCCPartTypeZOrder(type);
        }
    }
}
