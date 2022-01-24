using OpenNefia.Content.Charas;
using OpenNefia.Content.Dialog;
using OpenNefia.Content.Prototypes;
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
        public PrototypeId<DialogPrototype>? DialogID = Protos.Dialog.DialogDefault;

        [DataField]
        public bool CanTalk { get; set; } = false;
    }
}
