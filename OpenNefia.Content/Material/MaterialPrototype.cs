using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNefia.Content.Material
{
    [Prototype("Elona.Material")]
    public class MaterialPrototype : IPrototype, IHspIds<int>
    {
        [DataField("id", required: true)]
        public string ID { get; } = default!;
        
        /// <inheritdoc/>
        [DataField]
        [NeverPushInheritance]
        public HspIds<int>? HspIds { get; }

        [DataField]
        public int Weight { get; }

        [DataField]
        public int Value { get; }

        [DataField]
        public int HitBonus { get; }

        [DataField]
        public int DamageBonus { get; }

        [DataField]
        public int DV { get; }

        [DataField]
        public int PV { get; }

        [DataField]
        public int DiceY { get; }

        [DataField]
        public Color Color { get; }

        [DataField]
        public bool NoFurniture { get; }
    }
}