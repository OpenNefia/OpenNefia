using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    public class WeightComponent : Component
    {
        public override string Name => "Weight";

        [DataField]
        public int Weight { get; set; } = 0;

        [DataField]
        public int Height { get; set; } = 0;

        [DataField]
        public int Age { get; set; } = 0;
    }
}
