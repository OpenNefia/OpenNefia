using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;

namespace OpenNefia.Content.PCCs
{
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
        public int? ZOrder { get; set; }

        public PCCPart()
        {
        }

        public PCCPart(PCCPartType type, ResourcePath imagePath, Color color, int zOrder)
        {
            Type = type;
            ImagePath = imagePath;
            Color = color;
            ZOrder = zOrder;
        }
    }
}
