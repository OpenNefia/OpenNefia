using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Content.Prototypes;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    public class DoorComponent : Component
    {
        public override string Name => "Door";

        [DataField]
        public PrototypeId<ChipPrototype> ChipOpen { get; } = Protos.Chip.FeatDoorWoodenOpen;

        [DataField]
        public PrototypeId<ChipPrototype> ChipClosed { get; } = Protos.Chip.FeatDoorWoodenClosed;

        [DataField]
        public PrototypeId<SoundPrototype> SoundOpen { get; } = Protos.Sound.Door1;
    }
}
