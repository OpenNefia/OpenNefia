using OpenNefia.Content.GameObjects.Components;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;

namespace OpenNefia.Content.Combat
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class NullifyDamageComponent : Component, IComponentRefreshable
    {
        [DataField]
        public Stat<float> NullifyDamageChance { get; set; } = new(0f);

        public void Refresh()
        {
            NullifyDamageChance.Reset();
        }
    }
}