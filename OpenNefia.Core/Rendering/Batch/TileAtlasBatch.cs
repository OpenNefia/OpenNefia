using Love;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    public class TileAtlasBatch : IDisposable
    {
        private string _atlasName { get; }
        private TileAtlas _atlas { get; set; }
        private SpriteBatch _batch { get; set; }
        public int PixelWidth { get; private set; }
        public int PixelHeight { get; private set; }

        public TileAtlasBatch(string atlasName)
        {
            _atlasName = atlasName;
            _atlas = IoCManager.Resolve<ITileAtlasManager>().GetAtlas(atlasName);
            _batch = Love.Graphics.NewSpriteBatch(_atlas.Image, 2048, SpriteBatchUsage.Dynamic);
            PixelWidth = 0;
            PixelHeight = 0;
        }

        public void OnThemeSwitched()
        {
            _atlas = IoCManager.Resolve<ITileAtlasManager>().GetAtlas(_atlasName);
            _batch = Love.Graphics.NewSpriteBatch(_atlas.Image, 2048, SpriteBatchUsage.Dynamic);
        }

        public void Add(float uiScale, string tileId, float x, float y, float? width = null, float? height = null, Love.Color? color = null, bool centered = false, float rotation = 0f)
        {
            if (!this._atlas.TryGetTile(tileId, out var tile))
            {
                Logger.ErrorS("tile.batch", $"Unknown tile {tileId} in atlas {_atlasName}");
                return;
            }

            x *= uiScale;
            y *= uiScale;

            var quadRect = tile.Quad.GetViewport();
            var sx = 1f * uiScale;
            var sy = 1f * uiScale;

            if (color != null)
            {
                var c = color.Value;
                this._batch.SetColor(c.Rf, c.Gf, c.Bf, c.Af);
            }
            else
            {
                this._batch.SetColor(1f, 1f, 1f, 1f);
            }

            var ttw = (int)quadRect.Width;
            var tth = (int)quadRect.Height;

            if (width != null)
            {
                width *= uiScale;
                sx = (float)width.Value / ttw;
                ttw = (int)width.Value;
            }
            if (height != null)
            {
                height *= uiScale;
                sy = (float)height.Value / tth;
                tth = (int)height.Value;
            }

            var ox = 0f;
            var oy = 0f;
            if (centered)
            {
                ox = (float)ttw / 2;
                oy = (float)tth / 2;
            }

            this._batch.Add(tile.Quad, x, y, MathUtil.DegreesToRadians(rotation), sx, sy, ox, oy);

            PixelWidth = Math.Max(PixelWidth, (int)x + ttw);
            PixelHeight = Math.Max(PixelHeight, (int)y + tth);
        }

        public void Flush() => this._batch.Flush();

        public bool GetTileSize(TileSpecifier spec, out int width, out int height) => this._atlas.GetTileSize(spec, out width, out height);

        public void Clear()
        {
            this._batch.Clear();
            this.PixelWidth = 0;
            this.PixelHeight = 0;
        }

        public void Draw(float uiScale, float x, float y, float? width = null, float? height = null)
        {
            Love.Graphics.SetColor(Love.Color.White);
            GraphicsEx.DrawSpriteBatchS(uiScale, this._batch, x, y, width, height);
        }

        public void Dispose()
        {
            this._batch.Dispose();
        }
    }
}
