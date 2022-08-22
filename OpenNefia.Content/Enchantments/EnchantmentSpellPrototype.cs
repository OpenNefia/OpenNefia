using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Spells;
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

namespace OpenNefia.Content.Enchantments
{
    [Prototype("Elona.EnchantmentSpell")]
    public class EnchantmentSpellPrototype : IPrototype, IHspIds<int>
    {
        [DataField("id", required: true)]
        public string ID { get; } = default!;

        /// <inheritdoc/>
        [DataField]
        [NeverPushInheritance]
        public HspIds<int>? HspIds { get; }

        [DataField(required: true)]
        public PrototypeId<SpellPrototype> SpellID { get; }

        [DataField("validItemCategories")]
        private List<PrototypeId<TagPrototype>>? _validItemCategories { get; }
        public IReadOnlyList<PrototypeId<TagPrototype>>? ValidItemCategories => _validItemCategories;

        [DataField]
        public int RandomWeight { get; }
    }
}