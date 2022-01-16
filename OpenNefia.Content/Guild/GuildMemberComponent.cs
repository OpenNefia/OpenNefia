using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Guild
{
    [RegisterComponent]
    public class GuildMemberComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "GuildMember";

        [DataField]
        public PrototypeId<GuildPrototype>? GuildID { get; set; }
    }
}
