using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;

namespace OpenNefia.Content.GameObjects
{
    [RegisterComponent]
    public class InventoryComponent : Component
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


    }
}
