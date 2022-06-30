using OpenNefia.Core.Audio;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.BaseAnim
{
    [Prototype("Elona.BasicAnim", -1)]
    public class BasicAnimPrototype : IPrototype
    {
        [DataField("id", required: true)]
        public string ID { get; private set; } = default!;

        /// <summary>
        /// How much time to wait between frames of this animation.
        /// </summary>
        [DataField]
        public float FrameDelayMillis { get; } = 50f;

        /// <summary>
        /// How many frames this animation holds. 
        /// 
        /// Omit to default to the asset's <see cref="AssetDef.CountX"/> property.
        /// </summary>
        [DataField]
        public uint? FrameCount { get; }

        /// <summary>
        /// How much the asset should be rotated, in radians.
        /// </summary>
        [DataField]
        public float Rotation { get; } = 0f;

        /// <summary>
        /// Asset to display.
        /// 
        /// This asset should have a <see cref="AssetPrototype.CountX"/> property with
        /// the number of frames in the animation.
        /// </summary>
        [DataField(required: true)]
        public PrototypeId<AssetPrototype> Asset { get; } = default!;

        /// <summary>
        /// A sound to play when this animation is displayed.
        /// </summary>
        [DataField]
        public PrototypeId<SoundPrototype>? Sound;
    }
}
