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

namespace OpenNefia.Content.Nefia
{
    /// <summary>
    /// One large room spanning the entire map.
    /// </summary>
    public class NefiaLayoutBigRoom : IVanillaNefiaLayout
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly INefiaLayoutCommon _nefiaLayout = default!;
        [Dependency] private readonly IRandom _rand = default!;

        public IMap? Generate(IArea area, MapId mapId, int generationAttempt, int floorNumber, Blackboard<NefiaGenParams> data)
        {
            var baseParams = data.Get<BaseNefiaGenParams>();
            baseParams.MapSize = (48 + _rand.Next(20), 22);

            var map = _nefiaLayout.CreateMap(mapId, baseParams);
            baseParams.MaxCharaCount = map.Width * map.Height / 20;

            foreach (var tile in map.AllTiles)
            {
                var pos = tile.Position;
                var isBorder = pos.X == 0 || pos.Y == 0 || pos.X == map.Width - 1 || pos.Y == map.Height - 1;
                if (!isBorder)
                {
                    map.SetTile(pos, Protos.Tile.MapgenRoom);
                }
            }

            var surfacingStairsPos = (_rand.Next(map.Width / 2) + 2, _rand.Next(map.Height - 4) + 2);
            var delvingStairsPos = (_rand.Next(map.Width / 2) + (map.Width / 2) - 2, _rand.Next(map.Height - 4) + 2);

            if (_rand.OneIn(2))
            {
                // Swap the X positions.
                var temp = surfacingStairsPos;
                surfacingStairsPos = delvingStairsPos;
                delvingStairsPos = temp;
            }

            _nefiaLayout.PlaceStairsSurfacing(map.AtPos(surfacingStairsPos));
            _nefiaLayout.PlaceStairsDelving(map.AtPos(delvingStairsPos));

            return map;
        }

        void IVanillaNefiaLayout.AfterGenerateMap(IArea area, IMap map, int floorNumber, Blackboard<NefiaGenParams> data)
        {
            // >>>>>>>> shade2/map_rand.hsp:227 	if rdType=rdBigRoom{	 ...
            var modifier = _entityManager.EnsureComponent<NefiaCrowdDensityModifierComponent>(map.MapEntityUid);
            modifier.Modifier = new SimpleCrowdDensityModifier(2, 3);
            // <<<<<<<< shade2/map_rand.hsp:231 		} ..
        }
    }
}