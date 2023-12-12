using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Encounters
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Encounter)]
    public sealed class EncounterEnemyComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Encounter)]
    public sealed class EncounterMerchantComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Encounter)]
    public sealed class EncounterAssassinComponent : Component
    {
        [DataField]
        public EntityUid EscortQuestUid { get; set; }

        [DataField]
        public bool AllEnemiesDefeated { get; set; } = false;
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Encounter)]
    public sealed class EncounterRogueComponent : Component
    {
    }
}
