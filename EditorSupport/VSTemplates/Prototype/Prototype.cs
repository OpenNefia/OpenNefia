using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Random;
using OpenNefia.Content.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Content.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace $rootnamespace$
{
    [Prototype(!"Elona.$safeitemrootname$")]
    public class $safeitemrootname$ : IPrototype
    {
        [IdDataField]
        public string ID { get; } = default!;
    }
}