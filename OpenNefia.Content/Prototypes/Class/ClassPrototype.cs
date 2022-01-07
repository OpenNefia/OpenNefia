using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Effects;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.Skills;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.Prototypes
{
    [Prototype("Elona.Class")]
    public class ClassPrototype : IPrototype
    {
        [DataField("id", required: true)]
        public string ID { get; } = default!;

        [DataField]
        public bool IsExtra { get; } = false;

        [DataField]
        public IEffect? OnInitPlayer { get; } = null;

        [DataField]
        public PrototypeId<EquipmentTypePrototype>? EquipmentType { get; } = null;

        [DataField(required: true)]
        public Dictionary<PrototypeId<SkillPrototype>, int> BaseSkills = new();
    }
}
