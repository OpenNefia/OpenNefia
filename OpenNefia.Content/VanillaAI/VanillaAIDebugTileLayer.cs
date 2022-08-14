using OpenNefia.Content.Visibility;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Rendering.TileDrawLayers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.VanillaAI
{
    [RegisterTileLayer(renderAfter: new[] { typeof(TileAndChipTileLayer) }, enabledAtStartup: false)]
    public class VanillaAIDebugTileLayer : BaseTileLayer
    {
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly ICoords _coords = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IVisibilitySystem _visibility = default!;

        private Color ColorLineEnemy = Color.Red;
        private Color ColorLineAlly = Color.Blue;
        private Color ColorLineOther = Color.LimeGreen;
        private Color ColorLineAnchor = Color.Yellow;

        private List<DrawEntry> _entries = new();

        public override void RedrawAll()
        {
            UpdateEntries();
        }

        public override void RedrawDirtyTiles(HashSet<Vector2i> dirtyTilesThisTurn)
        {
            UpdateEntries();
        }

        private void UpdateEntries()
        {
            if (Map == null)
                return;

            _entries.Clear();

            foreach (var (spatial, ai) in _lookup.EntityQueryInMap<SpatialComponent, VanillaAIComponent>(Map.Id))
            {
                if (!EntityManager.IsAlive(spatial.Owner)
                    || _gameSession.IsPlayer(spatial.Owner)
                    || !_visibility.CanSeeEntity(_gameSession.Player, spatial.Owner))
                    continue;

                var tileEntity = spatial.WorldPosition;
                var tileDesired = ai.DestinationCoords;

                Vector2i? tileTarget = null;
                if (EntityManager.IsAlive(ai.CurrentTarget)
                    && EntityManager.TryGetComponent(ai.CurrentTarget.Value, out SpatialComponent targetSpatial))
                {
                    var spatialTarget = EntityManager.GetComponent<SpatialComponent>(ai.CurrentTarget.Value);
                    tileTarget = spatialTarget.WorldPosition;
                }

                Vector2i? tileAnchor = null;
                if (EntityManager.TryGetComponent<AIAnchorComponent>(spatial.Owner, out var anchor))
                {
                    tileAnchor = anchor.Anchor;
                }

                var entry = new DrawEntry(tileEntity, tileDesired, tileTarget, tileAnchor);
                _entries.Add(entry);
            }
        }

        public override void Update(float dt)
        {
        }

        public override void Draw()
        {
            foreach (var entry in _entries)
            {
                Love.Graphics.SetColor(ColorLineOther);

                var entityScreenPos = (Love.Vector2)(_coords.TileToScreen(entry.EntityPos) + _coords.TileSize / 2);
                var desiredOutline = GetTileOutline(entry.DesiredPos);

                Love.Graphics.Line((Love.Vector2)PixelPosition + entityScreenPos,
                                   desiredOutline[0] + (Love.Vector2)_coords.TileSize / 2);

                DrawTileOutline(desiredOutline, ColorLineAlly);

                if (entry.TargetPos != null)
                {
                    Love.Graphics.SetColor(ColorLineEnemy);
                    var targetOutline = GetTileOutline(entry.TargetPos.Value);

                    Love.Graphics.Line((Love.Vector2)PixelPosition + entityScreenPos,
                                       targetOutline[0] + (Love.Vector2)_coords.TileSize / 2);

                    DrawTileOutline(targetOutline, ColorLineEnemy);
                }

                if (entry.AnchorPos != null)
                {
                    Love.Graphics.SetColor(ColorLineAnchor);
                    var targetOutline = GetTileOutline(entry.AnchorPos.Value);

                    Love.Graphics.Line((Love.Vector2)PixelPosition + entityScreenPos,
                                       targetOutline[0] + (Love.Vector2)_coords.TileSize / 2);

                    DrawTileOutline(targetOutline, ColorLineAnchor);
                }
            }
        }

        private Love.Vector2[] GetTileOutline(Vector2i worldPos)
        {
            var outline = new Love.Vector2[5];

            // world position is the top-left corner of the screen-space tile
            outline[0] = (Love.Vector2)(PixelPosition + _coords.TileToScreen(worldPos));
            outline[1] = (Love.Vector2)(PixelPosition + _coords.TileToScreen(worldPos + (1, 0)));
            outline[2] = (Love.Vector2)(PixelPosition + _coords.TileToScreen(worldPos + (1, 1)));
            outline[3] = (Love.Vector2)(PixelPosition + _coords.TileToScreen(worldPos + (0, 1)));
            outline[4] = (Love.Vector2)(PixelPosition + _coords.TileToScreen(worldPos));

            return outline;
        }

        private void DrawTileOutline(Love.Vector2[] outline, Color color)
        {
            Love.Graphics.SetColor(color.Lighten(0.5f).WithAlphaB(60));
            Love.Graphics.Polygon(Love.DrawMode.Fill, outline);
        }

        private class DrawEntry
        {
            public DrawEntry(Vector2i entityPos, Vector2i desiredPos, Vector2i? targetPos, Vector2i? anchorPos)
            {
                EntityPos = entityPos;
                DesiredPos = desiredPos;
                TargetPos = targetPos;
                AnchorPos = anchorPos;
            }

            public Vector2i EntityPos { get; }
            public Vector2i DesiredPos { get; }
            public Vector2i? TargetPos { get; }
            public Vector2i? AnchorPos { get; }
        }
    }
}
