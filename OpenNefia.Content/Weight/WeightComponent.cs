using OpenNefia.Content.GameObjects.Components;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;

namespace OpenNefia.Content.Weight
{
    [RegisterComponent]
    public sealed class WeightComponent : Component, IComponentRefreshable
    {
        /// <inheritdoc/>
        [DataField]
        public Stat<int> Weight { get; set; } = new(0);

        [DataField]
        public int Height { get; set; } = 0;

        [DataField]
        public int Age { get; set; } = 0;

        public void Refresh()
        {
            Weight.Reset();
        }
    }
}
