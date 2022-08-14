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
using OpenNefia.Core.UI;
using OpenNefia.Content.UI;

namespace OpenNefia.Content.Hud
{
    public class UiMinimap : BaseHudWidget
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

        public override void RefreshWidget()
        {
            base.RefreshWidget();
            if (_entMan.TryGetComponent<SpatialComponent>(GameSession.Player, out var spatial))
                Refresh(_mapManager.ActiveMap!.TileMemory, spatial.MapPosition);
        }

        public void Refresh(Tile[,] tiles, MapCoordinates playerPos)
        {
            MapSize = new(tiles.GetLength(0), tiles.GetLength(1));
            PlayerCoords = playerPos;
            var tileWidth = Width / tiles.GetLength(0);
            var tileHeight = Height / tiles.GetLength(1);
            Batch.Clear();
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                for (int y = 0; y < tiles.GetLength(1); y++)
                {
                    var tile = _tileDefManager[tiles[x, y].Type];
                    Batch.Add(UIScale,
                        tile.Image.AtlasIndex,
                        x * tileWidth, y * tileHeight, tileWidth, tileHeight, tile.IsSolid ? WallColor : Color.White);
                }
            }
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
        }

        public override void Draw()
        {
            Batch.Draw(UIScale, X, Y, Width, Height);
            if (PlayerCoords.HasValue)
            {
                float x = Width * ((float)PlayerCoords.Value.X / MapSize.X);
                float y = Height * ((float)PlayerCoords.Value.Y / MapSize.Y);
                PlayerIcon.Draw(UIScale, X + x, Y + y, centered: true);
            }
        }

        public override void Update(float dt)
        {
        }
    }

    public class HudMinimapWidget : BaseHudWidget
    {
        private IAssetInstance MiniMapAsset = default!;
        [Child] private UiMinimap Minimap = default!;

        public const float MinimapWidth = 122;
        public const float MinimapHeight = 88;

        public override void Initialize()
        {
            base.Initialize();
            MiniMapAsset = Assets.Get(Protos.Asset.HudMinimap);
            Minimap = new UiMinimap();
            Minimap.Initialize();
        }

        public override void GetPreferredSize(out Vector2 size)
        {
            size = (MinimapWidth, MinimapHeight);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            Minimap.SetPosition(x + 2, y + 2);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            Minimap.SetSize(MinimapWidth - 4, MinimapHeight - 4);
        }

        public override void RefreshWidget()
        {
            base.RefreshWidget();
            Minimap.RefreshWidget();
        }

        public override void Draw()
        {
            base.Draw();
            MiniMapAsset.Draw(UIScale, X, Y);
            Minimap.Draw();
        }
    }
}