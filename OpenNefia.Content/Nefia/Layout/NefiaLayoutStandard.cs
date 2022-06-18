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
    /// Generates rooms connected by tunnels (similar to the original Rogue).
    /// </summary>
    public class NefiaLayoutStandard : IVanillaNefiaLayout
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly INefiaLayoutCommon _nefiaLayout = default!;

        public IMap? Generate(IArea area, MapId mapId, int generationAttempt, int floorNumber, Blackboard<NefiaGenParams> data)
        {
            var baseParams = data.Get<BaseNefiaGenParams>();
            var map = _nefiaLayout.CreateMap(mapId, baseParams);

            var rooms = _entityManager.EnsureComponent<NefiaRoomsComponent>(map.MapEntityUid).Rooms;

            if (!_nefiaLayout.TryDigRoomIfBelowMax(map, rooms, RoomType.NonEdge, baseParams.MinRoomSize, baseParams.MaxRoomSize, out var upstairsRoom))
            {
                Logger.ErrorS("nefia.gen.floor", "Could not dig room for upstairs");
                return null;
            }

            _nefiaLayout.PlaceStairsSurfacingInRoom(map, upstairsRoom.Value);

            if (!_nefiaLayout.TryDigRoomIfBelowMax(map, rooms, RoomType.NonEdge, baseParams.MinRoomSize, baseParams.MaxRoomSize, out var downstairsRoom))
            {
                Logger.ErrorS("nefia.gen.floor", "Could not dig room for downstairs");
                return null;
            }

            _nefiaLayout.PlaceStairsDelvingInRoom(map, downstairsRoom.Value);

            for (int i = 0; i < baseParams.RoomCount; i++)
            {
                _nefiaLayout.TryDigRoomIfBelowMax(map, rooms, RoomType.NonEdge, baseParams.MinRoomSize, baseParams.MaxRoomSize, out _);
            }

            if (!_nefiaLayout.TryConnectRooms(map, rooms, true, baseParams))
            {
                Logger.ErrorS("nefia.gen.floor", $"Could not connect {rooms.Count} rooms");
                return null;
            }

            return map;
        }
    }
}