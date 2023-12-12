using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Encounters
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public sealed class MapEncounterComponent : Component
    {
        public static readonly ContainerId ContainerIdMapEncounter = new("Elona.MapEncounter");

        /// <summary>
        /// Container that holds the activity.
        /// </summary>
        public ContainerSlot EncounterContainer { get; private set; } = default!;

        protected override void Initialize()
        {
            base.Initialize();

            EncounterContainer = ContainerHelpers.EnsureContainer<ContainerSlot>(Owner, ContainerIdMapEncounter);
        }
    }
}