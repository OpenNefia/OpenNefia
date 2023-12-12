using OpenNefia.Content.Inventory;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Items
{
    /// <summary>
    /// A trunk is defined as an item that can hold other items.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class TrunkComponent : Component, ISerializationHooks
    {
        public static readonly ContainerId ContainerIdInventory = new("Elona.Trunk");

        public Container Container { get; private set; } = default!;

        protected override void Initialize()
        {
            base.Initialize();

            Container = ContainerHelpers.EnsureContainer<Container>(Owner, ContainerIdInventory);
        }

        bool ISerializationHooks.AfterCompare(object? other)
        {
            if (other is not InventoryComponent otherInv)
                return false;

            // Don't stack if either inventory is not empty.
            if (Container.ContainedEntities.Count > 0 || otherInv.Container.ContainedEntities.Count > 0)
            {
                return false;
            }

            return true;
        }
    }
}