using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Maps;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Utility;
using OpenNefia.Core.Log;

namespace OpenNefia.Content.GameObjects
{
    public class MapTransferSystem : EntitySystem
    {
        [Dependency] private readonly IMapBlueprintLoader _mapBlueprints = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly MapEntranceSystem _mapEntrances = default!;
        [Dependency] private readonly IAudioSystem _sounds = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<PlayerComponent, ExitMapEventArgs>(HandleExitMap, nameof(HandleExitMap));
            SubscribeLocalEvent<PlayerComponent, EntParentChangedEvent>(HandleEntityParentChanged, nameof(HandleEntityParentChanged));
        }

        private void HandleExitMap(EntityUid playerUid, PlayerComponent component, ExitMapEventArgs args)
        {
            SpatialComponent? spatial = null;
            
            if (!Resolve(playerUid, ref spatial))
                return;

            _sounds.Play(Protos.Sound.Exitmap1);

            args.Handle(_mapEntrances.UseMapEntrance(playerUid, args.Entrance));
        }

        private void HandleEntityParentChanged(EntityUid uid, PlayerComponent component, ref EntParentChangedEvent evt)
        {
            SpatialComponent? spatial = null;

            if (!Resolve(uid, ref spatial))
                return;

            if (spatial.MapID == _mapManager.ActiveMap?.Id)
                return;

            if (!_mapManager.TryGetMap(spatial.MapID, out var map))
            {
                Logger.WarningS("map", "Could not find map to set active map to!");
                return;
            }

            _mapManager.SetActiveMap(spatial.MapID);
        }
    }
}
