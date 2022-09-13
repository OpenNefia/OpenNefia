using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Linq;
using System.Text;

namespace OpenNefia.Content.Enchantments
{
    [Prototype("Elona.EgoMinorEnchantment")]
    public class EgoMinorEnchantmentPrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; } = default!;
    }
}