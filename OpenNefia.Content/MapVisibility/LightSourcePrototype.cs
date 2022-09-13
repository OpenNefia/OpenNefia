using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Linq;
using System.Text;

namespace OpenNefia.Content.MapVisibility
{
    [Prototype("Elona.LightSource")]
    public class LightSourcePrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; } = default!;

        [DataField]
        public PrototypeId<AssetPrototype> AssetID { get; } = Protos.Asset.LightLantern;

        [DataField]
        public int Brightness { get; }

        [DataField]
        public Vector2i Offset { get; }

        [DataField]
        public int Power { get; }

        [DataField]
        public int Flicker { get; }

        [DataField]
        public bool AlwaysOn { get; } = true;
    }
}