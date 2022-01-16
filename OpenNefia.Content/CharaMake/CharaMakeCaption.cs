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

        public override void GetPreferredSize(out Vector2i size)
        {
            size = new(760, 24);
        }

        public override void SetSize(int width, int height)
        {
            TextCaption.SetPreferredSize();
            width = Math.Min(TextCaption.TextWidth + 45, width);

            base.SetSize(width, height);

            AssetCaption?.Dispose();
            AssetCaption = Assets.GetSized(Asset.Caption, PixelSize);
        }

        public override void SetPosition(int x, int y)
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
            var count = MathF.Ceiling((float)Width / AssetCaption!.Width);

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

                AssetCaption.DrawRegion((0 + regionOffset).ToString(), i * AssetCaption.Width + X, Y);
                AssetCaption.DrawRegion((1 + regionOffset).ToString(), i * AssetCaption.Width + X, Y + 2);
                AssetCaption.DrawRegion((2 + regionOffset).ToString(), i * AssetCaption.Width + X, Y + 22);
            }

            TextCaption.Draw();
        }
    }
}