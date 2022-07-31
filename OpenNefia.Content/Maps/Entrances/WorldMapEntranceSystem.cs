using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Log;
using OpenNefia.Content.GameObjects;

namespace OpenNefia.Content.Maps
{
    public class WorldMapEntranceSystem : EntitySystem
    {
        [Dependency] private readonly IAudioManager _sounds = default!;
        [Dependency] private readonly MapEntranceSystem _mapEntrances = default!;

        public override void Initialize()
        {
            SubscribeComponent<WorldMapEntranceComponent, GetVerbsEventArgs>(HandleGetVerbs);
        }

        private void HandleGetVerbs(EntityUid uid, WorldMapEntranceComponent component, GetVerbsEventArgs args)
        {
            args.Verbs.Add(new Verb(StairsSystem.VerbTypeActivate, "Enter Area", () => UseWorldMapEntrance(args.Target, args.Source)));
        }

        private TurnResult UseWorldMapEntrance(EntityUid entrance, EntityUid user,
            WorldMapEntranceComponent? worldMapEntrance = null,
            SpatialComponent? spatial = null)
        {
            if (!Resolve(entrance, ref worldMapEntrance, ref spatial))
                return TurnResult.Failed;

            var prevCoords = spatial.MapPosition;

            if (_mapEntrances.UseMapEntrance(user, worldMapEntrance.Entrance, out var mapId))
            {
                _mapEntrances.SetPreviousMap(mapId.Value, prevCoords);
                return TurnResult.Succeeded;
            }

            return TurnResult.Aborted;
        }
    }
}
