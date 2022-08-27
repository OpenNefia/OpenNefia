using OpenNefia.Content.GameObjects.Components;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Stats;
using System;
using System.Collections.Generic;

namespace OpenNefia.LightenedTravels.FueledLight
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class FueledLightComponent : Component, IComponentRefreshable
    {
        public override string Name => "LightenedTravels.FueledLight";
        
        [DataField(required: true)]
        public Stat<int> MaxFuel { get; set; } = new();

        [DataField(required: true)]
        public Stat<int> LightPower { get; set; } = new();

        [DataField]
        public int FuelRemaining { get; set; }
        
        [DataField]
        public Stat<int> FuelConsumedPerTurn { get; set; } = new(100);

        public void Refresh()
        {
            MaxFuel.Reset();
            LightPower.Reset();
            FuelConsumedPerTurn.Reset();
        }
    }
}