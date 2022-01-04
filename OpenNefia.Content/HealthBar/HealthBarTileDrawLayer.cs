using OpenNefia.Content.Factions;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Skills;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Rendering.TileDrawLayers;

namespace OpenNefia.Content.VanillaAI
{
    [RegisterTileLayer(renderAfter: new[] { typeof(TileAndChipTileLayer) })]
    public class HealthBarTileDrawLayer : BaseTileLayer
    {
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly ICoords _coords = default!;
        [Dependency] private readonly IEntityManager _entMan = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IVisibilitySystem _visibility = default!;

        private IAssetInstance AssetHPBarAlly = default!;
        private IAssetInstance AssetHPBarOther = default!;

        private IMap? _map;
        private List<DrawEntry> _entries = new();

        public override void SetMap(IMap map)
        {
            _map = map;
        }

        public override void Initialize()
        {
            AssetHPBarAlly = Assets.GetAsset(AssetPrototypeOf.HpBarAlly);
            AssetHPBarOther = Assets.GetAsset(AssetPrototypeOf.HpBarOther);
        }

        public override void RedrawAll()
        {
            UpdateEntries();
        }

        public override void RedrawDirtyTiles(HashSet<Vector2i> dirtyTilesThisTurn)
        {
            UpdateEntries();
        }

        private void UpdateEntries()
        {
            if (_map == null)
                return;

            foreach (var entry in _entries)
                entry.Dispose();

            _entries.Clear();

            var playerParty = EntityManager.EnsureComponent<PartyComponent>(_gameSession.Player!.Uid);

            foreach (var (spatial, skills) in _lookup.EntityQueryInMap<SpatialComponent, SkillsComponent>(_map.Id))
            {
                if (!_entMan.IsAlive(spatial.OwnerUid) || !_visibility.CanSeeEntity(_gameSession.Player.Uid!, spatial.OwnerUid))
                    continue;

                IAssetInstance assetInstance;

                if (playerParty.Members.Contains(spatial.OwnerUid))
                    assetInstance = AssetHPBarAlly;
                else
                    assetInstance = AssetHPBarOther;

                var hpRatio = Math.Clamp((float)skills.HP / skills.MaxHP, 0.0f, 1.0f);

                var screenPos = spatial.Owner.GetScreenPos();

                var entry = new DrawEntry(assetInstance, hpRatio, screenPos);
                _entries.Add(entry);
            }
        }

        public override void Update(float dt)
        {
        }

        private void DrawPercentageBar(DrawEntry entry, Vector2 pos, float barWidth, Vector2 drawSize)
        {
            var size = entry.Asset.Size;
            var lastWidth = barWidth;
            if (entry.BarWidth != barWidth)
            {
                entry.BarWidth = barWidth;
                entry.BarQuad.SetViewport(size.X - barWidth, 0, lastWidth, size.Y);
            }

            entry.Asset.Draw(entry.BarQuad, pos.X, pos.Y, drawSize.X, drawSize.Y);
        }

        private const int BarWidthPixels = 30;

        public override void Draw()
        {
            var size = _coords.TileSize;

            Love.Graphics.SetColor(Color.White);

            foreach (var entry in _entries)
            {
                DrawPercentageBar(entry,
                    GlobalPixelPosition + entry.ScreenPos + (9, size.Y),
                    entry.HPRatio * BarWidthPixels,
                    Vector2i.Zero);
            }
        }

        private class DrawEntry : IDisposable
        {
            public DrawEntry(IAssetInstance assetInstance, float hpRatio, Vector2 screenPos)
            {
                Asset = assetInstance;
                HPRatio = hpRatio;
                ScreenPos = screenPos;
                BarQuad = Love.Graphics.NewQuad(0, 0, assetInstance.Width, assetInstance.Height, assetInstance.Width, assetInstance.Height);
                BarWidth = -1;
            }

            public IAssetInstance Asset { get; }
            public float HPRatio { get; }
            public Vector2 ScreenPos { get; }
            public Love.Quad BarQuad { get; }

            public float BarWidth { get; set; }

            public void Dispose()
            {
                BarQuad.Dispose();
            }
        }
    }
}
