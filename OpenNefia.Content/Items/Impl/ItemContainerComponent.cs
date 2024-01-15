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
    /// An item that can hold other items and has a max weight/capacity.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class ItemContainerComponent : Component, ISerializationHooks
    {
        public static readonly ContainerId ContainerIdInventory = new("Elona.Trunk");

        public Container Container { get; private set; } = default!;

        /// <summary>
        /// Maximum total weight of items in the container.
        /// Past this weight, further insertion will fail.
        /// Contrast with <see cref="InventoryComponent.MaxWeight"/>,
        /// which applies a burden state instead of limiting insertion.
        /// </summary>
        [DataField]
        public int? MaxTotalWeight { get; set; }

        /// <summary>
        /// Maximum weight of a single item in the container.
        /// </summary>
        [DataField]
        public int? MaxItemWeight { get; set; }

        /// <summary>
        /// Maximum item count this entity can hold. Null means "unlimited".
        /// Past this count, further insertion will fail.
        /// </summary>
        [DataField]
        public int? MaxItemCount { get; set; }

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

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class ItemContainerNoCargoComponent : Component
    {
        [DataField]
        public bool AllowCargo { get; set; } = false;
    }
}