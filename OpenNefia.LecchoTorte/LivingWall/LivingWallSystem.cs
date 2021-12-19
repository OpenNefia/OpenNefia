using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Prototypes;

namespace OpenNefia.LecchoTorte.LivingWall
{
    /// <summary>
    /// Also titled: the walls have ears, eyes, and a loving family
    /// </summary>
    public class LivingWallSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<LivingWallComponent, EntityPositionChangedEvent>(HandlePositionChanged, nameof(HandlePositionChanged));
            SubscribeLocalEvent<LivingWallComponent, MapInitEvent>(HandleMapInit, nameof(HandleMapInit));
            SubscribeLocalEvent<LivingWallComponent, EntityLivenessChangedEvent>(HandleLivenessChanged, nameof(HandleLivenessChanged));
        }

        private void HandleMapInit(EntityUid uid, LivingWallComponent component, ref MapInitEvent args)
        {
            SpatialComponent? spatial = null;

            if (!Resolve(uid, ref spatial))
                return;

            UpdateLivingWall(uid, spatial.MapPosition, spatial.MapPosition, component, spatial);
        }

        private void HandleLivenessChanged(EntityUid uid, LivingWallComponent component, ref EntityLivenessChangedEvent args)
        {
            SpatialComponent? spatial = null;

            if (!Resolve(uid, ref spatial))
                return;

            UpdateLivingWall(uid, spatial.MapPosition, spatial.MapPosition, component, spatial);
        }

        private void HandlePositionChanged(EntityUid uid, LivingWallComponent component, ref EntityPositionChangedEvent args)
        {
            var oldCoords = args.OldPosition.ToMap(EntityManager);
            var newCoords = args.NewPosition.ToMap(EntityManager);
            UpdateLivingWall(uid, oldCoords, newCoords, component);
        }

        private void UpdateLivingWall(EntityUid uid, 
            MapCoordinates oldCoords,
            MapCoordinates newCoords,
            LivingWallComponent livingWall,
            SpatialComponent? spatial = null,
            MetaDataComponent? metaData = null)
        {
            if (!Resolve(uid, ref spatial, ref metaData))
                return;

            if (_mapManager.TryGetMap(oldCoords.MapId, out var oldMap))
            {
                if (livingWall.TileStandingOn != null)
                {
                    oldMap.SetTile(oldCoords.Position, livingWall.TileStandingOn.Value);
                    livingWall.TileStandingOn = null;
                }
            }

            if (_mapManager.TryGetMap(newCoords.MapId, out var newMap))
            {
                if (metaData.IsAlive)
                {
                    var newTile = newMap.GetTile(newCoords.Position).ResolvePrototype().GetStrongID();
                    if (newTile != Protos.Tile.Empty)
                    {
                        livingWall.TileStandingOn = newTile;
                        newMap.SetTile(newCoords.Position, livingWall.TileID);
                    }
                }
            }
        }
    }
}
