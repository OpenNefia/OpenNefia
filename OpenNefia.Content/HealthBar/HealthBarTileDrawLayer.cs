using Love;
using OpenNefia.Content.Factions;
using OpenNefia.Content.HealthBar;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Content.UI;
using OpenNefia.Content.Visibility;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Rendering.TileDrawLayers;
using OpenNefia.Core.UI;

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
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IHealthBarSystem _healthBar = default!;

        private IAssetInstance AssetHPBarAlly = default!;
        private IAssetInstance AssetHPBarOther = default!;

        private List<UiHelpers.UiBarDrawableState> _entries = new();

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
            if (Map == null)
                return;

            _entries.Clear();

            foreach (var (spatial, skills) in _lookup.EntityQueryInMap<SpatialComponent, SkillsComponent>(Map.Id))
            {
                if (!_entMan.IsAlive(spatial.Owner) || !_visibility.CanSeeEntity(_gameSession.Player!, spatial.Owner))
                    continue;

                if (!_healthBar.ShouldShowHealthBarFor(spatial.Owner))
                    continue;

                IAssetInstance assetInstance;

                if (_parties.IsInPlayerParty(spatial.Owner))
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

        private const float BarWidthPixels = 30f;

        public override void Draw()
        {
            var size = _coords.TileSize;
            var barHeight = 3;

            Love.Graphics.SetColor(Love.Color.White);

            foreach (var entry in _entries)
            {
                UiHelpers.DrawPercentageBar(1f, 
                    entry,
                    Position + (entry.ScreenPos + (9, size.Y)) * _coords.TileScale,
                    entry.HPRatio * BarWidthPixels * _coords.TileScale,
                    (0, barHeight * _coords.TileScale));
            }
        }
    }
}
