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

namespace OpenNefia.Content.Fishing
{
    [Prototype("Elona.Fish")]
    public class FishPrototype : IPrototype, IHspIds<int>
    {
        [IdDataField]
        public string ID { get; } = default!;

        /// <inheritdoc/>
        [DataField]
        [NeverPushInheritance]
        public HspIds<int>? HspIds { get; }

        [DataField]
        public int Level { get; set; }

        [DataField]
        public int Rarity { get; set; }

        [DataField]
        public int Power { get; set; }

        [DataField]
        public int Speed { get; set; }

        [DataField]
        public int Weight { get; set; }

        [DataField]
        public int Value { get; set; }

        [DataField]
        public PrototypeId<EntityPrototype> ItemID { get; set; } = Protos.Item.Fish;
    }
}