using OpenNefia.Content.Prototypes;
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
        public PrototypeId<SkillPrototype> UsedSkill { get; } = default!;
    }
}