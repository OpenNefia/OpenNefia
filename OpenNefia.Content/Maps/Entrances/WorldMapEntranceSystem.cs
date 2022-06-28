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
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly MapEntranceSystem _mapEntrances = default!;

        public override void Initialize()
        {
            SubscribeComponent<WorldMapEntranceComponent, GetVerbsEventArgs>(HandleGetVerbs);
            SubscribeBroadcast<ExecuteVerbEventArgs>(HandleExecuteVerb);
            SubscribeComponent<WorldMapEntranceComponent, UseWorldMapEntranceEvent>(HandleUseWorldMapEntrance);
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
            SpatialComponent? spatial = null)
        {
            if (!Resolve(entrance, ref worldMapEntrance, ref spatial))
                return TurnResult.Failed;

            var prevCoords = spatial.MapPosition;

            _sounds.Play(Protos.Sound.Exitmap1);

            if (_mapEntrances.UseMapEntrance(user, worldMapEntrance.Entrance, out var mapId))
            {
                _mapEntrances.SetPreviousMap(mapId.Value, prevCoords);
                return TurnResult.Succeeded;
            }

            return TurnResult.Failed;
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
