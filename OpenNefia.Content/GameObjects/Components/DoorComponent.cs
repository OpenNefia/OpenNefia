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

        [ComponentDependency] private ChipComponent? _chip = null;
        [ComponentDependency] private SpatialComponent? _spatial = null;

        [DataField]
        public PrototypeId<ChipPrototype> ChipOpen { get; } = Protos.Chip.FeatDoorWoodenOpen;

        [DataField]
        public PrototypeId<ChipPrototype> ChipClosed { get; } = Protos.Chip.FeatDoorWoodenClosed;

        [DataField]
        public PrototypeId<SoundPrototype>? SoundOpen { get; }

        [DataField]
        private bool _isOpen = false;
        public bool IsOpen
        {
            get => _isOpen;
            set
            {
                _isOpen = value;

                if (_spatial != null)
                {
                    _spatial.IsSolid = !_isOpen;
                    _spatial.IsOpaque = !_isOpen;
                }

                if (_chip != null)
                {
                    _chip.ChipID = _isOpen ? ChipOpen : ChipClosed;
                }
            }
        }
    }
}
