using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Why.Core.Serialization.Manager.Attributes;

namespace Why.Core.GameObjects
{
    [RegisterComponent]
    public class CharaComponent : Component
    {
        public override string Name => "Chara";

        [DataField("displayName")]
        public string DisplayName { get; set; } = string.Empty;

        [DataField("title")]
        public string Title { get; set; } = string.Empty;

        [DataField("hasFullName")]
        public bool HasFullName { get; set; } = false;
    }
}