using OpenNefia.Content.VanillaAI;
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

namespace OpenNefia.Content.Rendering.TileDrawLayers
{
    [RegisterTileLayer(renderAfter: new[] { typeof(TileAndChipTileLayer) })]
    public class VanillaAIDebugTileLayer : BaseTileLayer
    {
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly ICoords _coords = default!;
        [Dependency] private readonly IEntityManager _entMan = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;

        private Color ColorLineEnemy = Color.Red;
        private Color ColorLineAlly = Color.Blue;
        private Color ColorLineOther = Color.LimeGreen;

        private IMap? _map;
        private List<DrawEntry> _entries = new();

        public override void SetMap(IMap map)
        {
            _map = map;
        }

        public override void OnThemeSwitched()
        {
        }

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
            if (_map == null)
                return;

            _entries.Clear();

            foreach (var (spatial, ai) in _lookup.EntityQueryInMap<SpatialComponent, VanillaAIComponent>(_map.Id))
            {
                if (_gameSession.IsPlayer(spatial.OwnerUid))
                    continue;

                var screenEntity = spatial.WorldPosition;
                var screenDesired = ai.DesiredMovePosition;

                Vector2i? screenTarget = null;
                if (ai.CurrentTarget != null)
                {
                    var spatialTarget = _entMan.GetComponent<SpatialComponent>(ai.CurrentTarget.Value);
                    screenTarget = spatialTarget.WorldPosition;
                }

                var entry = new DrawEntry(screenEntity, screenDesired, screenTarget);
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

                Love.Graphics.Line((Love.Vector2)GlobalPixelPosition + entityScreenPos, 
                                   desiredOutline[0] + (Love.Vector2)_coords.TileSize / 2);

                DrawTileOutline(desiredOutline, ColorLineAlly);

                if (entry.TargetPos != null)
                {
                    Love.Graphics.SetColor(ColorLineEnemy);
                    var targetOutline = GetTileOutline(entry.TargetPos.Value);

                    Love.Graphics.Line((Love.Vector2)GlobalPixelPosition + entityScreenPos,
                                       targetOutline[0] + (Love.Vector2)_coords.TileSize / 2);

                    DrawTileOutline(targetOutline, ColorLineEnemy);
                }
            }
        }

        private Love.Vector2[] GetTileOutline(Vector2i worldPos)
        {
            var outline = new Love.Vector2[5];

            // world position is the top-left corner of the screen-space tile
            outline[0] = (Love.Vector2)(GlobalPixelPosition + _coords.TileToScreen(worldPos));
            outline[1] = (Love.Vector2)(GlobalPixelPosition + _coords.TileToScreen(worldPos + (1, 0)));
            outline[2] = (Love.Vector2)(GlobalPixelPosition + _coords.TileToScreen(worldPos + (1, 1)));
            outline[3] = (Love.Vector2)(GlobalPixelPosition + _coords.TileToScreen(worldPos + (0, 1)));
            outline[4] = (Love.Vector2)(GlobalPixelPosition + _coords.TileToScreen(worldPos));

            return outline;
        }

        private void DrawTileOutline(Love.Vector2[] outline, Color color)
        {
            Love.Graphics.SetColor(color.Lighten(0.5f).WithAlpha(128));
            Love.Graphics.Polygon(Love.DrawMode.Fill, outline);

            Love.Graphics.SetColor(color);
            Love.Graphics.Polygon(Love.DrawMode.Line, outline);
        }

        private class DrawEntry
        {
            public DrawEntry(Vector2i entityPos, Vector2i desiredPos, Vector2i? targetPos)
            {
                EntityPos = entityPos;
                DesiredPos = desiredPos;
                TargetPos = targetPos;
            }

            public Vector2i EntityPos { get; }
            public Vector2i DesiredPos { get; }
            public Vector2i? TargetPos { get; }
        }
    }
}
