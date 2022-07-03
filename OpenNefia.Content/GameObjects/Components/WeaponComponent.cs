using OpenNefia.Content.Skills;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    public class WeaponComponent : Component
    {
        public override string Name => "Weapon";

        [DataField(required: true)]
        public PrototypeId<SkillPrototype> WeaponSkill { get; } = default!;

        [DataField]
        public int DiceX { get; set; }

        [DataField]
        public int DiceY { get; set; }

        /// <summary>
        /// 0-100.
        /// </summary>
        [DataField]
        public int PierceRate { get; set; }
    }
}