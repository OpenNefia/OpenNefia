using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.LecchoTorte.InfiniteSpawner
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Normal)]
    public sealed class InfiniteSpawnedComponent : Component
    {

        [DataField]
        public EntityUid Spawner { get; set; }
    }
}