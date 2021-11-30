using Love;
using OpenNefia.Core.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    public class TileAtlasBatch : IDisposable
    {
        private TileAtlas _atlas { get; }
        private SpriteBatch _batch { get; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public TileAtlasBatch(string atlasName)
        {
            _atlas = IoCManager.Resolve<ITileAtlasManager>().GetAtlas(atlasName);
            _batch = Love.Graphics.NewSpriteBatch(_atlas.Image, 2048, SpriteBatchUsage.Dynamic);
            Width = 0;
            Height = 0;
        }

        public void Add(TileSpecifier spec, int x, int y, int? width = null, int? height = null, Love.Color? color = null, bool centered = false, float rotation = 0f)
        {
            var tile = this._atlas.GetTile(spec);

            if (tile == null)
                throw new ArgumentException($"Unknown tile {spec.AtlasIndex}", nameof(spec));

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

        public void Draw(int x, int y, int width, int height)
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
