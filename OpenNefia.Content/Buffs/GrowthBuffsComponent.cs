using OpenNefia.Content.GameObjects.Components;
using OpenNefia.Content.Skills;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Buffs
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class GrowthBuffsComponent : Component, IComponentRefreshable
    {
        /// <summary>
        /// Current growth buffs, cleared on entity refresh.
        /// </summary>
        [DataField]
        public Dictionary<PrototypeId<SkillPrototype>, int> GrowthBuffs { get; } = new();

        public void Refresh()
        {
            GrowthBuffs.Clear();
        }
    }
}