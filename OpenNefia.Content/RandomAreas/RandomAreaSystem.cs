using OpenNefia.Content.Maps;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.RandomAreas
{
    /// <summary>
    /// System for randomly generating areas in a world map. This
    /// logic is part of what makes random Nefias tick. In ON, the
    /// random generation/placement logic has been decoupled from the Nefia
    /// logic, so that a more varied assortment of random areas can be generated.
    /// </summary>
    public class RandomAreaSystem : EntitySystem
    {
        [Dependency] private readonly IEntityLookup _lookup = default!;

        /// <summary>
        /// The number of active random areas that should exist in the world map at any given time.
        /// If the live number drops below this amount, then enough new random areas will be generated
        /// to fill the needed amount.
        /// </summary>
        private const int ActiveRandomAreaThreshold = 25;

        public override void Initialize()
        {
            SubscribeLocalEvent<MapRandomAreaManagerComponent, MapEnteredEvent>(OnMapEntered, nameof(OnMapEntered));
        }

        private void OnMapEntered(EntityUid mapEnt, MapRandomAreaManagerComponent mapRandomAreas, MapEnteredEvent args)
        {
            if (ShouldRegenerateRandomAreas(args.NewMap, mapRandomAreas))
            {
                RegenerateRandomAreas(args.NewMap, mapRandomAreas);
            }
        }

        private bool ShouldRegenerateRandomAreas(IMap newMap, MapRandomAreaManagerComponent mapRandomAreas)
        {
            var totalLiveAreas = GetTotalActiveRandomAreasInMap(newMap);
            return false;
        }

        /// <summary>
        /// Calculates how many world map entrances to random areas in this map
        /// area 
        /// </summary>
        /// <param name="newMap"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private int GetTotalActiveRandomAreasInMap(IMap newMap)
        {
            var ev = new RandomAreaIsActiveCheckEvent();
            foreach (var entrance in EnumerateWorldMapEntrancesIn(newMap.Id))
            {
            }
            return 0;
        }

        private IEnumerable<WorldMapEntranceComponent> EnumerateWorldMapEntrancesIn(MapId mapId)
        {
            return _lookup.EntityQueryInMap<WorldMapEntranceComponent>(mapId);
        }

        private void RegenerateRandomAreas(IMap newMap, MapRandomAreaManagerComponent mapRandomAreas)
        {

            mapRandomAreas.NeedsRegeneration = false;
        }
    }

    /// <summary>
    /// This event is used to see if a random area is "dead" and can be cleaned up
    /// by the random area manager. Examples include Nefias that have been conquered
    /// by the player.
    /// </summary>
    public sealed class RandomAreaIsActiveCheckEvent : EntityEventArgs
    {
        /// <summary>
        /// If true, this area shouldn't be cleaned up by the random area manager.
        /// </summary>
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Fired when the current map is being regenerated according to world map rules.
    /// This is used to regenerate global area entrances and to regenerate random areas
    /// like Nefia.
    /// </summary>
    public sealed class WorldMapRegeneratingEvent : EntityEventArgs
    {
    }
}
