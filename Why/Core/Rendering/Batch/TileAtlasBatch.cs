using Love;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    public class TileAtlasBatch : IDisposable
    {
        private TileAtlas Atlas { get; }
        private SpriteBatch Batch { get; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public TileAtlasBatch(TileAtlas atlas)
        {
            this.Atlas = atlas;
            this.Batch = Love.Graphics.NewSpriteBatch(atlas.Image, 2048, SpriteBatchUsage.Dynamic);
            this.Width = 0;
            this.Height = 0;
        }

        public void Add(TileSpecifier spec, int x, int y, int? width = null, int? height = null, Love.Color? color = null, bool centered = false, float rotation = 0f)
        {
            var tile = this.Atlas.GetTile(spec);

            if (tile == null)
                throw new Exception($"Unknown tile {spec.TileId}");

            var quadRect = tile.Quad.GetViewport();
            var sx = 1f;
            var sy = 1f;

            if (color != null)
            {
                var c = color.Value;
                this.Batch.SetColor(c.Rf, c.Gf, c.Bf, c.Af);
            }
            else
            {
                this.Batch.SetColor(1f, 1f, 1f, 1f);
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

            this.Batch.Add(tile.Quad, x, y, MathUtil.DegreesToRadians(rotation), sx, sy, ox, oy);

            this.Width = Math.Max(this.Width, x + ttw);
            this.Height = Math.Max(this.Height, y + tth);
        }

        public void Flush() => this.Batch.Flush();

        public bool GetTileSize(TileSpec spec, out int width, out int height) => this.Atlas.GetTileSize(spec, out width, out height);

        public void Clear()
        {
            this.Batch.Clear();
            this.Width = 0;
            this.Height = 0;
        }

        public void Draw(int x, int y, int width, int height)
        {
            Love.Graphics.SetColor(Love.Color.White);
            GraphicsEx.DrawSpriteBatch(this.Batch, x, y, width, height);
        }

        public void Dispose()
        {
            this.Batch.Dispose();
        }
    }
}
