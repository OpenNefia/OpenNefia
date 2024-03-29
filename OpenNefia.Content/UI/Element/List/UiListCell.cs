﻿using OpenNefia.Core.Audio;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.Input;
using OpenNefia.Core.UI;

namespace OpenNefia.Content.UI.Element.List
{
    public class UiListCell<T> : UiListCell, IUiListCell<T>
    {
        private T _data;

        public T Data
        {
            get => _data;
            set
            {
                _data = value;
                OnCellDataChanged();
            }
        }

        public UiListCell(T data, UiText text, UiListChoiceKey? key = null) : base(text, key)
        {
            _data = data;
        }

        public UiListCell(T data, string text = "", UiListChoiceKey? key = null) 
            : this(data, new UiText(UiFonts.ListText, text), key)
        {
        }

        protected virtual void OnCellDataChanged()
        {
        }
    }

    public class UiListCell : UiElement, IUiListCell
    {

        private UiListChoiceKey? _Key;
        public UiListChoiceKey? Key
        {
            get => _Key;
            set
            {
                _Key = value;
                var keyName = string.Empty;
                if (Key != null && Key.Key != Core.Input.Keyboard.Key.Unknown)
                {
                    keyName = UiUtils.GetKeyName(Key.Key);
                }
                KeyNameText.Text = keyName;
            }
        }

        public string Text
        {
            get => UiText.Text;
            set => UiText.Text = value;
        }

        public int IndexInList { get; set; }

        [Child] [Localize("Text")] protected UiText UiText;
        [Child] protected UiText KeyNameText = null!;

        public virtual string? LocalizeKey => null;

        /// <summary>
        /// X offset of the text in virtual pixels.
        /// </summary>
        public float XOffset { get; set; }

        /// <summary>
        /// X offset of the text in physical pixels.
        /// </summary>
        public int PixelXOffset => (int)(XOffset * UIScale);

        /// <summary>
        /// X offset of the text in virtual pixels, counted from the left edge of the cell.
        /// </summary>
        public float XOffsetEdge => AssetSelectKey.PixelWidth + 2 + 4 + XOffset;

        protected FontSpec FontListKeyName = UiFonts.ListKeyName;
        public Color ColorSelectedAdd = UiColors.ListSelectedAdd;
        public Color ColorSelectedSub = UiColors.ListSelectedSub;

        protected IAssetInstance AssetListBullet;
        protected IAssetInstance AssetSelectKey;

        public UiListCell(UiText text, UiListChoiceKey? key = null)
        {
            UiText = text;
            KeyNameText = new UiTextOutlined(FontListKeyName);

            AssetSelectKey = Assets.Get(Protos.Asset.SelectKey);
            AssetListBullet = Assets.Get(Protos.Asset.ListBullet);

            Key = key;

            OnMouseEntered += HandleMouseEntered;
            EventFilter = UIEventFilterMode.Pass;
        }

        private void HandleMouseEntered(GUIMouseHoverEventArgs args)
        {
            if (Parent is not IUiList list)
                return;

            if (list.SelectedIndex != IndexInList)
            {
                Sounds.Play(Protos.Sound.Cursor1);
                list.Select(this.IndexInList);
            }
        }

        public override void GetPreferredSize(out Vector2 size)
        {
            UiText.GetPreferredSize(out size);
            size.X = size.X + XOffsetEdge;
        }

        public override void SetSize(float width, float height)
        {
            UiText.GetPreferredSize(out var textSize);
            UiText.SetSize(textSize.X - (AssetSelectKey.PixelWidth - 6 + XOffset), textSize.Y);
            KeyNameText.SetPreferredSize();
            base.SetSize(MathF.Max(width, textSize.X + XOffsetEdge), height);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            UiText.SetPosition(X + XOffsetEdge, Y);

            var keyNameX = X + (AssetSelectKey.PixelWidth - KeyNameText.Width) / 2 - 2;
            KeyNameText.SetPosition(keyNameX, Y - 1);
        }

        public virtual void DrawHighlight()
        {
            var virtualWidth = Math.Clamp(UiText.TextWidth + AssetSelectKey.PixelWidth + 8 + XOffset, 10, 480);
            Love.Graphics.SetBlendMode(Love.BlendMode.Subtract);
            GraphicsEx.SetColor(ColorSelectedSub);
            GraphicsS.RectangleS(UIScale, Love.DrawMode.Fill, UiText.X - XOffset - 4, UiText.Y - 1, virtualWidth, 19);
            Love.Graphics.SetBlendMode(Love.BlendMode.Add);
            GraphicsEx.SetColor(ColorSelectedAdd);
            GraphicsS.RectangleS(UIScale, Love.DrawMode.Fill, UiText.X - XOffset - 3, UiText.Y, virtualWidth - 2, 17);
            Love.Graphics.SetBlendMode(Love.BlendMode.Alpha);
            GraphicsEx.SetColor(Love.Color.White);
            AssetListBullet.Draw(UIScale, UiText.X - XOffset - 5 + virtualWidth - 20, UiText.Y + 3);
        }

        public virtual void DrawLineTint(float width)
        {
            if (IndexInList % 2 == 0)
            {
                Love.Graphics.SetColor(UiColors.ListEntryAccent);
                GraphicsS.RectangleS(UIScale, Love.DrawMode.Fill, X - 1, Y - 1, width, Height);
            }
        }

        public override void Draw()
        {
            GraphicsEx.SetColor(Love.Color.White);
            AssetSelectKey.Draw(UIScale, X, Y - 1);
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
