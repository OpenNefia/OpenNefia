using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.GameObjects
{
    [RegisterComponent]
    public class CharaComponent : Component
    {
        public override string Name => "Chara";

        [DataField]
        public string DisplayName { get; set; } = string.Empty;

        [DataField]
        public string Title { get; set; } = string.Empty;

        [DataField]
        public bool HasFullName { get; set; } = false;

        [DataField(required: true)]
        public PrototypeId<ClassPrototype> Class { get; set; } = default;
    }
}