using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using OpenNefia.VisualAI.Engine;

namespace OpenNefia.VisualAI.UserInterface
{
    public sealed class VisualAIBlockIcon : UiElement
    {
        private IAssetInstance? _assetIcon;
        public PrototypeId<AssetPrototype>? Icon
        {
            get => _assetIcon?.Asset.GetStrongID();
            set => _assetIcon = value != null ? Assets.Get(value.Value) : null;
        }

        public float BorderSize { get; set; } = 1;
        public float Padding { get; set; } = DefaultPadding;
        public Color Color { get; set; }
        public bool IsSelected { get; set; }

        public const float DefaultSize = 48;
        public const float DefaultPadding = 8;

        public Vector2 SizeWithoutPadding => Size - Padding * 2;

        public VisualAIBlockIcon(PrototypeId<AssetPrototype>? icon = null, Color? color = null, bool isSelected = true, float padding = DefaultPadding, float borderSize = 1)
        {
            Color = color ?? Color.White;
            _assetIcon = icon != null ? Assets.Get(icon.Value) : null;
            IsSelected = isSelected;
            Padding = padding;
            BorderSize = borderSize;
        }

        public override void GetPreferredSize(out Vector2 size)
        {
            size = new(DefaultSize, DefaultSize);
        }

        public override void Draw()
        {
            var size = SizeWithoutPadding;

            Love.Graphics.SetColor(Color.White);
            GraphicsS.RectangleS(UIScale, Love.DrawMode.Fill, X + Padding - BorderSize * 2, Y + Padding - BorderSize * 2, size.X + BorderSize * 4, size.Y + BorderSize * 4);
            Love.Graphics.SetColor(Color.Black);
            GraphicsS.RectangleS(UIScale, Love.DrawMode.Fill, X + Padding - BorderSize, Y + Padding - BorderSize, size.X + BorderSize * 2, size.Y + BorderSize * 2);
            Love.Graphics.SetColor(IsSelected ? Color : Color.Lighten(0.5f));
            GraphicsS.RectangleS(UIScale, Love.DrawMode.Fill, X + Padding, Y + Padding, size.X, size.Y);

            if (_assetIcon != null)
            {
                Love.Graphics.SetColor(IsSelected ? Color.White : Color.Gray);
                _assetIcon.Draw(UIScale, X + Padding - BorderSize * 4 + 1, Y + Padding - BorderSize * 4 + 1, size.X + BorderSize * 4, size.Y + BorderSize * 4);
            }
        }
    }
}
