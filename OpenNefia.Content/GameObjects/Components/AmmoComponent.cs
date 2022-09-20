using OpenNefia.Content.GameObjects.Components;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class AmmoComponent : Component, IComponentRefreshable
    {
        [DataField]
        public PrototypeId<SkillPrototype> AmmoSkill { get; set; }

        [DataField]
        public Stat<int> DiceX { get; set; } = new(0);

        [DataField]
        public Stat<int> DiceY { get; set; } = new(0);

        [DataField]
        public EntityUid? ActiveAmmoEnchantment { get; set; }

        public void Refresh()
        {
            DiceX.Reset();
            DiceY.Reset();
        }
    }
}