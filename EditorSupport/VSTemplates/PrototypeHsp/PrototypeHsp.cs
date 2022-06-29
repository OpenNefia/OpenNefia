using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Random;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace $rootnamespace$
{
    [Prototype("Elona.$safeitemrootname$")]
    public class $safeitemrootname$Prototype : IPrototype
    {
        [DataField("id", required: true)]
        public string ID { get; } = default!;
    }
}