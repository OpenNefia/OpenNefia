using Love;
using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    [Serializable]
    public class AnimFrame
    {
        public string TileId = string.Empty;
        public float Duration = 0f;
    }

    [Serializable]
    public class AtlasTile
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
    }

    [Serializable]
    public class TileAtlas
    {
        private Dictionary<string, AtlasTile> _tiles = new Dictionary<string, AtlasTile>();
        private Dictionary<string, List<AnimFrame>> _anims = new Dictionary<string, List<AnimFrame>>();
        private ResourcePath _imageFilepath;

        public Love.Image Image { get; private set; }

        public TileAtlas(Image image, ResourcePath imageFilepath, Dictionary<string, AtlasTile> atlasTiles)
        {
            this.Image = image;
            this._imageFilepath = imageFilepath;
            this._tiles = atlasTiles;
        }

        public AtlasTile? GetTile(string tileId)
        {
            if (_tiles.TryGetValue(tileId, out var tile))
                return tile;
            return null;
        }

        public AtlasTile? GetTile(TileSpecifier spec) => GetTile(spec.Identifier);

        public bool GetTileSize(TileSpecifier spec, out int width, out int height)
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
    }
}
