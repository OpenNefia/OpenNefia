using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Rendering;
using static OpenNefia.Content.Prototypes.Protos;
using OpenNefia.Core;
using OpenNefia.Core.Maths;

namespace OpenNefia.Content.CharaMake
{
    public class CharaMakeCaption : UiElement
    {
        public string Text 
        {
            get => TextCaption.Text;
            set
            {
                TextCaption.Text = value;
                SetPreferredSize();
            }
        }

        public UiText TextCaption { get; }

        private IAssetInstance? AssetCaption;

        public CharaMakeCaption()
        {
            TextCaption = new UiTextOutlined(UiFonts.WindowTitle);
        }

        public override void Localize(LocaleKey key)
        {
            TextCaption.Text = Loc.GetString(key);
        }

        public override void GetPreferredSize(out Vector2 size)
        {
            size = new(760, 24);
        }

        public override void SetSize(float width, float height)
        {
            TextCaption.SetPreferredSize();
            width = MathF.Min(TextCaption.TextWidth + 45, width);

            base.SetSize(width, height);

            AssetCaption?.Dispose();
            AssetCaption = Assets.GetSized(Asset.Caption, PixelSize);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            TextCaption.SetPosition(X + 18, Y + 4);
        }

        public override void Update(float dt)
        {
            TextCaption.Update(dt);
        }

        public override void Draw()
        {
            var captionWidth = AssetCaption!.VirtualWidth(UIScale);
            var count = MathF.Ceiling((float)Width / captionWidth);

            Love.Graphics.SetColor(Color.White);

            for (int i = 0; i < count; i++)
            {
                int regionOffset;

                if (i == count - 1)
                {
                    regionOffset = 3;
                }
                else
                {
                    regionOffset = 0;
                }

                AssetCaption.DrawRegionS(UIScale, (0 + regionOffset).ToString(), i * captionWidth + X, Y);
                AssetCaption.DrawRegionS(UIScale, (1 + regionOffset).ToString(), i * captionWidth + X, Y + 2);
                AssetCaption.DrawRegionS(UIScale, (2 + regionOffset).ToString(), i * captionWidth + X, Y + 22);
            }

            TextCaption.Draw();
        }
    }
}