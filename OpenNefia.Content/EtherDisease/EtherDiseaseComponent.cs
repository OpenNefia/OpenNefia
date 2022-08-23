using OpenNefia.Content.GameObjects.Components;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.EtherDisease
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class EtherDiseaseComponent : Component, IComponentRefreshable
    {
        public override string Name => "EtherDisease";

        [DataField]
        public int Corruption { get; set; } = 0;

        /// <summary>
        /// Extra speed to add to ether disease progression.
        /// </summary>
        [DataField]
        public Stat<int> EtherDiseaseExtraSpeed { get; set; } = new(0);

        public void Refresh()
        {
            EtherDiseaseExtraSpeed.Reset();
        }
    }
}