using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Maps;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Utility;
using OpenNefia.Core.Log;
using OpenNefia.Core.Game;
using OpenNefia.Core.SaveGames;
using OpenNefia.Content.GameObjects;

namespace OpenNefia.Content.Maps
{
    public class MapTransferSystem : EntitySystem
    {
        [Dependency] private readonly IMapLoader _mapLoader = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly MapEntranceSystem _mapEntrances = default!;
        [Dependency] private readonly IAudioSystem _sounds = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly ISaveGameManager _saveGameManager = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<PlayerComponent, ExitMapEventArgs>(HandleExitMap, nameof(HandleExitMap));
            SubscribeLocalEvent<PlayerComponent, EntityParentChangedEvent>(HandleEntityParentChanged, nameof(HandleEntityParentChanged));
        }

        private void HandleExitMap(EntityUid playerUid, PlayerComponent component, ExitMapEventArgs args)
        {
            SpatialComponent? spatial = null;
            
            if (!Resolve(playerUid, ref spatial))
                return;

            _sounds.Play(Protos.Sound.Exitmap1);

            args.Handle(_mapEntrances.UseMapEntrance(playerUid, args.Entrance));
        }

        private void HandleEntityParentChanged(EntityUid uid, PlayerComponent component, ref EntityParentChangedEvent evt)
        {
            SpatialComponent? spatial = null;

            if (!Resolve(uid, ref spatial))
                return;

            if (spatial.MapID == _mapManager.ActiveMap?.Id)
                return;

            // TODO: dunno if the potential for an expensive map load on property setting
            // is desirable...
            if (_gameSession.IsPlayer(uid) && spatial.MapID != MapId.Nullspace)
            {
                DoMapTransfer(uid, spatial);
            }
        }

        private void RunMapInitializeEvents()
        {
            // TODO
        }

        private void DoMapTransfer(EntityUid player, SpatialComponent spatial)
        {
            var oldMap = _mapManager.ActiveMap;

            _mapManager.SetActiveMap(spatial.MapID);

            RunMapInitializeEvents();

            // TODO move allies over and do other things before the old map gets unloaded.

            if (oldMap != null)
            {
                var save = _saveGameManager.CurrentSave!;
                _mapLoader.SaveMap(oldMap.Id, save);
                _mapManager.UnloadMap(oldMap.Id);
            }
        }
    }
}
