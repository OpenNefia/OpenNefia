using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.GlobalEntities
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class GlobalEntitiesComponent : Component
    {
        public static readonly ContainerId ContainerIdStayers = new("Elona.GlobalEntities");

        /// <summary>
        /// Holds the staying character entities.
        /// </summary>
        public Container Container { get; private set; } = default!;

        protected override void Initialize()
        {
            base.Initialize();
            Container = ContainerHelpers.EnsureContainer<Container>(Owner, ContainerIdStayers);
        }
    }
}