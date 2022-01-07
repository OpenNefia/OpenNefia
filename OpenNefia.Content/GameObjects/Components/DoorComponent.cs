using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Audio;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    public class DoorComponent : Component, IFromHspFeat
    {
        public override string Name => "Door";

        [ComponentDependency] private ChipComponent? _chip = null;
        [ComponentDependency] private SpatialComponent? _spatial = null;

        [DataField]
        public PrototypeId<ChipPrototype> ChipOpen { get; } = Protos.Chip.FeatDoorWoodenOpen;

        [DataField]
        public PrototypeId<ChipPrototype> ChipClosed { get; } = Protos.Chip.FeatDoorWoodenClosed;

        [DataField]
        public SoundSpecifier? SoundOpen { get; }

        [DataField]
        public int UnlockDifficulty { get; set; } = 0;

        [DataField]
        public bool IsOpen { get; set; }

        public void FromHspFeat(int cellObjId, int param1, int param2)
        {
            UnlockDifficulty = param1;
        }
    }
}
