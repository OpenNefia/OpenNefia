﻿using Love;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using Color = OpenNefia.Core.Maths.Color;

namespace OpenNefia.Core.Rendering
{
    public enum BatchCentering
    {
        None,
        Centered,
        AlignBottom
    }

    public class TileAtlasBatch : IDisposable
    {
        private ICoords _coords;

        private string _atlasName { get; }
        private TileAtlas _atlas { get; set; }
        private SpriteBatch _batch { get; set; }
        public int BatchPixelWidth { get; private set; }
        public int BatchPixelHeight { get; private set; }

        public TileAtlasBatch(string atlasName)
        {
            _coords = IoCManager.Resolve<ICoords>();
            _atlasName = atlasName;
            _atlas = IoCManager.Resolve<ITileAtlasManager>().GetAtlas(atlasName);
            _batch = Love.Graphics.NewSpriteBatch(_atlas.Image, 2048, SpriteBatchUsage.Dynamic);
            BatchPixelWidth = 0;
            BatchPixelHeight = 0;
        }

        public void OnThemeSwitched()
        {
            _atlas = IoCManager.Resolve<ITileAtlasManager>().GetAtlas(_atlasName);
            _batch = Love.Graphics.NewSpriteBatch(_atlas.Image, 2048, SpriteBatchUsage.Dynamic);
        }

        public void Add(float uiScale, string tileId, float x, float y, float? width = null, float? height = null, Love.Color? color = null, BatchCentering centering = BatchCentering.None, float rotationRads = 0f)
        {
            if (!_atlas.TryGetTile(tileId, out var tile))
            {
                Logger.ErrorS("tile.batch", $"Unknown tile {tileId} in atlas {_atlasName}");
                return;
            }

            x *= uiScale;
            y *= uiScale;

            var quadRect = tile.Quad.GetViewport();
            var sx = uiScale;
            var sy = uiScale;

            if (color != null)
            {
                var c = color.Value;
                _batch.SetColor(c.Rf, c.Gf, c.Bf, c.Af);
            }
            else
            {
                _batch.SetColor(1f, 1f, 1f, 1f);
            }

            var ttw = quadRect.Width;
            var tth = quadRect.Height;

            if (width != null)
            {
                width *= uiScale;
                sx = (float)width.Value / ttw;
                ttw = width.Value;
            }
            if (height != null)
            {
                height *= uiScale;
                sy = (float)height.Value / tth;
                tth = height.Value;
            }

            var ox = 0f;
            var oy = 0f;
            switch (centering)
            {
                case BatchCentering.Centered:
                    ox = ((float)ttw) / 4;
                    oy = ((float)tth) / 4;
                    break;
                case BatchCentering.AlignBottom:
                    oy = tth - _coords.TileSize.Y;
                    break;
            }

            _batch.Add(tile.Quad, x, y, rotationRads, sx, sy, ox, oy);

            BatchPixelWidth = Math.Max(BatchPixelWidth, (int)(x + ttw));
            BatchPixelHeight = Math.Max(BatchPixelHeight, (int)(y + tth));
        }

        public void Flush() => _batch.Flush();

        public Vector2i GetTileSize(TileSpecifier spec) => _atlas.GetTileSize(spec);
        public Vector2i GetTileSize(string tileId) => _atlas.GetTileSize(tileId);

        public void Clear()
        {
            _batch.Clear();
            BatchPixelWidth = 0;
            BatchPixelHeight = 0;
        }

        public void Draw(float uiScale, float x, float y, float? width = null, float? height = null, Color? color = null)
        {
            Love.Graphics.SetColor(color ?? Color.White);
            GraphicsEx.DrawSpriteBatchS(uiScale, _batch, x, y, width, height);
        }

        public void Dispose()
        {
            _batch.Dispose();
        }
    }
}
