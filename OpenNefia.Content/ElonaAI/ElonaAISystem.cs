using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Maps;
using OpenNefia.Content.TurnOrder;
using OpenNefia.Core.Directions;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.ElonaAI
{
    /// <summary>
    /// Handles Elona's vanilla AI. The idea is that this can be replaced with whatever
    /// AI system you want by simply removing the <see cref="ElonaAIComponent"/> on the 
    /// entity prototype and writing another entity system that handles <see cref="NPCTurnStartedEvent"/>.
    /// </summary>
    public class ElonaAISystem : EntitySystem
    {
        [Dependency] private readonly IMapRandom _mapRandom = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly MoveableSystem _movement = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<ElonaAIComponent, NPCTurnStartedEvent>(HandleNPCTurnStarted, nameof(HandleNPCTurnStarted));
        }

        private void HandleNPCTurnStarted(EntityUid uid, ElonaAIComponent ai, ref NPCTurnStartedEvent args)
        {
            args.Handle(RunElonaAI(uid, ai));
        }

        public TurnResult RunElonaAI(EntityUid uid, ElonaAIComponent? ai = null,
            SpatialComponent? spatial = null)
        {
            if (!Resolve(uid, ref ai, ref spatial))
                return TurnResult.Failed;

            if (!_mapManager.TryGetMap(spatial.MapID, out var map))
                return TurnResult.Failed;

            foreach (var tile in _mapRandom.GetRandomAdjacentTiles(spatial.MapPosition))
            {
                if (map.CanAccess(tile.Position))
                {
                    var result = _movement.MoveEntity(uid, tile.MapPosition, spatial: spatial);
                    if (result != null)
                    {
                        return result.Value;
                    }
                }
            }

            return TurnResult.Failed;
        }
    }
}
