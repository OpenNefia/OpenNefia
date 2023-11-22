using OpenNefia.Core.Rendering.TileDrawLayers;
using OpenNefia.Core.Rendering;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Maths;
using OpenNefia.Core.IoC;
using static OpenNefia.Core.Rendering.AssetInstance;
using OpenNefia.Core.GameObjects;
using OpenNefia.Content.Visibility;
using OpenNefia.Content.GameObjects;

namespace OpenNefia.Content.EmotionIcon
{
    [RegisterTileLayer(renderAfter: new[] { typeof(TileAndChipTileLayer) })]
    public class EmotionIconTileDrawLayer : BaseTileLayer
    {
        [Dependency] private readonly ICoords _coords = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IVisibilitySystem _vis = default!;
        [Dependency] private readonly IEntityMemorySystem _entityMemory = default!;

        private static readonly Dictionary<string, string> BatchIndices = new()
        {
            { EmotionIcons.Happy, "6" },
            { EmotionIcons.Silent, "7" },
            { EmotionIcons.Skull, "8" },
            { EmotionIcons.Bleed, "9" },
            { EmotionIcons.Blind, "10" },
            { EmotionIcons.Confuse, "11" },
            { EmotionIcons.Dim, "12" },
            { EmotionIcons.Fear, "13" },
            { EmotionIcons.Sleep, "14" },
            { EmotionIcons.Paralyze, "15" },
            { EmotionIcons.Eat, "16" },
            { EmotionIcons.Heart, "17" },
            { EmotionIcons.Angry, "18" },
            { EmotionIcons.Item, "19" },
            { EmotionIcons.Notice, "20" },
            { EmotionIcons.Question, "21" },
            { EmotionIcons.QuestTarget, "22" },
            { EmotionIcons.QuestClient, "23" },
            { EmotionIcons.Insane, "24" },
            { EmotionIcons.Party, "25" },
        };

        private IAssetInstance _assetEmotionIcons = default!;
        private Love.SpriteBatch? _batch;

        public override void Initialize()
        {
            _assetEmotionIcons = Assets.GetAsset(Protos.Asset.EmotionIcons);
        }

        public override void RedrawDirtyTiles(HashSet<Vector2i> dirtyTilesThisTurn)
        {
            RedrawAll();
        }

        public override void RedrawAll()
        {
            var parts = new List<AssetBatchPart>();
            var memory = new MapObjectMemory();

            foreach (var (spatial, emoIcon) in _lookup.EntityQueryInMap<SpatialComponent, EmotionIconComponent>(Map!))
            {
                if (!EntityManager.IsAlive(spatial.Owner) || !_vis.IsInWindowFov(spatial.Owner) || emoIcon.EmotionIconId == null)
                    continue;

                if (BatchIndices.TryGetValue(emoIcon.EmotionIconId, out var regionID))
                {
                    var pos = _coords.TileToScreen(spatial.WorldPosition);
                    _entityMemory.GetEntityMemory(spatial.Owner, ref memory);
                    parts.Add(new(regionID, pos.X + 4 + 16 + memory.ScreenOffset.X, pos.Y - 16 + memory.ScreenOffset.Y));
                }
            }

            _batch?.Dispose();
            _batch = _assetEmotionIcons.MakeBatch(parts);
        }

        public override void Update(float dt)
        {
        }

        public override void Draw()
        {
            Love.Graphics.SetColor(Color.White);
            GraphicsS.DrawS(_coords.TileScale, _batch!, PixelX, PixelY);
        }
    }
}
