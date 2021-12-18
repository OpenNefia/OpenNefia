using OpenNefia.Core.Effects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    public class PotionPuddleComponent : Component
    {
        public override string Name => "PotionPuddle";

        [DataField]
        public EffectArgs Args = new();

        [DataField]
        public IEffect? Effect;
    }
}
