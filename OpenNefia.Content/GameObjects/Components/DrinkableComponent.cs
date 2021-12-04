using OpenNefia.Core.Effects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    public class DrinkableComponent : Component
    {
        public override string Name => "Drinkable";

        [DataField]
        public EffectArgs Args = new();

        [DataField(required: true)]
        public IEffect Effect = default!;
    }
}
