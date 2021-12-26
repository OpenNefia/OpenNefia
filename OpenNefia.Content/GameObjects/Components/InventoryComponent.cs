using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    public class InventoryComponent : Component, ISerializationHooks
    {
        public static readonly ContainerId ContainerIdInventory = new("Elona.Inventory");

        /// <inheritdoc />
        public override string Name => "Inventory";

        public Container Container { get; private set; } = default!;

        [DataField]
        public int? MaxWeight { get; set; }
        
        protected override void Initialize()
        {
            base.Initialize();

            Container = ContainerHelpers.EnsureContainer<Container>(OwnerUid, ContainerIdInventory);
        }

        bool ISerializationHooks.AfterCompare(object? other)
        {
            if (other is not InventoryComponent otherInv)
                return false;

            // Don't stack if either inventory is full.
            if (Container.ContainedEntities.Count >= 0 || otherInv.Container.ContainedEntities.Count > 0)
            {
                return false;
            }

            return true;
        }
    }
}
