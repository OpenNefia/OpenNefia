﻿using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Content.EffectMap
{
    public interface IEffectMapSystem : IEntitySystem
    {
        void AddEffectMap(PrototypeId<AssetPrototype> assetID, MapCoordinates coords, int? maxFrames = null, float rotation = 0f, EffectMapType type = EffectMapType.Anime);
    }

    public sealed class EffectMapSystem : EntitySystem, IEffectMapSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IMapTileRowRenderer _tileRowRenderer = default!;

        public override void Initialize()
        {
        }

        public void AddEffectMap(PrototypeId<AssetPrototype> assetID, MapCoordinates coords, int? maxFrames = null, float rotation = 0f, EffectMapType type = EffectMapType.Anime)
        {
            if (coords.MapId != _mapManager.ActiveMap?.Id)
                return;

            if (_tileRowRenderer.TryGetTileRowLayer<EffectMapTileRowLayer>(out var layer))
                layer.AddEffectMap(assetID, coords.Position, maxFrames, rotation, type);
        }
    }

    public enum EffectMapType
    {
        Anime,
        Fade
    }
}