using Love;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = OpenNefia.Core.Maths.Color;

namespace OpenNefia.Core.UI.Element.List
{
    public class UiListCell<T> : BaseUiElement, IUiListCell<T>
    {
        public T Data { get; set; }

        private UiListChoiceKey? _Key;
        public UiListChoiceKey? Key {
            get => _Key;
            set {
                _Key = value;
                var keyName = string.Empty;
                if (this.Key != null && this.Key.Key != Keys.None)
                {
                    keyName = UiUtils.GetKeyName(this.Key.Key);
                }
                this.KeyNameText = new UiTextOutlined(this.FontListKeyName, keyName);
            }
        }

        public string Text
        {
            get => this.UiText.Text;
            set => this.UiText.Text = value;
        }

        [Localize("Text")]
        protected IUiText UiText;

        protected IUiText KeyNameText = null!;

        public virtual string? LocalizeKey => null;

        public int XOffset { get; set; }

        protected FontSpec FontListKeyName = UiFonts.ListKeyName;
        public Color ColorSelectedAdd = UiColors.ListSelectedAdd;
        public Color ColorSelectedSub = UiColors.ListSelectedSub;

        protected IAssetDrawable AssetListBullet;
        public IAssetDrawable AssetSelectKey;

        public UiListCell(T data, IUiText text, UiListChoiceKey? key = null)
        {
            this.Data = data;
            this.UiText = text;

            this.AssetSelectKey = Assets.Get(AssetPrototypeOf.SelectKey);
            this.AssetListBullet = Assets.Get(AssetPrototypeOf.ListBullet);

            this.Key = key;
        }

        public UiListCell(T data, string text, UiListChoiceKey? key = null) : this(data, new UiText(UiFonts.ListText, text), key) {}

        public override void GetPreferredSize(out Vector2i size)
        {
            this.UiText.GetPreferredSize(out size);
            size.X = size.X + this.AssetSelectKey.Width + 2 + 4 + this.XOffset;
        }

        public override void SetSize(int width, int height)
        {
            this.UiText.GetPreferredSize(out var textSize);
            this.UiText.SetSize(textSize.X - this.AssetSelectKey.Width - 6 + this.XOffset, textSize.Y);
            this.KeyNameText.SetPreferredSize();
            base.SetSize(Math.Max(width, textSize.X + this.AssetSelectKey.Width + 2 + 4 + this.XOffset), height);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            this.UiText.SetPosition(x + this.AssetSelectKey.Width + 2 + 4 + this.XOffset, y);

            var keyNameX = x + (this.AssetSelectKey.Width - this.KeyNameText.Width) / 2 - 2;
            var keyNameY = y + (this.AssetSelectKey.Height - Love.Graphics.GetFont().GetHeight()) / 2 - 1;
            this.KeyNameText.SetPosition(keyNameX, keyNameY);
        }

        public virtual void DrawHighlight()
        {
            var width = Math.Clamp(this.UiText.TextWidth + this.AssetSelectKey.Width + 8 + this.XOffset, 10, 480);
            Love.Graphics.SetBlendMode(Love.BlendMode.Subtract);
            GraphicsEx.SetColor(this.ColorSelectedSub);
            Love.Graphics.Rectangle(Love.DrawMode.Fill, this.UiText.X - 4, this.UiText.Y - 2, width, 19);
            Love.Graphics.SetBlendMode(Love.BlendMode.Add);
            GraphicsEx.SetColor(this.ColorSelectedAdd);
            Love.Graphics.Rectangle(Love.DrawMode.Fill, this.UiText.X - 3, this.UiText.Y - 1, width - 2, 17);
            Love.Graphics.SetBlendMode(Love.BlendMode.Alpha);
            GraphicsEx.SetColor(Love.Color.White);
            this.AssetListBullet.Draw(this.UiText.X - 5 + width - 20, this.UiText.Y + 2);
        }

        public override void Draw()
        {
            GraphicsEx.SetColor(Love.Color.White);
            this.AssetSelectKey.Draw(this.X, this.Y - 1);
            this.KeyNameText.Draw();
            this.UiText.Draw();
        }

        public override void Update(float dt)
        {
            this.KeyNameText.Update(dt);
            this.UiText.Update(dt);
        }

        public override void Dispose()
        {
            this.KeyNameText.Dispose();
            this.UiText.Dispose();
        }
    }
}
