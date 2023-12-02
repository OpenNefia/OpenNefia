using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.RandomGen
{
    /// <summary>
    /// Overrides the random generation tables for entities in a specific area with its own.
    /// </summary>
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class AreaRandomGenTablesComponent : Component
    {
        [DataField]
        public Dictionary<string, Dictionary<PrototypeId<EntityPrototype>, RandomGenTable>> Tables = new();
    }
}