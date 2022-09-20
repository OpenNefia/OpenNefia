using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Content.Prototypes;

namespace OpenNefia.Content.Portraits
{
    [RegisterComponent]
    public class PortraitComponent : Component
    {
        [DataField]
        public PrototypeId<PortraitPrototype> PortraitID { get; set; } = Protos.Portrait.Default;

        [DataField]
        public bool HasRandomPortrait { get; set; } = false;
    }
}
