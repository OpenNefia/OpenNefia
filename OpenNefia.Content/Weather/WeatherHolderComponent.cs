using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Weather
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class WeatherHolderComponent : Component
    {
        public static readonly ContainerId ContainerIdWeatherHolder = new("Elona.WeatherHolder");

        /// <summary>
        /// Holds the active weather, if any.
        /// </summary>
        public ContainerSlot Container { get; private set; } = default!;

        protected override void Initialize()
        {
            base.Initialize();
            Container = ContainerHelpers.EnsureContainer<ContainerSlot>(Owner, ContainerIdWeatherHolder);
        }
    }
}