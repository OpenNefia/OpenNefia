using System;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.TypeSerializers.Implementations.Custom;
using DrawDepthTag = OpenNefia.Core.GameObjects.DrawDepth;

namespace OpenNefia.Core.GameObjects
{
    /// <summary>
    /// A component for displaying a colored sprite.
    /// </summary>
    [RegisterComponent]
    public class ChipComponent : Component
    {
        public override string Name => "Chip";

        [DataField("id")]
        public PrototypeId<ChipPrototype> ChipID { get; set; } = new("Default");

        [DataField]
        public Color Color { get; set; } = Color.White;

        [DataField("drawDepth", customTypeSerializer: typeof(ConstantSerializer<DrawDepthTag>))]
        private int drawDepth = DrawDepthTag.Default;

        /// <summary>
        ///     Z-index for drawing.
        /// </summary>
        public int DrawDepth
        {
            get => drawDepth;
            set => drawDepth = value;
        }
    }
}
