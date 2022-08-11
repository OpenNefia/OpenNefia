using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Items.Impl
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class MusicDiscComponent : Component
    {
        public override string Name => "MusicDisc";

        [DataField]
        public PrototypeId<MusicPrototype> MusicID { get; set; }
    }
}