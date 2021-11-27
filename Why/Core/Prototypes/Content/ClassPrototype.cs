using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;

namespace OpenNefia.Core.Prototypes
{
    [Prototype("Class")]
    public class ClassPrototype : IPrototype
    {
        [DataField("id", required: true)]
        public string ID { get; } = default!;

        [DataField(required: true, customTypeSerializer: typeof(PrototypeIdDictionarySerializer<SkillPrototype, int>))]
        public Dictionary<PrototypeId<SkillPrototype>, int> BaseSkills = new();
    }
}
