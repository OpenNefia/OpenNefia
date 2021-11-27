using Love;
using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                this.KeyNameText = new UiText(this.FontListKeyName, keyName);
            }
        }

        public string Text
        {
            get => this.UiText.Text;
            set => this.UiText.Text = value;
        }

        [Localize(Key="Text")]
        protected IUiText UiText;

        protected IUiText KeyNameText = null!;

        public virtual string? LocalizeKey => null;

        public int XOffset { get; set; }

        protected AssetDrawable AssetListBullet;
        public AssetDrawable AssetSelectKey;
        protected FontDef FontListKeyName;
        public ColorDef ColorSelectedAdd;
        public ColorDef ColorSelectedSub;

        public UiListCell(T data, IUiText text, UiListChoiceKey? key = null)
        {
            this.Data = data;
            this.UiText = text;

            this.AssetSelectKey = new AssetDrawable(AssetDefOf.SelectKey);
            this.AssetListBullet = new AssetDrawable(AssetDefOf.ListBullet);
            this.FontListKeyName = FontDefOf.ListKeyName;
            this.ColorSelectedAdd = ColorDefOf.ListSelectedAdd;
            this.ColorSelectedSub = ColorDefOf.ListSelectedSub;

            this.Key = key;
        }

        public UiListCell(T data, string text, UiListChoiceKey? key = null) : this(data, new UiText(FontDefOf.ListText, text), key) {}

        public override void GetPreferredSize(out int width, out int height)
        {
            this.UiText.GetPreferredSize(out width, out height);
            width = width + this.AssetSelectKey.Width + 2 + 4 + this.XOffset;
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            this.UiText.SetPosition(x + this.AssetSelectKey.Width + 2 + 4 + this.XOffset, y);

            var keyNameX = x + (this.AssetSelectKey.Width - this.KeyNameText.Width) / 2 - 2;
            var keyNameY = y + (this.AssetSelectKey.Height - GraphicsEx.GetTextHeight()) / 2 - 1;
            this.KeyNameText.SetPosition(keyNameX, keyNameY);
        }

        public override void SetSize(int width = -1, int height = -1)
        {
            this.UiText.GetPreferredSize(out var textWidth, out var textHeight);
            this.UiText.SetSize(width - this.AssetSelectKey.Width - 6 + this.XOffset, textHeight);
            this.KeyNameText.SetPreferredSize();
            base.SetSize(Math.Max(width, textWidth + this.AssetSelectKey.Width + 2 + 4 + this.XOffset), height);
        }

        public virtual void DrawHighlight()
        {
            var width = Math.Clamp(this.UiText.TextWidth + this.AssetSelectKey.Width + 8 + this.XOffset, 10, 480);
            Love.Graphics.SetBlendMode(Love.BlendMode.Subtract);
            GraphicsEx.SetColor(this.ColorSelectedSub);
            GraphicsEx.FilledRect(this.UiText.X - 4, this.UiText.Y - 2, width, 19);
            Love.Graphics.SetBlendMode(Love.BlendMode.Add);
            GraphicsEx.SetColor(this.ColorSelectedAdd);
            GraphicsEx.FilledRect(this.UiText.X - 3, this.UiText.Y - 1, width - 2, 17);
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
            this.AssetSelectKey.Dispose();
            this.KeyNameText.Dispose();
            this.UiText.Dispose();
        }
    }
}
