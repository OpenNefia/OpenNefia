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
using OpenNefia.Content.Pickable;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Qualities;

namespace OpenNefia.Content.Nefia
{
    /// <summary>
    /// Maze with corridors of width 4 (Minotaur's Nest).
    /// </summary>
    public class NefiaLayoutMaze : IVanillaNefiaLayout
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly INefiaLayoutCommon _nefiaLayout = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;

        public IMap? Generate(IArea area, MapId mapId, int generationAttempt, int floorNumber, Blackboard<NefiaGenParams> data)
        {
            var klass = 12;
            var bold = 2;

            var baseParams = data.Get<BaseNefiaGenParams>();
            baseParams.MapSize = (klass * (bold * 2) - bold + 8, baseParams.MapSize.X);
            baseParams.MaxCharaCount = baseParams.MapSize.X * baseParams.MapSize.Y / 12;

            var map = _nefiaLayout.CreateMap(mapId, baseParams);

            var rooms = _entityManager.EnsureComponent<NefiaRoomsComponent>(map.MapEntityUid).Rooms;

            _nefiaLayout.DigMaze(map, rooms, data, klass, bold);

            if (!_nefiaLayout.PlaceStairsInMaze(map))
                return null;

            _nefiaLayout.DigMaze(map, rooms, data, klass, bold);

            return map;
        }

        void IVanillaNefiaLayout.AfterGenerateMap(IArea area, IMap map, int floorNumber, Blackboard<NefiaGenParams> data)
        {
            // >>>>>>>> shade2/map_rand.hsp:243 	if rdType=rdMaze{	 ...
            var modifier = _entityManager.EnsureComponent<NefiaCrowdDensityModifierComponent>(map.MapEntityUid);
            modifier.Modifier = new SimpleCrowdDensityModifier(3, 10);
            // <<<<<<<< shade2/map_rand.hsp:246 		} ..

            // TODO
            _itemGen.GenerateItem(map, tags: new[] { _randomGen.PickTag(Protos.TagSet.ItemWear) }, quality: Quality.Unique);
        }
    }
}