using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Rendering.TileDrawLayers;
using OpenNefia.Core.UI;

namespace OpenNefia.Content.EffectMap
{
    [RegisterTileLayer(renderAfter: new[] { typeof(TileAndChipTileLayer) })]
    public sealed class EffectMapTileLayer : BaseTileLayer
    {
        [Dependency] private readonly ICoords _coords = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;

        private readonly List<EffectMapEntry> _entries = new();

        public override void Initialize()
        {
            _entries.Clear();
        }

        public override void OnThemeSwitched()
        {
            _entries.Clear();
        }

        private sealed class EffectMapEntry
        {
            public IAssetInstance Asset;
            public int MaxFrames;
            public float Rotation;
            public EffectMapType Type;
            public Vector2i ScreenPosition = Vector2i.Zero;
            public Vector2i TilePosition = Vector2i.Zero;
            public byte Alpha;

            public float Dt = 0f;
            public int AssetFrame = 0;
            public int Frame = 0;

            public EffectMapEntry(IAssetInstance asset, int maxFrames, float rotation, EffectMapType type, Vector2i screenPosition, Vector2i tilePosition, byte alpha)
            {
                Asset = asset;
                MaxFrames = maxFrames;
                Rotation = rotation;
                Type = type;
                ScreenPosition = screenPosition;
                TilePosition = tilePosition;
                Alpha = alpha;
            }
        }

        public void AddEffectMap(PrototypeId<AssetPrototype> assetID, Vector2i tilePos, int? maxFrames = null, float rotation = 0f, EffectMapType type = EffectMapType.Anime)
        {
            var asset = Assets.GetAsset(assetID);
            byte alpha;

            switch (type)
            {
                case EffectMapType.Anime:
                default:
                    maxFrames = Math.Clamp(maxFrames ?? asset.Regions.Count - 1, 0, asset.Regions.Count - 1);
                    alpha = 150;
                    break;
                case EffectMapType.Fade:
                    maxFrames = Math.Max(maxFrames ?? 10, 0);
                    alpha = (byte)(maxFrames.Value * 12 + 30);
                    break;
            }

            var screenPos = _coords.TileToScreen(tilePos);
            screenPos += _coords.TileSize / 2;

            _entries.Add(new(asset, maxFrames.Value, rotation, type, screenPos, tilePos, alpha));
        }

        private void StepAll(float dt)
        {
            var i = 0;
            while (i < _entries.Count - 1)
            {
                var entry = _entries[i];

                entry.Dt += dt;

                while (entry.Dt > 0)
                {
                    entry.Dt -= 1f;
                    entry.Frame++;
                    switch (entry.Type)
                    {
                        case EffectMapType.Anime:
                        default:
                            entry.AssetFrame++;
                            break;
                        case EffectMapType.Fade:
                            entry.Alpha = (byte)((entry.MaxFrames - entry.Frame) * 12 + 30);
                            break;
                    }
                }

                i++;
            }

            _entries.RemoveAll(entry => entry.Frame >= entry.MaxFrames);
        }

        public override void Update(float dt)
        {
            var frames = dt / (_config.GetCVar(CCVars.AnimeScreenRefresh) * Constants.FRAME_MS);
            StepAll(frames);
        }

        public override void Draw()
        {
            foreach (var entry in _entries)
            {
                if (Map!.IsInWindowFov(entry.TilePosition))
                {
                    Love.Graphics.SetColor(Color.White.WithAlphaB(entry.Alpha));
                    entry.Asset.DrawRegion(_coords.TileScale, entry.AssetFrame.ToString(), PixelX + entry.ScreenPosition.X, PixelY + entry.ScreenPosition.Y, centered: true, rotationRads: entry.Rotation);
                }
            }
        }
    }
}