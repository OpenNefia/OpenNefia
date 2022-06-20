using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Content.Prototypes;

namespace OpenNefia.Content.Charas
{
    [RegisterComponent]
    public class PortraitComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "PortraitComponent";

        [DataField("id")]
        public PrototypeId<PortraitPrototype> PortraitID { get; set; } = Protos.Portrait.Default;
    }
}
