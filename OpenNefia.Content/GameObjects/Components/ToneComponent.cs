using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    public class ToneComponent : Component
    {
        public override string Name => "Tone";

        [DataField("id", required: true)]
        public PrototypeId<TonePrototype> ID;
    }
}
