using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.God
{
    [RegisterComponent]
    public class GodFollowerComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "GodFollower";

        [DataField]
        public PrototypeId<GodPrototype>? GodID { get; set; }
    }
}
