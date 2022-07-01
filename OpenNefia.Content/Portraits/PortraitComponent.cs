using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Content.Prototypes;

namespace OpenNefia.Content.Portraits
{
    [RegisterComponent]
    public class PortraitComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "Portrait";

        [DataField("id")]
        public PrototypeId<PortraitPrototype> PortraitID { get; set; } = Protos.Portrait.Default;

        [DataField]
        public bool HasRandomPortrait { get; set; } = false;
    }
}
