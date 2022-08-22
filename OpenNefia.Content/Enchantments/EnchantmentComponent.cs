using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Items;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.UI;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using OpenNefia.Core.IoC;
using OpenNefia.Content.RandomText;

namespace OpenNefia.Content.Enchantments
{
    /// <summary>
    /// Indicates that this entity is an enchantment, to be stored in an <see
    /// cref="EnchantmentsComponent"/>'s container.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Enchantment)]
    public sealed class EnchantmentComponent : Component
    {
        public override string Name => "Enchantment";

        /// <summary>
        /// Generic power level of this enchantment. The effects based on power should vary
        /// depending on what other enchantment components are on the entity.
        /// </summary>
        [DataField]
        public int TotalPower { get; set; }

        [DataField]
        public int Level { get; set; }

        [DataField]
        public int RandomWeight { get; set; }

        [DataField]
        public float ValueModifier { get; set; } = 1f;

        [DataField]
        public ItemDescriptionIcon Icon { get; set; } = ItemDescriptionIcon.Boots;

        [DataField]
        public EnchantmentAlignmentType AlignmentType { get; set; } = EnchantmentAlignmentType.BaseOnPower;

        [DataField]
        public Color? Color { get; set; }

        [DataField]
        public bool IsInheritable { get; set; }

        /// <summary>
        /// Power contributions of enchantments merged into this enchantment.
        /// </summary>
        [DataField]
        public List<EnchantmentPowerContrib> PowerContributions { get; set; } = new();

        [DataField]
        public HashSet<PrototypeId<TagPrototype>>? ValidItemCategories { get; set; }

        /// <summary>
        /// Number of "sub enchantments" that make up this enchantment, for purposes of item value
        /// calculation.
        /// </summary>
        public int SubEnchantmentCount => PowerContributions.DistinctBy(e => e.Source).Count();
    }

    public enum EnchantmentAlignmentType
    {
        BaseOnPower,
        AlwaysPositive,
        AlwaysNegative,
    }

    public enum EnchantmentAlignment
    {
        Positive,
        Negative
    }

    [DataDefinition]
    public sealed class EnchantmentPowerContrib
    {
        public EnchantmentPowerContrib() {}

        public EnchantmentPowerContrib(int power, string source)
        {
            Power = power;
            Source = source;
        }

        [DataField]
        public int Power { get; set; }

        [DataField(required: true)]
        public string Source { get; set; } = EnchantmentSources.Generated;
    }

    /// <summary>
    /// Indicates where this enchantment originated from.
    /// </summary>
    public static class EnchantmentSources
    {
        /// <summary>
        /// Randomly generated during item generation/afterwards. This is the default for new
        /// enchantments.
        /// </summary>
        public const string Generated = "Generated";

        /// <summary>
        /// Contributed by the item's <see cref="EnchantmentsComponent.InitialEnchantments"/>.
        /// </summary>
        public const string EntityPrototype = "EntityPrototype";

        /// <summary>
        /// Contributed by the item's material.
        /// </summary>
        public const string Material = "Material";
    }
}