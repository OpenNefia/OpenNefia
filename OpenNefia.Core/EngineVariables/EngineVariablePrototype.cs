using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Serialization;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Value;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Linq;
using System.Text;

namespace OpenNefia.Core.EngineVariables
{
    /// <summary>
    /// Defines a new engine variable.
    /// </summary>
    [Prototype("EngineVariable")]
    public class EngineVariablePrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; } = default!;

        [DataField("default", required: true)]
        public AnyDataNode Default { get; } = new();
    }
}