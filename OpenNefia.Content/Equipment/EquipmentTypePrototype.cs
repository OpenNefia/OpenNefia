using OpenNefia.Content.Qualities;
using OpenNefia.Content.RandomGen;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Equipment
{
    [Prototype("Elona.EquipmentType")]
    public class EquipmentTypePrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; } = default!;
    }
}
