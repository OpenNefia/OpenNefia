using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Linq;
using System.Text;

namespace OpenNefia.Content.Scene
{
    [Prototype("Elona.Scene")]
    public class ScenePrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; } = default!;

        [DataField]
        public PrototypeId<LanguagePrototype> FallbackLanguage { get; } = LanguagePrototypeOf.English;

        [DataField(required: true)]
        public string Filename  { get; } = string.Empty;

        [DataField(required: true)]
        public ResourcePath Location { get; } = ResourcePath.Root;
    }
}