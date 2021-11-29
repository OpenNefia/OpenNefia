using OpenNefia.Core.Audio;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI.Layer
{
    public class TextPrompt : BaseUiLayer<string>
    {
        private string _Value;
        public string Value
        {
            get => _Value;
            set
            {
                _Value = value;
                this.UpdateText();
            }
        }

        public bool IsCancellable { get; set; }
        public bool LimitLength { get; set; }
        public int? MaxLength { get; set; }
        public bool HasShadow { get; set; }

        protected bool IsCutOff = false;
        protected float Dt = 0f;
        protected double CaretAlpha = 2;

        protected UiTopicWindow TopicWindow;
        protected IUiText Text;

        protected AssetDrawable AssetLabelInput;
        protected AssetDrawable AssetImeStatusJapanese;
        protected AssetDrawable AssetImeStatusEnglish;
        protected AssetDrawable AssetImeStatusNone;
        protected AssetDrawable AssetInputCaret;
        
        [UiStyled] protected Color ColorPromptBackground;
        [UiStyled] protected FontSpec FontPromptText = new();

        public TextPrompt(int? maxLength = 16, bool limitLength = false, string? initialValue = null, bool isCancellable = true, bool hasShadow = true)
        {
            if (initialValue == null)
                initialValue = string.Empty;

            this.MaxLength = maxLength;
            this.LimitLength = limitLength;
            this._Value = initialValue;
            this.IsCancellable = isCancellable;
            this.HasShadow = hasShadow;

            this.AssetLabelInput = Assets.Get(AssetPrototypeOf.LabelInput);
            this.AssetImeStatusJapanese = Assets.Get(AssetPrototypeOf.ImeStatusJapanese);
            this.AssetImeStatusEnglish = Assets.Get(AssetPrototypeOf.ImeStatusEnglish);
            this.AssetImeStatusNone = Assets.Get(AssetPrototypeOf.ImeStatusNone);
            this.AssetInputCaret = Assets.Get(AssetPrototypeOf.InputCaret);

            this.TopicWindow = new UiTopicWindow(UiTopicWindow.FrameStyleKind.Zero, UiTopicWindow.WindowStyleKind.Two);
            this.Text = new UiText(this.FontPromptText);

            this.UpdateText();

            this.BindKeys();
        }

        protected virtual void BindKeys()
        {
            this.TextInput.Enabled = true;
            this.TextInput.Callback += (evt) => {
                this.Value = this.Value + evt.Text;
                this.UpdateText();
            };
            this.Keybinds[CoreKeybinds.Enter] += (_) => this.Finish(this.Value);
            this.Keybinds[CoreKeybinds.Escape] += (_) => {
                if (this.IsCancellable)
                    this.Cancel();
            };
            this.Keybinds[Keys.Backspace] += (_) => {
                if (this.Value.Length > 0)
                {
                    this.Value = this.Value.Remove(this.Value.Length - 1, 1);
                    this.UpdateText();
                }
            };
        }

        public override void OnQuery()
        {
            Sounds.Play(SoundPrototypeOf.Pop2);
        }

        protected virtual void UpdateText()
        {
            this.IsCutOff = false;
            string displayText = string.Empty;
            var wideLength = this.Value.GetWideLength();

            if (this.MaxLength.HasValue)
            {
                if (this.MaxLength.HasValue && wideLength > this.MaxLength.Value - 2)
                {
                    var dots = "...";
                    if (Loc.IsFullwidth())
                    {
                        dots = "…";
                    }
                    displayText = this.Value.WideSubstring(0, this.MaxLength.Value - 2) + dots;
                }
                else
                {
                    displayText = this.Value;
                }
            }
            else
            {
                displayText = this.Value;
            }

            this.Text.Text = displayText;

            if (this.MaxLength.HasValue && wideLength > this.MaxLength)
            {
                this._Value = this.Value.WideSubstring(0, this.MaxLength);
                this.IsCutOff = true;
            }
        }

        public const int DEFAULT_WIDTH = 16 * 16 + 60;
        public const int DEFAULT_HEIGHT = 36;
        
        public override void GetPreferredBounds(out Box2i bounds)
        {
            UiUtils.GetCenteredParams(DEFAULT_WIDTH, DEFAULT_HEIGHT, out bounds);
        }

        public override void SetSize(Vector2i size)
        {
            size.X = Math.Max(size.X, this.FontPromptText.LoveFont.GetWidth(this.Value));

            base.SetSize(size);

            this.TopicWindow.SetSize(this.Width, this.Height);
            this.Text.SetPreferredSize();
        }

        public override void SetPosition(Vector2i pos)
        {
            base.SetPosition(pos);

            this.TopicWindow.SetPosition(this.Left, this.Top);
            this.Text.SetPosition(this.Left + 36, this.Top + 9);
        }

        public override void Update(float dt)
        {
            this.Dt += (dt / Constants.SCREEN_REFRESH) * 4;
            this.CaretAlpha = Math.Sin(this.Dt) * 255f * 1f;
            this.TopicWindow.Update(dt);
            this.Text.Update(dt);
        }

        public override void Draw()
        {
            if (this.HasShadow)
            {
                GraphicsEx.SetColor(this.ColorPromptBackground);
                Love.Graphics.Rectangle(Love.DrawMode.Fill, this.Left + 4, this.Top + 4, this.Width - 1, this.Height - 1);
            }

            this.TopicWindow.Draw();

            GraphicsEx.SetColor(Love.Color.White);
            this.AssetLabelInput.Draw(this.Left + this.Width / 2 - 60, this.Top - 32);

            if (this.IsCutOff)
            {
                this.AssetImeStatusNone.Draw(this.Left + 8, this.Top + 4);
            }
            else
            {
                this.AssetImeStatusEnglish.Draw(this.Left + 8, this.Top + 4);
            }

            this.Text.Draw();

            GraphicsEx.SetColor(255, 255, 255, (int)this.CaretAlpha);
            this.AssetInputCaret.Draw(this.Left + this.Text.Width + 34, this.Top + 5);
        }

        public override void Dispose()
        {
            this.AssetLabelInput.Dispose();
            this.AssetImeStatusEnglish.Dispose();
            this.AssetImeStatusJapanese.Dispose();
            this.AssetImeStatusNone.Dispose();
            this.AssetInputCaret.Dispose();
            this.Text.Dispose();
            this.TopicWindow.Dispose();
        }
    }
}
