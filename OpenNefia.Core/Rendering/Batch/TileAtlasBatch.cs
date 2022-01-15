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
        public int Width { get; private set; }
        public int Height { get; private set; }

        public TileAtlasBatch(string atlasName)
        {
            _atlasName = atlasName;
            _atlas = IoCManager.Resolve<ITileAtlasManager>().GetAtlas(atlasName);
            _batch = Love.Graphics.NewSpriteBatch(_atlas.Image, 2048, SpriteBatchUsage.Dynamic);
            Width = 0;
            Height = 0;
        }

        public void OnThemeSwitched()
        {
            _atlas = IoCManager.Resolve<ITileAtlasManager>().GetAtlas(_atlasName);
            _batch = Love.Graphics.NewSpriteBatch(_atlas.Image, 2048, SpriteBatchUsage.Dynamic);
        }

        public void Add(string tileId, int x, int y, int? width = null, int? height = null, Love.Color? color = null, bool centered = false, float rotation = 0f)
        {
            if (!this._atlas.TryGetTile(tileId, out var tile))
            {
                Logger.ErrorS("tile.batch", $"Unknown tile {tileId} in atlas {_atlasName}");
                return;
            }

            var quadRect = tile.Quad.GetViewport();
            var sx = 1f;
            var sy = 1f;

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
                sx = (float)width.Value / (float)ttw;
                ttw = width.Value;
            }
            if (height != null)
            {
                sy = (float)height.Value / (float)tth;
                tth = height.Value;
            }

            var ox = 0f;
            var oy = 0f;
            if (centered)
            {
                ox = (float)ttw / 2;
                oy = (float)tth / 2;
            }

            this._batch.Add(tile.Quad, x, y, MathUtil.DegreesToRadians(rotation), sx, sy, ox, oy);

            this.Width = Math.Max(this.Width, x + ttw);
            this.Height = Math.Max(this.Height, y + tth);
        }

        public void Flush() => this._batch.Flush();

        public bool GetTileSize(TileSpecifier spec, out int width, out int height) => this._atlas.GetTileSize(spec, out width, out height);

        public void Clear()
        {
            this._batch.Clear();
            this.Width = 0;
            this.Height = 0;
        }

        public void Draw(int x, int y, int? width = null, int? height = null)
        {
            Love.Graphics.SetColor(Love.Color.White);
            GraphicsEx.DrawSpriteBatch(this._batch, x, y, width, height);
        }

        public void Dispose()
        {
            this._batch.Dispose();
        }
    }
}
