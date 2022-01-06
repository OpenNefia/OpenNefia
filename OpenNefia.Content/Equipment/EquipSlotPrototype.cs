using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Containers;

namespace OpenNefia.Content.Equipment
{
    /// <summary>
    /// Defines an equipment slot type on an entity. An entity can have multiple
    /// EquipSlots of the same type, and and arbitrary number of parts.
    /// </summary>
    /// <remarks>The closest equivalent in Robust is their <c>SlotDefinition</c>.</remarks>
    [Prototype("Elona.EquipSlot")]
    public class EquipSlotPrototype : IPrototype
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