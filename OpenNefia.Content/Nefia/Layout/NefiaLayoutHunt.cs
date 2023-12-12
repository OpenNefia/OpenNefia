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
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Pickable;

namespace OpenNefia.Content.Nefia
{
    /// <summary>
    /// Map layout used in hunting quests.
    /// </summary>
    public class NefiaLayoutHunt : IVanillaNefiaLayout
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly ICharaGen _charaGen = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly INefiaLayoutCommon _nefiaLayout = default!;
        [Dependency] private readonly IRandom _rand = default!;

        public IMap? Generate(IArea area, MapId mapId, int generationAttempt, int floorNumber, Blackboard<NefiaGenParams> data)
        {
            var baseParams = data.Get<BaseNefiaGenParams>();
            baseParams.MaxCharaCount = 0;

            var map = _nefiaLayout.CreateMap(mapId, baseParams);
            map.Clear(Protos.Tile.MapgenRoom);

            var common = _entityManager.EnsureComponent<MapCommonComponent>(map.MapEntityUid);
            common.IsIndoors = false;

            var wallCount = _rand.Next(map.Width * map.Height / 30);
            for (var i = 0; i < wallCount; i++)
            {
                var pos = _rand.NextVec2iInBounds(map.Bounds);
                map.SetTile(pos, Protos.Tile.MapgenWall);
            }

            return map;
        }

        void IVanillaNefiaLayout.AfterGenerateMap(IArea area, IMap map, int floorNumber, Blackboard<NefiaGenParams> data)
        {
            var count = 10 + _rand.Next(6);
            for (var i = 0; i < count; i++)
            {
                var chara = _charaGen.GenerateCharaFromMapFilter(map);
                if (chara != null && _entityManager.TryGetComponent<FactionComponent>(chara.Value, out var faction))
                {
                    faction.RelationToPlayer = Relation.Enemy;
                }
            }

            count = 10 + _rand.Next(6);
            for (var i = 0; i < count; i++)
            {
                var item = _itemGen.GenerateItem(map, tags: new[] { Protos.Tag.ItemCatTree });
                if (item != null && _entityManager.TryGetComponent<PickableComponent>(item.Value, out var pickable))
                {
                    pickable.OwnState = OwnState.None;
                }
            }
        }
    }
}