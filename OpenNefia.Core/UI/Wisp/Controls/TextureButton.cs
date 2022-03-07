using System;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Core.UI.Wisp.Controls
{
    public class TextureButton : BaseButton
    {
        private Vector2 _scale = (1, 1);
        private PrototypeId<AssetPrototype>? _assetNormal;
        public const string StylePropertyTexture = "texture";
        public const string StylePseudoClassNormal = "normal";
        public const string StylePseudoClassHover = "hover";
        public const string StylePseudoClassDisabled = "disabled";
        public const string StylePseudoClassPressed = "pressed";

        public TextureButton()
        {
            DrawModeChanged();
        }

        public PrototypeId<AssetPrototype>? AssetNormal
        {
            get => _assetNormal;
            set
            {
                _assetNormal = value;
                InvalidateMeasure();
            }
        }

        public Vector2 Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                InvalidateMeasure();
            }
        }

        protected override void DrawModeChanged()
        {
            switch (DrawMode)
            {
                case DrawModeEnum.Normal:
                    SetOnlyStylePseudoClass(StylePseudoClassNormal);
                    break;
                case DrawModeEnum.Pressed:
                    SetOnlyStylePseudoClass(StylePseudoClassPressed);
                    break;
                case DrawModeEnum.Hover:
                    SetOnlyStylePseudoClass(StylePseudoClassHover);
                    break;
                case DrawModeEnum.Disabled:
                    SetOnlyStylePseudoClass(StylePseudoClassDisabled);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void Draw()
        {
            var assetId = AssetNormal;

            if (assetId == null)
            {
                TryGetStyleProperty(StylePropertyTexture, out assetId);
                if (assetId == null)
                {
                    return;
                }
            }

            var asset = assetId == null ? null : Assets.Get(assetId.Value);
            asset?.Draw(UIScale, GlobalPixelRect);
        }

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            var assetId = AssetNormal;

            if (assetId == null)
            {
                TryGetStyleProperty(StylePropertyTexture, out assetId);
            }

            var asset = assetId == null ? null : Assets.Get(assetId.Value);
            return Scale * (asset?.VirtualSize(UIScale) ?? Vector2.Zero);
        }
    }
}
