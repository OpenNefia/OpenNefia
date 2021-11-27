using Love;
using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Util;
using OpenNefia.Serial;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    public class AnimFrame : IDataExposable
    {
        public string TileId = string.Empty;
        public float Duration = 0f;

        public AnimFrame() { }

        public void Expose(DataExposer data)
        {
            data.ExposeValue(ref TileId!, nameof(TileId));
            data.ExposeValue(ref Duration, nameof(Duration));
        }
    }

    public class AtlasTile : IDataExposable
    {
        public Love.Quad Quad;
        public int YOffset = 0;
        public bool HasOverhang = false;

        public AtlasTile() 
        {
            Quad = Love.Graphics.NewQuad(0, 0, 0, 0, 0, 0);
        }

        public AtlasTile(Quad quad, int yOffset = 0)
        {
            Quad = quad;
            YOffset = yOffset;
        }

        public void Expose(DataExposer data)
        {
            data.ExposeDeep(ref Quad, nameof(Quad));
            data.ExposeValue(ref YOffset, nameof(YOffset));
            data.ExposeValue(ref HasOverhang, nameof(HasOverhang));
        }
    }

    public class TileAtlas : IDataExposable
    {
        private Dictionary<string, AtlasTile> Tiles = new Dictionary<string, AtlasTile>();
        private Dictionary<string, List<AnimFrame>> Anims = new Dictionary<string, List<AnimFrame>>();
        private string ImageFilepath;
        public Image Image { get; private set; }

        public TileAtlas(Image image, string imageFilepath, Dictionary<string, AtlasTile> atlasTiles)
        {
            this.Image = image;
            this.ImageFilepath = imageFilepath;
            this.Tiles = atlasTiles;
        }

        public AtlasTile? GetTile(string tileId)
        {
            if (Tiles.TryGetValue(tileId, out var tile))
                return tile;
            return null;
        }

        public AtlasTile? GetTile(TileSpec spec) => GetTile(spec.TileIndex);

        public bool GetTileSize(TileSpec spec, out int width, out int height)
        {
            var tile = GetTile(spec);
            if (tile == null)
            {
                width = 0;
                height = 0;
                return false;
            }

            var rect = tile.Quad.GetViewport();

            width = (int)rect.Width;
            height = (int)rect.Height;

            return true;
        }

        public void Expose(DataExposer data)
        {
            data.ExposeCollection(ref this.Tiles, nameof(Tiles));
            data.ExposeCollection(ref this.Anims, nameof(Anims));
            data.ExposeValue(ref this.ImageFilepath!, nameof(ImageFilepath));

            if (data.Stage == SerialStage.ResolvingRefs)
            {
                this.Image = ImageLoader.NewImage(this.ImageFilepath);
            }
        }
    }
}
