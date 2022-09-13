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

namespace OpenNefia.Content.Scenarios
{
    /// <summary>
    /// Defines a set of start/win conditions, selected during character creation. In previous
    /// variants of Elona there was only ever one scenario with one or more acts. This system will
    /// allow for greater customization of game progression.
    /// </summary>
    [Prototype("Elona.Scenario")]
    public class ScenarioPrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; } = default!;
    }
}