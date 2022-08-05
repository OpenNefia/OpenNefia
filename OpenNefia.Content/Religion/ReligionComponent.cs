using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Religion
{
    [RegisterComponent]
    public class ReligionComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "Religion";

        /// <summary>
        /// ID of the following god. Null means Eyth.
        /// </summary>
        [DataField]
        public PrototypeId<GodPrototype>? GodID { get; set; }
   
        [DataField]
        public int Piety { get; set; }
        
        [DataField]
        public int GodRank { get; set; }

        [DataField]
        public int PrayerCharge { get; set; }

        [DataField]
        public bool HasRandomGod { get; set; } = true;
    }
}
