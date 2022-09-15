using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Talk
{
    [RegisterComponent]
    public class ToneComponent : Component
    {
        public override string Name => "Tone";

        [DataField]
        public PrototypeId<TonePrototype> ToneID { get; set; }

        [DataField]
        public bool IsTalkSilenced { get; set; }
    }
}
