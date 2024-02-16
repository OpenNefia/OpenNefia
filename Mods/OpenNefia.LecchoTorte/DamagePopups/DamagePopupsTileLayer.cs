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
using Microsoft.Extensions.ObjectPool;

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
        public IAssetInstance? Icon { get; set; }
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
        private ObjectPool<UiTextShadowed> _textPool = new DefaultObjectPool<UiTextShadowed>(new DefaultPooledObjectPolicy<UiTextShadowed>());

        private sealed class DamagePopupInstance
        {
            public DamagePopup DamagePopup { get; set; }
            public Vector2 ScreenPosition { get; set; }
            public EntityUid? Entity { get; set; }
            public float Frame { get; set; }
            public UiTextShadowed UiText { get; set; } = new();
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

            var text = _textPool.Get();
            text.Font = popup.Font;
            text.Text = popup.Text;

            _popups.Enqueue(new DamagePopupInstance()
            {
                DamagePopup = popup,
                ScreenPosition = (_coords.TileToScreen(coords.Position) - (0, _coords.TileSize.Y / 2)) * _coords.TileScale,
                Frame = 0f,
                UiText = text
            });
        }

        public void AddDamagePopup(DamagePopup popup, EntityUid uid)
        {
            var spatial = _entityMan.GetComponent<SpatialComponent>(uid);
            var coords = spatial.MapPosition;

            if (_mapManager.ActiveMap?.Id != coords.MapId)
                return;

            if (_popups.Count > MaxDamagePopups)
            {
                var toRemove = _popups.Dequeue();
                _textPool.Return(toRemove.UiText);
            }

            foreach (var popupInst in _popups)
            {
                popupInst.ScreenPosition = (popupInst.ScreenPosition.X, popupInst.ScreenPosition.Y - popupInst.DamagePopup.Font.Size * _coords.TileScale);
            }

            var text = _textPool.Get();
            text.Font = popup.Font;
            text.Text = popup.Text;

            var popupInstance = new DamagePopupInstance()
            {
                DamagePopup = popup,
                ScreenPosition = (_coords.TileToScreen(coords.Position) - (0, _coords.TileSize.Y / 2)) * _coords.TileScale,
                Frame = 0f,
                UiText = text,
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
                _textPool.Return(instance.UiText);
                if (instance.Entity != null)
                {
                    var perEntity = _popupsPerEntity[instance.Entity.Value];
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
                    popup.ScreenPosition = _coords.TileToScreen(spatial.MapPosition.Position) * _coords.TileScale;

                    if (_popupsPerEntity.TryGetValue(popup.Entity.Value, out var perEntity))
                    {
                        yOffset = perEntity.Count - popup.YOffset;
                    }
                }

                var pos = popup.ScreenPosition;

                popup.UiText.Color = popup.DamagePopup.Color;
                popup.UiText.BgColor = popup.DamagePopup.ShadowColor;

                popup.UiText.SetPreferredSize();

                pos += new Vector2(X - (popup.UiText.TextWidth / 2) + (_coords.TileSize.X * _coords.TileScale) / 2f, Y - (popup.UiText.Height / 2f) - popup.Frame - (yOffset * popup.UiText.Height) + (_coords.TileSize.Y * _coords.TileScale) / 2f);

                if (popup.DamagePopup.Icon != null)
                {
                    var icon = popup.DamagePopup.Icon;
                    var iconHeight = icon.PixelWidth;
                    var textHeight = popup.UiText.PixelHeight;
                    var ratio = textHeight / iconHeight;
                    Love.Graphics.SetColor(Color.White.WithAlphaB(popup.DamagePopup.Color.AByte));
                    popup.DamagePopup.Icon.DrawUnscaled(pos.X - icon.PixelWidth - (icon.PixelWidth * ratio) / 4, pos.Y + (icon.PixelHeight * ratio) / 8, icon.PixelWidth * ratio, icon.PixelHeight * ratio);
                }

                popup.UiText.SetPosition(pos.X, pos.Y);
                popup.UiText.Draw();
            }
        }
    }
}