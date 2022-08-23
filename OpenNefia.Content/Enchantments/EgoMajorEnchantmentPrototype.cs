using OpenNefia.Content.Combat;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;

namespace OpenNefia.Content.Enchantments
{
    [Prototype("Elona.EgoMajorEnchantment")]
    public class EgoMajorEnchantmentPrototype : IPrototype
    {
        [DataField("id", required: true)]
        public string ID { get; } = default!;

        [DataField(required: true)]
        public int Level { get; set; }

        [DataField]
        public IEgoMajorEnchantmentFilter? Filter { get; }

        [DataField("enchantments")]
        private List<EnchantmentSpecifer> _enchantments { get; } = new();
        public IReadOnlyList<EnchantmentSpecifer> Enchantments => _enchantments;
    }

    [ImplicitDataDefinitionForInheritors]
    public interface IEgoMajorEnchantmentFilter
    {
        bool CanApplyTo(EntityUid item);
    }

    public sealed class EgoMajorArmorFilter : IEgoMajorEnchantmentFilter
    {
        [Dependency] private readonly IEntityManager _entities = default!;

        public bool CanApplyTo(EntityUid item)
        {
            return _entities.HasComponent<ArmorComponent>(item);
        }
    }

    public sealed class EgoMajorAccessoryFilter : IEgoMajorEnchantmentFilter
    {
        [Dependency] private readonly IEntityManager _entities = default!;

        public bool CanApplyTo(EntityUid item)
        {
            return _entities.HasComponent<AccessoryComponent>(item);
        }
    }

    public sealed class EgoMajorGeneralFilter : IEgoMajorEnchantmentFilter
    {
        [Dependency] private readonly IEntityManager _entities = default!;

        public bool CanApplyTo(EntityUid item)
        {
            return _entities.HasComponent<EquipmentComponent>(item);
        }
    }

    public sealed class EgoMajorItemCategoryFilter : IEgoMajorEnchantmentFilter
    {
        [Dependency] private readonly ITagSystem _tags = default!;

        [DataField("itemCategories", required: true)]
        private HashSet<PrototypeId<TagPrototype>> _itemCategories { get; } = new();
        public IReadOnlySet<PrototypeId<TagPrototype>> ItemCategories => _itemCategories;

        public bool CanApplyTo(EntityUid item)
        {
            return ItemCategories.Any(ic => _tags.HasTag(item, ic));
        }
    }
}