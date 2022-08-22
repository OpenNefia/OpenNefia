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

        /// <summary>
        /// Locale ID for indexing into <c>Elona.Enchantment.Ego.Major</c>.
        /// </summary>
        [DataField]
        public LocaleKey? EgoMajorEnchantment { get; set; }

        /// <summary>
        /// Locale ID for indexing into <c>Elona.Enchantment.Ego.Minor</c>.
        /// </summary>
        [DataField]
        public LocaleKey? EgoMinorEnchantment { get; set; }

        [DataField("initialEnchantments")]
        private List<EnchantmentSpecifer> _initialEnchantments { get; set; } = new();
        public IReadOnlyList<EnchantmentSpecifer> InitialEnchantments => _initialEnchantments;

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
