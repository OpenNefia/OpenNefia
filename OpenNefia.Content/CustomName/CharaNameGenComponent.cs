using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.CustomName
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class CharaNameGenComponent : Component
    {
        public override string Name => "CharaNameGen";

        [DataField]
        public bool HasRandomName { get; set; } = false;
    }
}