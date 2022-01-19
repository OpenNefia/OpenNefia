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
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Game;

namespace OpenNefia.Content.Hud
{
    public class UiHudMinimap : BaseHudWidget
    {
        [Dependency] private readonly ITileDefinitionManager _tileDefManager = default!;
        [Dependency] private readonly IEntityManager _entMan = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;

        private Vector2i MapSize;
        private MapCoordinates? PlayerCoords = default!;
        private TileAtlasBatch Batch = default!;
        private IAssetInstance PlayerIcon = default!;

        private readonly Color WallColor = new(90, 90, 90);

        public override void Initialize()
        {
            base.Initialize();
            Batch = new TileAtlasBatch(AtlasNames.Tile);
            PlayerIcon = Assets.Get(Protos.Asset.MinimapMarkerPlayer);
        }

        public override void UpdateWidget()
        {
            base.UpdateWidget();
            if (_entMan.TryGetComponent<SpatialComponent>(GameSession.Player, out var spatial))
                Refresh(_mapManager.ActiveMap?.TileMemory!, spatial.MapPosition);
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

    public class HudMinimapWidget : BaseHudWidget
    {
        private IAssetInstance MiniMapAsset = default!;
        private UiHudMinimap Minimap = default!;

        public const int MinimapWidth = 122;
        public const int MinimapHeight = 88;

        public override void Initialize()
        {
            base.Initialize();
            MiniMapAsset = Assets.Get(Protos.Asset.HudMinimap);
            Minimap = new UiHudMinimap();
            Minimap.Initialize();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            Minimap.SetPosition(x + 2, y + 2);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            Minimap.SetSize(MinimapWidth - 4, MinimapHeight - 4);
        }

        public override void UpdateWidget()
        {
            base.UpdateWidget();
            Minimap.UpdateWidget();
        }

        public override void Draw()
        {
            base.Draw();
            MiniMapAsset.Draw(X, Y);
            Minimap.Draw();
        }
    }
}