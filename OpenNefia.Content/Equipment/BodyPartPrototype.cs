using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Content.Equipment
{
    [Prototype("Elona.BodyPart")]
    public class BodyPartPrototype : IPrototype
    {
        /// <inheritdoc />
        [DataField("id", required: true)]
        public string ID { get; private set; } = default!;

        /// <summary>
        /// Icon index for this body part.
        /// </summary>
        /// <remarks>
        /// This is a region ID in <see cref="AssetPrototypeOf.BodyPartIcons"/>. 
        /// </remarks>
        [DataField]
        public string Icon { get; } = "1";
    }
}