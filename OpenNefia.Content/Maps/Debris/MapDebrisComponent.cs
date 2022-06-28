using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Maps
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public sealed class MapDebrisComponent : Component
    {
        public override string Name => "MapDebris";

        [DataField]
        public MapDebris[,] DebrisState { get; set; } = new MapDebris[0, 0];

        [DataField]
        public MapDebris[,] DebrisMemory { get; set; } = new MapDebris[0, 0];

        public void Initialize(IMap map)
        {
            DebrisState = new MapDebris[map.Width, map.Height];
            DebrisMemory = new MapDebris[map.Width, map.Height];
        }
    }
    
    [DataDefinition]
    public struct MapDebris
    {
        [DataField]
        public int Blood { get; set; }

        [DataField]
        public int Fragments { get; set; }
    }
}