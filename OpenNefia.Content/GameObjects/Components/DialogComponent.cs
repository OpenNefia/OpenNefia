using OpenNefia.Content.Charas;
using OpenNefia.Content.Dialog;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    public class DialogComponent : Component
    {
        public override string Name => "Dialog";

        [DataField]
        public PrototypeId<DialogItemPrototype>? DialogID;

        [DataField]
        public bool CanTalk { get; set; } = false;

        [DataField]
        public PrototypeId<PortraitPrototype>? Portrait { get; set; }
    }
}
