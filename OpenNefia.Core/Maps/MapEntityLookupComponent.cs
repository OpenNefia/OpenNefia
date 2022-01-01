using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;

namespace OpenNefia.Core.Maps
{
    [RegisterComponent]
    public class MapEntityLookupComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "MapEntityLookup";

        /// <summary>
        /// Spatial lookup of entities on this map.
        /// </summary>
        public List<EntityUid>[,] EntitySpatial = new List<EntityUid>[0,0];

        internal void InitializeFromMap(IMap map)
        {
            EntitySpatial = new List<EntityUid>[map.Width, map.Height];

            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    EntitySpatial[x, y] = new List<EntityUid>();
                }
            }
        }
    }
}
