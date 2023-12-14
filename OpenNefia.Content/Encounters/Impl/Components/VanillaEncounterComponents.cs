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
    public sealed class EncounterTypeEnemyComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Encounter)]
    public sealed class EncounterTypeMerchantComponent : Component
    {
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Encounter)]
    public sealed class EncounterTypeAssassinComponent : Component
    {
        [DataField]
        public EntityUid EscortQuestUid { get; set; }
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Encounter)]
    public sealed class EncounterTypeRogueComponent : Component
    {
    }
}
