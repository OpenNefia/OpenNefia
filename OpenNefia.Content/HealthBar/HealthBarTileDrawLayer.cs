using OpenNefia.Content.Factions;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Content.UI;
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
        private List<UiHelpers.UiBarDrawableState> _entries = new();

        public override void SetMap(IMap map)
        {
            _map = map;
        }

        public override void Initialize()
        {
            AssetHPBarAlly = Assets.GetAsset(Protos.Asset.HpBarAlly);
            AssetHPBarOther = Assets.GetAsset(Protos.Asset.HpBarOther);
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

            var playerParty = EntityManager.EnsureComponent<PartyComponent>(_gameSession.Player!);

            foreach (var (spatial, skills) in _lookup.EntityQueryInMap<SpatialComponent, SkillsComponent>(_map.Id))
            {
                if (!_entMan.IsAlive(spatial.Owner) || !_visibility.CanSeeEntity(_gameSession.Player!, spatial.Owner))
                    continue;

                IAssetInstance assetInstance;

                if (playerParty.Members.Contains(spatial.Owner))
                    assetInstance = AssetHPBarAlly;
                else
                    assetInstance = AssetHPBarOther;

                var hpRatio = Math.Clamp((float)skills.HP / skills.MaxHP, 0.0f, 1.0f);

                var screenPos = spatial.GetScreenPos();

                var entry = new UiHelpers.UiBarDrawableState(assetInstance, hpRatio, screenPos);
                _entries.Add(entry);
            }
        }

        public override void Update(float dt)
        {
        }

        private const int BarWidthPixels = 30;

        public override void Draw()
        {
            var size = _coords.TileSize;

            Love.Graphics.SetColor(Color.White);

            foreach (var entry in _entries)
            {
                UiHelpers.DrawPercentageBar(entry,
                    GlobalPixelPosition + entry.ScreenPos + (9, size.Y),
                    entry.HPRatio * BarWidthPixels,
                    Vector2i.Zero);
            }
        }
    }
}
