using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Effects;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.Skills;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using static OpenNefia.Core.Prototypes.EntityPrototype;

namespace OpenNefia.Content.Prototypes
{
    [Prototype("Elona.Class")]
    public class ClassPrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; } = default!;

        [DataField]
        public bool IsExtra { get; } = false;

        [DataField]
        public IEffect? OnInitPlayer { get; } = null;

        [DataField]
        public PrototypeId<EquipmentTypePrototype>? EquipmentType { get; } = null;

        [DataField("baseSkills")]
        private Dictionary<PrototypeId<SkillPrototype>, int> _baseSkills = new();

        public IReadOnlyDictionary<PrototypeId<SkillPrototype>, int> BaseSkills => _baseSkills;

        [DataField]
        public ComponentRegistry Components { get; } = new();
    }
}