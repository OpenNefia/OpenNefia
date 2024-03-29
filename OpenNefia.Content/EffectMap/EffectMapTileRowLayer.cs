﻿using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Rendering.TileDrawLayers;
using OpenNefia.Core.Rendering.TileRowDrawLayers;
using OpenNefia.Core.UI;

namespace OpenNefia.Content.EffectMap
{
    [RegisterTileRowLayer(TileRowLayerType.Tile)]
    public sealed class EffectMapTileRowLayer : BaseTileRowLayer
    {
        [Dependency] private readonly ICoords _coords = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;

        private List<EffectMapEntry>[] _entries = { };

        public override void Initialize()
        {
            foreach (var row in _entries)
            {
                row.Clear();
            }
        }

        public override void OnThemeSwitched()
        {
            foreach (var row in _entries)
            {
                row.Clear();
            }
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

        public override void SetMap(IMap map)
        {
            base.SetMap(map);
            _entries = new List<EffectMapEntry>[map.Height];
            for (var y = 0; y < map.Height; y++)
            {
                _entries[y] = new();
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

            _entries[tilePos.Y].Add(new(asset, maxFrames.Value, rotation, type, screenPos, tilePos, alpha));
        }

        private void StepAll(float dt)
        {
            for (var y = 0; y < _entries.Length; y++)
            {
                var row = _entries[y];
                var i = 0;
                while (i < row.Count)
                {
                    var entry = row[i];

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

                row.RemoveAll(entry => entry.Frame >= entry.MaxFrames);
            }
        }

        public override void Update(float dt)
        {
            var frames = dt / (_config.GetCVar(CCVars.AnimeScreenRefresh) * Constants.FRAME_MS);
            StepAll(frames);
        }

        public override void DrawRow(int tileY, int screenX, int screenY)
        {
            foreach (var entry in _entries[tileY])
            {
                // TODO optimize...
                if (Map!.IsInWindowFov(entry.TilePosition))
                {
                    Love.Graphics.SetColor(Color.White.WithAlphaB(entry.Alpha));
                    entry.Asset.DrawRegion(_coords.TileScale, entry.AssetFrame.ToString(), screenX / _coords.TileScale + entry.ScreenPosition.X, screenY / _coords.TileScale + entry.ScreenPosition.Y, centered: true, rotationRads: entry.Rotation);
                }
            }
        }
    }
}