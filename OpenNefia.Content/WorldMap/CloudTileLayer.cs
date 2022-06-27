using OpenNefia.Content.MapVisibility;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Rendering.TileDrawLayers;
using OpenNefia.Core.UI;
using OpenNefia.Content.Prototypes;
using System.Collections.Generic;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Graphics;

namespace OpenNefia.Content.WorldMap
{
    [RegisterTileLayer(renderAfter: new[] { typeof(ShadowTileLayer) }, enabledAtStartup: false)]
    public sealed class CloudTileLayer : BaseTileLayer
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly ICoords _coords = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly IEntityManager _entityMan = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IGraphics _graphics = default!;

        private List<Cloud> _clouds = new();
        private float _frame = 0f;
        private float _speed = 10f;

        private const int CloudCount = 12;
        private PrototypeId<AssetPrototype>[] CloudAssets = new[]
        {
            Protos.Asset.Cloud1,
            Protos.Asset.Cloud2,
        };

        private sealed class Cloud
        {
            public Cloud(IAssetInstance asset, Vector2i tilePosition)
            {
                Asset = asset;
                TilePosition = tilePosition;
            }

            public IAssetInstance Asset { get; set; }
            public Vector2i TilePosition { get; set; } = Vector2i.Zero;
        }

        public override void Initialize()
        {
            _clouds.Clear();
            for (var i = 0; i < CloudCount; i++)
            {
                var cloudAsset = _rand.Pick(CloudAssets);
                var x = _rand.Next(100) + i * 200 + 100000;
                var y = _rand.Next(100) + (i / 5) * 200 + 100000;
                var pos = new Vector2i(x, y);
                _clouds.Add(new Cloud(Assets.GetAsset(cloudAsset), pos));
            }
        }

        public override void Update(float dt)
        {
            // TODO assumes 60fps
            _frame += dt;
        }

        public override void Draw()
        {
            var player = _gameSession.Player;
            if (!_entityMan.IsAlive(player))
                return;

            var playerPos = _entityMan.GetComponent<SpatialComponent>(player).WorldPosition;

            Love.Graphics.SetBlendMode(Love.BlendMode.Subtract);
            for (int i = 0; i < _clouds.Count; i++)
            {
                var cloud = _clouds[i];

                var color = Color.White.WithAlpha((byte)(7 + i * 2));
                Love.Graphics.SetColor(color);
                var pos = _coords.TileToScreen(cloud.TilePosition - playerPos);
                var x = ((float)pos.X * 100 / (40 + i * 5)) + (_frame * _speed) * 100 / (50 + i * 20);
                var y = (float)pos.Y * 100 / (40 + i * 5);

                var asset = cloud.Asset;
                x = (x % (_graphics.WindowPixelSize.X + asset.PixelWidth)) - asset.PixelWidth;
                y = (y % (_graphics.WindowPixelSize.Y - Constants.INF_VERH + asset.PixelHeight)) - asset.PixelHeight;

                if (y < _graphics.WindowPixelSize.Y - Constants.INF_VERH)
                    asset.DrawUnscaled(x, y);

            }
            Love.Graphics.SetBlendMode(Love.BlendMode.Alpha);
        }
    }
}