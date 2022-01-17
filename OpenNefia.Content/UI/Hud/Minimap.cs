using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using OpenNefia.Content.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Maths;

namespace OpenNefia.Content.UI.Hud
{
    public class Minimap : BaseDrawable
    {
        [Dependency] private readonly ITileDefinitionManager _tileDefManager = default!;
        private Vector2i MapSize;
        private MapCoordinates? PlayerCoords = default!;
        private TileAtlasBatch Batch;
        private IAssetInstance PlayerIcon;

        private readonly Color WallColor = new(90, 90, 90);

        public Minimap()
        {
            IoCManager.InjectDependencies(this);
            Batch = new TileAtlasBatch(AtlasNames.Tile);
            PlayerIcon = Assets.Get(Protos.Asset.MinimapMarkerPlayer);
        }

        public void Refresh(Tile[,] tiles, MapCoordinates playerPos)
        {
            MapSize = new(tiles.GetLength(0), tiles.GetLength(1));
            PlayerCoords = playerPos;
            var tileWidth = Width / tiles.GetLength(0);
            var tileHeight = Height / tiles.GetLength(1);
            Batch.Clear();
            for(int x = 0; x < tiles.GetLength(0); x++)
            {
                for(int y = 0; y < tiles.GetLength(1); y++)
                {
                    var tile = _tileDefManager[tiles[x, y].Type];
                    Batch.Add($"{tile.ID}:Tile", 
                        x * tileWidth, y * tileHeight, tileWidth, tileHeight, tile.IsSolid ? WallColor : Color.White);
                }
            }
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            Batch.SetSize(width, height);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            Batch.SetPosition(x, y);
        }

        public override void Draw()
        {
            Batch.Draw();
            if (PlayerCoords.HasValue)
            {
                float xSize = (float)Width / (float)MapSize.X;
                float ySize = (float)Height / (float)MapSize.Y;
                float x = xSize * PlayerCoords.Value.X + (xSize / 2f);
                float y = ySize * PlayerCoords.Value.Y + (ySize / 2f);
                PlayerIcon.Draw(X + x, Y + y, centered: true);
            }
        }

        public override void Update(float dt)
        {
        }
    }
}