using OpenNefia.Content.Inventory;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Rendering;
using OpenNefia.Content.Visibility;
using OpenNefia.Content.World;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Rendering.TileDrawLayers;
using OpenNefia.Core.UI;
using OpenNefia.Core.Configuration;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.MapVisibility
{
    [RegisterTileLayer(renderAfter: new[] { typeof(TileAndChipTileLayer) })]
    public sealed class ShadowTileLayer : BaseTileLayer
    {
        [Dependency] private readonly IAssetManager _assetManager = default!;
        [Dependency] private readonly ICoords _coords = default!;
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IEntityLookup _entityLookup = default!;
        [Dependency] private readonly IInventorySystem _inv = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IVisibilitySystem _vis = default!;
        [Dependency] private readonly IWorldSystem _world = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;

        private LightInstance?[,] _lights = new LightInstance?[0, 0];

        private MapVisibilityComponent _mapVis = default!;
        private ShadowBatch _batch = new();
        private float _frames = 0f;

        public const int DefaultShadowStrength = 70;
        
        public int ShadowStrength { get; set; } = DefaultShadowStrength;

        public override void Initialize()
        {
            _batch.Initialize(_assetManager, _coords, _config);
        }

        public override void OnThemeSwitched()
        {
            _batch.OnThemeSwitched();
        }

        public override void SetMap(IMap map)
        {
            base.SetMap(map);
            _mapVis = _entityManager.GetComponent<MapVisibilityComponent>(map.MapEntityUid);
            _batch.SetMapSize(map.Size);
            _lights = new LightInstance?[map.Width, map.Height];
            _frames = 0f;
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            _batch.SetSize(width, height);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            _batch.SetPosition(x, y);
        }

        public override void RedrawAll()
        {
            var shadowStrength = ShadowStrength;

            var dungeonLight = _inv.EntityQueryInInventory<DungeonLightComponent>(_gameSession.Player).FirstOrDefault();
            if (dungeonLight != null && dungeonLight.IsLit && EntityManager.HasComponent<MapTypeDungeonComponent>(Map!.MapEntityUid))
            {
                shadowStrength -= dungeonLight.LightPower;
            }

            var hour = _world.State.GameDate.Hour;

            var playerSpatial = EntityManager.GetComponent<SpatialComponent>(_gameSession.Player);

            _lights = new LightInstance?[Map!.Width, Map.Height];
            foreach (var lightSource in _entityLookup.EntityQueryInMap<LightSourceComponent>(Map!))
            {
                if (!_vis.IsInWindowFov(lightSource.Owner))
                    continue;

                var lightProto = _protos.Index(lightSource.ID);

                var showLight = lightProto.AlwaysOn || (hour > 17 || hour < 6);
                if (!showLight)
                    continue;

                var spatial = EntityManager.GetComponent<SpatialComponent>(lightSource.Owner);
                if (_lights[spatial.WorldPosition.X, spatial.WorldPosition.Y] != null)
                    continue;

                var power = 6;
                if (playerSpatial.MapPosition.TryDistanceTiled(spatial.MapPosition, out var dist))
                    power -= dist;
                power = Math.Clamp(power, 0, 6) * lightProto.Power;
                shadowStrength -= power;

                var light = new LightInstance(Assets.GetAsset(lightProto.AssetID))
                {
                    GlobalScreenPosition = _coords.TileToScreen(spatial.MapPosition.Position),
                    Brightness = lightProto.Brightness,
                    Offset = lightProto.Offset,
                    Power = power,
                    Flicker = lightProto.Flicker,
                    Color = Color.White,
                    Frame = 1,
                };
                _lights[spatial.WorldPosition.X, spatial.WorldPosition.Y] = light;
            }

            shadowStrength = Math.Max(shadowStrength, 25);

            _batch.SetAllTileShadows(_mapVis.ShadowMap.ShadowTiles, _mapVis.ShadowMap.ShadowBounds, shadowStrength);
            _batch.UpdateBatches();

            UpdateLights();
        }

        public override void RedrawDirtyTiles(HashSet<Vector2i> dirtyTilesThisTurn)
        {
            RedrawAll();
        }

        public override void Update(float dt)
        {
            _frames += dt / (_config.GetCVar(CCVars.AnimeScreenRefresh) * Constants.FRAME_MS);
            _batch.Update(dt);

            var updateLight = false;

            if (_frames > 1f)
            {
                _frames %= 1f;
                updateLight = true;
            }

            if (updateLight)
            {
                UpdateLights();
            }
        }

        private void UpdateLights()
        {
            foreach (var light in _lights)
            {
                if (light == null)
                    continue;

                var flicker = light.Brightness + _rand.Next(light.Flicker + 1);
                light.Color = light.Color.WithAlphaB((byte)flicker);

                if (light.Asset.CountX > 1)
                    light.Frame = _rand.Next((int)light.Asset.CountX);
                else
                    light.Frame = 0;
            }
        }

        public override void Draw()
        {
            Love.Graphics.SetBlendMode(Love.BlendMode.Add);
            foreach (var light in _lights)
            {
                if (light == null)
                    continue;

                Love.Graphics.SetColor(light.Color);
                if (light.Asset.CountX == 0)
                {
                    light.Asset.Draw(_coords.TileScale, X + light.GlobalScreenPosition.X + light.Offset.X, Y + light.GlobalScreenPosition.Y + light.Offset.Y);
                }
                else
                {
                    light.Asset.DrawRegion(_coords.TileScale, light.Frame.ToString(), X + light.GlobalScreenPosition.X + light.Offset.X, Y + light.GlobalScreenPosition.Y + light.Offset.Y);
                }
            }

            Love.Graphics.SetBlendMode(Love.BlendMode.Alpha);
            Love.Graphics.SetColor(Color.White);

            _batch.Draw();
        }
    }

    internal sealed class LightInstance
    {
        public LightInstance(IAssetInstance asset)
        {
            Asset = asset;
        }

        public IAssetInstance Asset { get; set; }

        public Vector2i GlobalScreenPosition { get; set; }

        public int Brightness { get; set; }

        public Vector2i Offset { get; set; }

        public int Power { get; set; }

        public int Flicker { get; set; }

        public Color Color { get; set; } = Color.White;

        public int Frame { get; set; } = 0;
    }
}
