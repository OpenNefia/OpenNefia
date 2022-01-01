using OpenNefia.Core.Game;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Core.Maps
{
    public sealed class ShadowMap
    {
        private readonly ICoords _coords;

        private IMap _map;
        internal ShadowTile[,] _ShadowTiles;
        internal Vector2i _ShadowPos;
        internal Vector2i _ShadowSize;
        internal UIBox2i _ShadowBounds { get => UIBox2i.FromDimensions(_ShadowPos, _ShadowSize); }

        public ShadowMap(IMap map, ICoords coords)
        {
            _map = map;
            _coords = coords;
            _ShadowTiles = new ShadowTile[map.Width, map.Height];
        }

        private void SetShadowBorder(int tx, int ty, ShadowTile shadow)
        {
            if (tx >= 0 && ty >= 0 && tx < _map.Width && ty < _map.Height)
                _ShadowTiles[tx, ty] |= shadow;
        }

        private void MarkShadow(int tx, int ty)
        {
            SetShadowBorder(tx + 1, ty, ShadowTile.West);
            SetShadowBorder(tx - 1, ty, ShadowTile.East);
            SetShadowBorder(tx, ty - 1, ShadowTile.North);
            SetShadowBorder(tx, ty + 1, ShadowTile.South);
            SetShadowBorder(tx + 1, ty + 1, ShadowTile.Northwest);
            SetShadowBorder(tx - 1, ty - 1, ShadowTile.Southeast);
            SetShadowBorder(tx + 1, ty - 1, ShadowTile.Southwest);
            SetShadowBorder(tx - 1, ty + 1, ShadowTile.Northeast);
        }

        internal void RefreshVisibility()
        {
            Array.Clear(_ShadowTiles, 0, _ShadowTiles.Length);

            var player = GameSession.Player!;
            var playerPos = player.Spatial.MapPosition.Position;

            if (_map.Id != player.Spatial.MapID)
                return;

            _coords.GetWindowTiledSize(out var windowTiledSize);

            var windowTiledW = Math.Min(windowTiledSize.X, _map.Width);
            var windowTiledH = Math.Min(windowTiledSize.Y, _map.Height);

            var start = new Vector2i(Math.Clamp(playerPos.X - windowTiledW / 2 - 2, 0, _map.Width - windowTiledW),
                                    Math.Clamp(playerPos.Y - windowTiledH / 2 - 2, 0, _map.Height - windowTiledH));
            var end = new Vector2i(start.X + windowTiledW + 4, start.Y + windowTiledH + 4);

            _ShadowPos = _coords.TileToScreen(start);
            var shadowEnd = _coords.TileToScreen(end - 1);
            _ShadowSize = shadowEnd - _ShadowPos;

            var fovSize = 15;
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
                    SetShadowBorder(start.X, y, ShadowTile.West);
                }
            }
            if (end.X - 4 < _map.Width - windowTiledW)
            {
                for (int y = start.Y; y < end.Y; y++)
                {
                    SetShadowBorder(end.X - 2, y, ShadowTile.East);
                }
            }
            if (start.Y > 0)
            {
                for (int x = start.X; x < end.X; x++)
                {
                    SetShadowBorder(x, start.Y, ShadowTile.South);
                }
            }
            if (end.Y - 4 < _map.Height)
            {
                for (int x = start.X; x < end.X; x++)
                {
                    SetShadowBorder(x, end.Y - 2, ShadowTile.North);
                }
            }

            for (int j = start.Y; j < end.Y; j++)
            {
                if (j < 0 || j >= _map.Height)
                {
                    for (int i = start.X; i < end.X; i++)
                    {
                        MarkShadow(i, j);
                    }
                }
                else
                {
                    for (int i = start.X; i < end.X; i++)
                    {
                        if (i < 0 || i >= _map.Width)
                        {
                            MarkShadow(i, j);
                        }
                        else
                        {
                            var shadow = true;

                            if (fovYStart <= j && j <= fovYEnd)
                            {
                                if (i >= fovRadius[j + cy, 0] + cx && i < fovRadius[j + cy, 1] + cx)
                                {
                                    var pos = new Vector2i(i, j);
                                    if (_map.HasLineOfSight(player.Spatial.WorldPosition, pos))
                                    {
                                        _map.MemorizeTile(pos);
                                        shadow = false;
                                    }
                                }
                            }

                            if (shadow)
                            {
                                SetShadowBorder(i, j, ShadowTile.IsShadow);
                                MarkShadow(i, j);
                            }
                        }
                    }
                }
            }
        }
    }
}
