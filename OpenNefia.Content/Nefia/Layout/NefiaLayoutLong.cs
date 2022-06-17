using OpenNefia.Content.Levels;
using OpenNefia.Content.Maps;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Directions;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Nefia.Layout;
using OpenNefia.Content.Factions;
using OpenNefia.Content.GameObjects.Pickable;

namespace OpenNefia.Content.Nefia
{
    /// <summary>
    /// Long vertical tunnel.
    /// </summary>
    public class NefiaLayoutLong : IVanillaNefiaLayout
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IEntityGen _entityGen = default!;
        [Dependency] private readonly INefiaLayoutCommon _nefiaLayout = default!;
        [Dependency] private readonly IRandom _rand = default!;

        public IMap? Generate(IArea area, MapId mapId, int generationAttempt, int floorNumber, Blackboard<NefiaGenParams> data)
        {
            var baseParams = data.Get<BaseNefiaGenParams>();
            baseParams.MapSize = (30, 60 + _rand.Next(60));

            var map = _nefiaLayout.CreateMap(mapId, baseParams);
            baseParams.MaxCharaCount = map.Width * map.Height / 20;

            var tunnelWidth = 6;
            var dx = (map.Width / 2) - (tunnelWidth / 2);
            var variance = 0;

            for (var i = 0; i < map.Height - 4; i++)
            {
                var y = i + 2;
                for (var j = 0; j < tunnelWidth; j++)
                {
                    var x = j + dx;
                    map.SetTile((x, y), Protos.Tile.MapgenTunnel);
                }

                if (variance <= 0 && _rand.OneIn(2))
                {
                    variance = _rand.Next(12);
                }
                if (_rand.OneIn(2) && tunnelWidth > 4)
                {
                    tunnelWidth -= _rand.Next(2);
                }
                if (variance > 0)
                {
                    if (variance < 5 && tunnelWidth > 3)
                    {
                        tunnelWidth -= _rand.Next(2);
                        variance--;
                    }
                    if (variance > 4 && tunnelWidth < 10)
                    {
                        tunnelWidth++;
                        variance--;
                    }
                }
                if (dx > 1)
                {
                    dx -= _rand.Next(2);
                }
                if (dx + tunnelWidth < map.Width - 1)
                {
                    dx += _rand.Next(2);
                }
                if (dx + tunnelWidth > map.Width)
                {
                    tunnelWidth = map.Width - dx;
                }
            }

            // Place surfacing stairs
            while (true)
            {
                var pos = _rand.NextVec2iInVec(map.Width, Math.Min(15, map.Height));
                if (map.GetTile(pos)!.Value.Tile.GetStrongID() == Protos.Tile.MapgenTunnel)
                {
                    _nefiaLayout.PlaceStairsSurfacing(map.AtPos(pos));
                    break;
                }
            }

            // Place delving stairs
            while (true)
            {
                var pos = _rand.NextVec2iInVec(map.Width, Math.Min(map.Height - _rand.Next(15) - 1, map.Height));
                if (map.GetTile(pos)!.Value.Tile.GetStrongID() == Protos.Tile.MapgenTunnel)
                {
                    _nefiaLayout.PlaceStairsDelving(map.AtPos(pos));
                    break;
                }
            }

            return map;
        }

        void IVanillaNefiaLayout.AfterGenerateMap(IArea area, IMap map, int floorNumber, Blackboard<NefiaGenParams> data)
        {
            // >>>>>>>> shade2/map_rand.hsp:233 	if rdType=rdLong{	 ...
            var modifier = _entityManager.EnsureComponent<NefiaCrowdDensityModifierComponent>(map.MapEntityUid);
            modifier.Modifier = new SimpleCrowdDensityModifier(4, 10);
            // <<<<<<<< shade2/map_rand.hsp:236 		} ..
        }
    }
}