﻿using Love;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using Color = OpenNefia.Core.Maths.Color;

namespace OpenNefia.Content.UI.Element.List
{
    public class UiListCell<T> : BaseUiElement, IUiListCell<T>
    {
        public T Data { get; set; }

        private UiListChoiceKey? _Key;
        public UiListChoiceKey? Key
        {
            get => _Key;
            set
            {
                _Key = value;
                var keyName = string.Empty;
                if (Key != null && Key.Key != Keys.None)
                {
                    keyName = UiUtils.GetKeyName(Key.Key);
                }
                KeyNameText = new UiTextOutlined(FontListKeyName, keyName);
            }
        }

        public string Text
        {
            get => UiText.Text;
            set => UiText.Text = value;
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
            Data = data;
            UiText = text;

            AssetSelectKey = Assets.Get(AssetPrototypeOf.SelectKey);
            AssetListBullet = Assets.Get(AssetPrototypeOf.ListBullet);

            Key = key;
        }

        public UiListCell(T data, string text, UiListChoiceKey? key = null) : this(data, new UiText(UiFonts.ListText, text), key) { }

        public override void GetPreferredSize(out Vector2i size)
        {
            UiText.GetPreferredSize(out size);
            size.X = size.X + AssetSelectKey.Width + 2 + 4 + XOffset;
        }

        public override void SetSize(int width, int height)
        {
            UiText.GetPreferredSize(out var textSize);
            UiText.SetSize(textSize.X - AssetSelectKey.Width - 6 + XOffset, textSize.Y);
            KeyNameText.SetPreferredSize();
            base.SetSize(Math.Max(width, textSize.X + AssetSelectKey.Width + 2 + 4 + XOffset), height);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            UiText.SetPosition(x + AssetSelectKey.Width + 2 + 4 + XOffset, y);

            var keyNameX = x + (AssetSelectKey.Width - KeyNameText.Width) / 2 - 2;
            var keyNameY = y + (AssetSelectKey.Height - Graphics.GetFont().GetHeight()) / 2 - 1;
            KeyNameText.SetPosition(keyNameX, keyNameY);
        }

        public virtual void DrawHighlight()
        {
            var width = Math.Clamp(UiText.TextWidth + AssetSelectKey.Width + 8 + XOffset, 10, 480);
            Graphics.SetBlendMode(BlendMode.Subtract);
            GraphicsEx.SetColor(ColorSelectedSub);
            Graphics.Rectangle(DrawMode.Fill, UiText.X - 4, UiText.Y - 2, width, 19);
            Graphics.SetBlendMode(BlendMode.Add);
            GraphicsEx.SetColor(ColorSelectedAdd);
            Graphics.Rectangle(DrawMode.Fill, UiText.X - 3, UiText.Y - 1, width - 2, 17);
            Graphics.SetBlendMode(BlendMode.Alpha);
            GraphicsEx.SetColor(Love.Color.White);
            AssetListBullet.Draw(UiText.X - 5 + width - 20, UiText.Y + 2);
        }

        public override void Draw()
        {
            GraphicsEx.SetColor(Love.Color.White);
            AssetSelectKey.Draw(X, Y - 1);
            KeyNameText.Draw();
            UiText.Draw();
        }

        public override void Update(float dt)
        {
            KeyNameText.Update(dt);
            UiText.Update(dt);
        }

        public override void Dispose()
        {
            KeyNameText.Dispose();
            UiText.Dispose();
        }
    }
}
