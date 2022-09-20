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

        [DataField]
        public PrototypeId<ChipPrototype> ChipOpen { get; set; } = Protos.Chip.MObjDoorWoodenOpen;

        [DataField]
        public PrototypeId<ChipPrototype> ChipClosed { get; set; } = Protos.Chip.MObjDoorWoodenClosed;

        [DataField]
        public SoundSpecifier? SoundOpen { get; set; }

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
