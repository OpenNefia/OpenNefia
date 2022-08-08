﻿using OpenNefia.Content.Roles;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Shopkeeper
{
    /// <hspVariant>elona122</hspVariant>
    /// <hspId>1</hspId>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public class RoleShopkeeperComponent : Component, IRoleComponent
    {
        public static readonly ContainerId ContainerIdShopInventory = new("Elona.ShopInventory");

        /// <inheritdoc />
        public override string Name => "RoleShopkeeper";

        [DataField(required: true)]
        public PrototypeId<ShopInventoryPrototype> InventoryId { get; set; } = default!;

        [DataField]
        public int ShopRank { get; set; } = 1;

        public Container ShopContainer { get; private set; } = default!;

        protected override void Initialize()
        {
            base.Initialize();

            ShopContainer = ContainerHelpers.EnsureContainer<Container>(Owner, ContainerIdShopInventory);
        }
    }
}
