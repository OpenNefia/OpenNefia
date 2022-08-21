using OpenNefia.Content.Skills;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    public class WeaponComponent : Component
    {
        public override string Name => "Weapon";

        [DataField(required: true)]
        public PrototypeId<SkillPrototype> WeaponSkill { get; } = default!;

        [DataField]
        public Stat<int> DiceX { get; set; } = new(0);

        [DataField]
        public Stat<int> DiceY { get; set; } = new(0);

        /// <summary>
        /// 0-100.
        /// </summary>
        [DataField]
        public Stat<int> PierceRate { get; set; } = new(0);
    }
}