using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Maps;

namespace OpenNefia.Content.GameObjects
{
    public class WorldMapEntranceSystem : EntitySystem
    {
        [Dependency] private readonly IAudioSystem _sounds = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly MapEntranceSystem _mapEntrances = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<WorldMapEntranceComponent, GetVerbsEventArgs>(HandleGetVerbs, nameof(HandleGetVerbs));
            SubscribeLocalEvent<ExecuteVerbEventArgs>(HandleExecuteVerb, nameof(HandleExecuteVerb));
            SubscribeLocalEvent<WorldMapEntranceComponent, UseWorldMapEntranceEvent>(HandleUseWorldMapEntrance, nameof(HandleUseWorldMapEntrance));
        }

        private void HandleGetVerbs(EntityUid uid, WorldMapEntranceComponent component, GetVerbsEventArgs args)
        {
            args.Verbs.Add(new Verb(StairsSystem.VerbIDActivate));
        }

        private void HandleExecuteVerb(ExecuteVerbEventArgs args)
        {
            if (args.Handled)
                return;

            switch (args.Verb.ID)
            {
                case StairsSystem.VerbIDActivate:
                    Raise(args.Target, new UseWorldMapEntranceEvent(args.Source), args);
                    break;
            }
        }

        private void HandleUseWorldMapEntrance(EntityUid entrance, WorldMapEntranceComponent worldMapEntrance, UseWorldMapEntranceEvent args)
        {
            args.Handle(UseWorldMapEntrance(entrance, args.User, worldMapEntrance));
        }

        private TurnResult UseWorldMapEntrance(EntityUid entrance, EntityUid user,
            WorldMapEntranceComponent? worldMapEntrance = null,
            MapEntranceComponent? mapEntrance = null,
            SpatialComponent? spatial = null)
        {
            if (!Resolve(entrance, ref worldMapEntrance, ref mapEntrance, ref spatial))
                return TurnResult.Failed;

            var prevCoords = spatial.MapPosition;

            _sounds.Play(Protos.Sound.Exitmap1);

            var turnResult = _mapEntrances.UseMapEntrance(entrance, user, mapEntrance);

            // Set the map to travel to when exiting the destination map via the edges.
            var mapId = mapEntrance.Entrance.DestinationMapId;
            if (mapId != null)
            {
                if (_mapManager.TryGetMapEntity(mapId.Value, out var mapEntity))
                {
                    var mapMapEntrance = EntityManager.EnsureComponent<MapEntranceComponent>(mapEntity.Uid);
                    mapMapEntrance.Entrance.DestinationMapId = prevCoords.MapId;
                    mapMapEntrance.Entrance.StartLocation = new SpecificMapLocation(prevCoords.Position);
                }
            }

            return turnResult;
        }
    }

    public class UseWorldMapEntranceEvent : TurnResultEntityEventArgs
    {
        public readonly EntityUid User;

        public UseWorldMapEntranceEvent(EntityUid user)
        {
            User = user;
        }
    }
}
