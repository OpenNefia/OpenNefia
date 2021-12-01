using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.GameObjects.EntitySystems
{
    [RegisterComponent]
    public class WeaponComponent : Component
    {
        public override string Name => "Weapon";

        [DataField(required: true)]
        public PrototypeId<SkillPrototype> UsedSkill { get; } = default!;
    }
}