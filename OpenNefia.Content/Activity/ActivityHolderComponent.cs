using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Activity
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class ActivityHolderComponent : Component
    {
        public static readonly ContainerId ContainerIdActivityHolder = new("Elona.ActivityHolder");
        
        public override string Name => "ActivityHolder";

        /// <summary>
        /// Container that holds the activity.
        /// </summary>
        public ContainerSlot Container { get; private set; } = default!;

        protected override void Initialize()
        {
            base.Initialize();

            Container = ContainerHelpers.EnsureContainer<ContainerSlot>(Owner, ContainerIdActivityHolder);
        }
    }
}