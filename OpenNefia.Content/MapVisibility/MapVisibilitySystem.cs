using OpenNefia.Content.Visibility;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using System.Runtime.CompilerServices;

namespace OpenNefia.Content.MapVisibility
{
    public sealed class MapVisibilitySystem : EntitySystem
    {
        [Core.IoC.Dependency] private readonly ICoords _coords = default!;
        [Core.IoC.Dependency] private readonly IGraphics _graphics = default!;
        [Core.IoC.Dependency] private readonly IGameSessionManager _gameSession = default!;

        public override void Initialize()
        {
            SubscribeBroadcast<MapCreatedEvent>(HandleMapCreated, priority: EventPriorities.Highest);
            SubscribeBroadcast<MapLoadedFromSaveEvent>(HandleMapLoadedFromSave, priority: EventPriorities.Highest);
            SubscribeComponent<MapVisibilityComponent, RefreshMapVisibilityEvent>(RefreshVisibility, priority: EventPriorities.High);
        }

        private void HandleMapCreated(MapCreatedEvent ev)
        {
            InitializeShadowMap(ev.Map);
        }

        private void HandleMapLoadedFromSave(MapLoadedFromSaveEvent ev)
        {
            InitializeShadowMap(ev.Map);
        }

        private void InitializeShadowMap(IMap map)
        {
            var mapVis = EntityManager.EnsureComponent<MapVisibilityComponent>(map.MapEntityUid);

            EntitySystem.InjectDependencies(mapVis.ShadowMap);
            mapVis.ShadowMap.Initialize(map);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private void SetShadowBorder(IMap map, ShadowMap shadows, Vector2i pos, ShadowTile shadow)
        {
            if (map.IsInBounds(pos))
                shadows.ShadowTiles[pos.X, pos.Y] |= shadow;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        private void MarkShadow(IMap map, ShadowMap shadows, Vector2i pos)
        {
            SetShadowBorder(map, shadows, pos + (1, 0), ShadowTile.West);
            SetShadowBorder(map, shadows, pos + (-1, 0), ShadowTile.East);
            SetShadowBorder(map, shadows, pos + (0, -1), ShadowTile.North);
            SetShadowBorder(map, shadows, pos + (0, 1), ShadowTile.South);
            SetShadowBorder(map, shadows, pos + (1, 1), ShadowTile.Northwest);
            SetShadowBorder(map, shadows, pos + (-1, -1), ShadowTile.Southeast);
            SetShadowBorder(map, shadows, pos + (1, -1), ShadowTile.Southwest);
            SetShadowBorder(map, shadows, pos + (-1, 1), ShadowTile.Northeast);
        }

        private void RefreshVisibility(EntityUid uid, MapVisibilityComponent mapVis, ref RefreshMapVisibilityEvent args)
        {
            var map = args.Map;
            var shadows = mapVis.ShadowMap;

            Array.Clear(shadows.ShadowTiles, (int)ShadowTile.None, shadows.ShadowTiles.Length);

            var player = _gameSession.Player;
            var playerSpatial = EntityManager.GetComponent<SpatialComponent>(player);
            var playerPos = playerSpatial.MapPosition.Position;

            if (map.Id != playerSpatial.MapID)
                return;

            var windowTiledSize = _coords.GetTiledSize(_graphics.WindowPixelSize);

            var windowTiledW = Math.Min(windowTiledSize.X, map.Width);
            var windowTiledH = Math.Min(windowTiledSize.Y, map.Height);

            var start = new Vector2i(Math.Clamp(playerPos.X - windowTiledW / 2 - 2, 0, map.Width - windowTiledW),
                                     Math.Clamp(playerPos.Y - windowTiledH / 2 - 2, 0, map.Height - windowTiledH));
            var end = new Vector2i(start.X + windowTiledW + 4, start.Y + windowTiledH + 4);

            shadows.ShadowPos = _coords.TileToScreen(start);
            var shadowEnd = _coords.TileToScreen(end - 1);
            shadows.ShadowSize = shadowEnd - shadows.ShadowPos;

            var fovSize = 15;
            if (TryComp<VisibilityComponent>(uid, out var vis))
                fovSize = vis.FieldOfViewRadius.Buffed;

            var fovRadius = FovRadius.Get(fovSize);
            var radius = fovSize / 2 + 1;

            var fovYStart = playerPos.Y - fovSize / 2;
            var fovYEnd = playerPos.Y + fovSize / 2;

            var cx = playerPos.X - radius;
            var cy = radius - playerPos.Y;

            if (start.X > 0)
            {
                for (int y = start.Y; y < end.Y; y++)
                {
                    SetShadowBorder(map, shadows, new(start.X, y), ShadowTile.West);
                }
            }
            if (end.X - 4 < map.Width - windowTiledW)
            {
                for (int y = start.Y; y < end.Y; y++)
                {
                    SetShadowBorder(map, shadows, new(end.X - 2, y), ShadowTile.East);
                }
            }
            if (start.Y > 0)
            {
                for (int x = start.X; x < end.X; x++)
                {
                    SetShadowBorder(map, shadows, new(x, start.Y), ShadowTile.South);
                }
            }
            if (end.Y - 4 < map.Height)
            {
                for (int x = start.X; x < end.X; x++)
                {
                    SetShadowBorder(map, shadows, new(x, end.Y - 2), ShadowTile.North);
                }
            }

            for (int j = start.Y; j < end.Y; j++)
            {

                if (j < 0 || j >= map.Height)
                {
                    for (int i = start.X; i < end.X; i++)
                    {
                        var pos = new Vector2i(i, j);
                        MarkShadow(map, shadows, pos);
                    }
                }
                else
                {
                    for (int i = start.X; i < end.X; i++)
                    {
                        var pos = new Vector2i(i, j);
                        if (i < 0 || i >= map.Width)
                        {
                            MarkShadow(map, shadows, pos);
                        }
                        else
                        {
                            var shadow = true;

                            if (fovYStart <= j && j <= fovYEnd)
                            {
                                if (i >= fovRadius[j + cy, 0] + cx && i < fovRadius[j + cy, 1] + cx)
                                {
                                    if (map.HasLineOfSight(playerSpatial.WorldPosition, pos))
                                    {
                                        map.MemorizeTile(pos);
                                        shadow = false;
                                    }
                                }
                            }

                            if (shadow)
                            {
                                SetShadowBorder(map, shadows, pos, ShadowTile.IsShadow);
                                MarkShadow(map, shadows, pos);
                            }
                        }
                    }
                }
            }
        }
    }
}
