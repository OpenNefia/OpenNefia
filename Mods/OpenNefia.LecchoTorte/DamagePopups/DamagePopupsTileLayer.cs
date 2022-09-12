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
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.Utility;

namespace OpenNefia.LecchoTorte.DamagePopups
{
    public class DamagePopup
    {
        public static readonly FontSpec DefaultFont =
            new(16, 16, color: UiColors.TextWhite, bgColor: UiColors.TextBlack);

        public string Text { get; set; } = string.Empty;
        public Color Color { get; set; } = Color.White;
        public Color ShadowColor { get; set; } = Color.Black;
        public FontSpec Font { get; set; } = DefaultFont;
    }

    [RegisterTileLayer(renderAfter: new[] { typeof(ShadowTileLayer) })]
    public sealed class DamagePopupsTileLayer : BaseTileLayer
    {
        [Dependency] private readonly ICoords _coords = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly IEntityManager _entityMan = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;
        [Dependency] private readonly IGraphics _graphics = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;

        private const int MaxDamagePopups = 20;
        private Queue<DamagePopupInstance> _popups = new();
        private Dictionary<EntityUid, Queue<DamagePopupInstance>> _popupsPerEntity = new();

        private sealed class DamagePopupInstance
        {
            public DamagePopup DamagePopup { get; set; }
            public Vector2 ScreenPosition { get; set; }
            public EntityUid? Entity { get; set; }
            public float Frame { get; set; }
            public UiText UiText { get; set; } = new();
            public int YOffset { get; set; }
        }

        public override void Initialize()
        {
            _popups.Clear();
        }

        public void AddDamagePopup(DamagePopup popup, MapCoordinates coords)
        {
            if (_mapManager.ActiveMap?.Id != coords.MapId)
                return;

            if (_popups.Count > MaxDamagePopups)
                _popups.Dequeue();

            _popups.Enqueue(new DamagePopupInstance()
            {
                DamagePopup = popup,
                ScreenPosition = _coords.TileToScreen(coords.Position),
                Frame = 0f,
                UiText = new UiTextShadowed(popup.Font, popup.Text)
            });
        }

        public void AddDamagePopup(DamagePopup popup, EntityUid uid)
        {
            var spatial = _entityMan.GetComponent<SpatialComponent>(uid);
            var coords = spatial.MapPosition;

            if (_mapManager.ActiveMap?.Id != coords.MapId)
                return;

            if (_popups.Count > MaxDamagePopups)
                _popups.Dequeue();

            var popupInstance = new DamagePopupInstance()
            {
                DamagePopup = popup,
                ScreenPosition = _coords.TileToScreen(coords.Position),
                Frame = 0f,
                UiText = new UiTextShadowed(popup.Font, popup.Text),
                Entity = uid
            };
            _popups.Enqueue(popupInstance);
            var perEntity = _popupsPerEntity.GetOrInsertNew(uid);
            popupInstance.YOffset = perEntity.Count;
            perEntity.Enqueue(popupInstance);
        }

        public void ClearDamagePopups()
        {
            _popups.Clear();
        }

        private const float MaxFrame = 80f;

        public override void Update(float dt)
        {
            foreach (var popup in _popups)
            {
                popup.Frame += dt * 50f;
                var alpha = MathF.Min(OutQuint(1f - (popup.Frame / MaxFrame), 0f, 1f, 1f), 1f);
                popup.DamagePopup.Color = popup.DamagePopup.Color.WithAlphaF(alpha);
                popup.DamagePopup.ShadowColor = popup.DamagePopup.ShadowColor.WithAlphaF(alpha);
            }

            while (_popups.TryPeek(out var popup) && popup.Frame > MaxFrame)
            {
                var instance = _popups.Dequeue();
                if (instance.Entity != null)
                {
                    var perEntity = _popupsPerEntity[instance.Entity.Value];
                    perEntity.Dequeue();
                    foreach (var other in perEntity)
                    {
                        other.YOffset--;
                    }
                }
            }
        }

        private static float OutQuint(float t, float b, float c, float d)
        {
            t = t / d - 1f;
            return c * (MathF.Pow(t, 5f) + 1f) + b;
        }

        public override void Draw()
        {
            foreach (var popup in _popups)
            {
                var yOffset = 0;

                if (_entityMan.IsAlive(popup.Entity))
                {
                    var spatial = _entityMan.GetComponent<SpatialComponent>(popup.Entity.Value);
                    // TODO support fractional positions
                    popup.ScreenPosition = _coords.TileToScreen(spatial.MapPosition.Position);

                    if (_popupsPerEntity.TryGetValue(popup.Entity.Value, out var perEntity))
                    {
                        yOffset = perEntity.Count - popup.YOffset;
                    }
                }

                var pos = popup.ScreenPosition;

                popup.UiText.Color = popup.DamagePopup.Color;
                popup.UiText.BgColor = popup.DamagePopup.ShadowColor;

                popup.UiText.SetPreferredSize();

                pos += new Vector2(X - (popup.UiText.TextWidth / 2) + _coords.TileSize.X / 2f, Y - (popup.UiText.Height / 2f) - (popup.Frame) - (yOffset * popup.UiText.Height) + _coords.TileSize.Y / 2f);

                popup.UiText.SetPosition(pos.X, pos.Y);
                popup.UiText.Draw();
            }
        }
    }
}