using OpenNefia.Content.Inventory;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core;
using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using static OpenNefia.Core.Prototypes.EntityPrototype;

namespace OpenNefia.Content.Enchantments
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class EnchantmentsComponent : Component, ISerializationHooks
    {
        public const int MaxEnchantments = 15;

        public static readonly ContainerId ContainerIdEnchantments = new("Elona.Enchantments");

        public override string Name => "Enchantments";

        /// <summary>
        /// Contains the enchantment entities on this item.
        /// </summary>
        public Container Container { get; private set; } = default!;

        [DataField]
        public PrototypeId<EgoMajorEnchantmentPrototype>? EgoMajorEnchantment { get; set; }

        [DataField]
        public PrototypeId<EgoMinorEnchantmentPrototype>? EgoMinorEnchantment { get; set; }

        [DataField("initialEnchantments")]
        private List<EnchantmentSpecifer> _initialEnchantments { get; set; } = new();
        public IReadOnlyList<EnchantmentSpecifer> InitialEnchantments => _initialEnchantments;

        /// <summary>
        /// True if random enchantments have been generated on this item. If true, it triples the
        /// item's buffed value and increases its identify difficulty.
        /// </summary>
        [DataField]
        public bool HasRandomEnchantments { get; set; }

        protected override void Initialize()
        {
            base.Initialize();

            Container = ContainerHelpers.EnsureContainer<Container>(Owner, ContainerIdEnchantments);
        }

        bool ISerializationHooks.AfterCompare(object? other)
        {
            if (other is not EnchantmentsComponent otherEnc)
                return false;

            // Don't stack if either container is not empty.
            // TODO: Check via CanMergeWith instead.
            if (Container.ContainedEntities.Count > 0 || otherEnc.Container.ContainedEntities.Count > 0)
            {
                return false;
            }

            return true;
        }
    }

    [DataDefinition]
    public sealed class EnchantmentSpecifer
    {
        public EnchantmentSpecifer() {}

        public EnchantmentSpecifer(PrototypeId<EntityPrototype> protoID, int power, int cursePower, bool randomize, ComponentRegistry components)
        {
            ProtoID = protoID;
            Power = power;
            CursePower = cursePower;
            Randomize = randomize;
            Components = components;
        }

        [DataField(required: true)]
        public PrototypeId<EntityPrototype> ProtoID { get; }

        [DataField(required: true)]
        public int Power { get; }

        [DataField]
        public int CursePower { get; } = 0;

        [DataField]
        public bool Randomize { get; } = false;

        [DataField]
        public ComponentRegistry Components { get; } = new();
    }
}
