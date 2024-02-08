using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using System.Text;

namespace OpenNefia.Content.UI.Element
{
    public class UiWrappedText : UiElement
    {
        [Child] public UiText UiText { get; }

        public UiWrappedText(FontSpec font, string text = "")
        {
            UiText = new UiText(font, text);
            _originalText = text;
            WrappedText = WordWrap(OriginalText, PixelWidth);
        }

        public UiWrappedText(UiText textElem)
        {
            UiText = textElem;
            _originalText = textElem.Text;
            WrappedText = WordWrap(OriginalText, PixelWidth);
        }

        private string _originalText;
        public string OriginalText
        {
            get => _originalText;
            set
            {
                _originalText = value;
                WrappedText = WordWrap(OriginalText, PixelWidth);
            }
        }

        public string WrappedText
        {
            get => UiText.Text;
            private set => UiText.Text = value;
        }

        public void WrapAt(int pixelWidth)
        {
            WrappedText = WordWrap(OriginalText, pixelWidth);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            WrappedText = WordWrap(OriginalText, PixelWidth);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            UiText.SetPosition(X, Y);
        }

        public override void Update(float dt)
        {
            UiText.Update(dt);
        }

        public override void Draw()
        {
            UiText.Draw();
        }

        public override void Dispose()
        {
            UiText.Dispose();
        }

        // function from https://stackoverflow.com/questions/17586/best-word-wrap-algorithm
        private string WordWrap(string str, int pixelWidth)
        {
            if (pixelWidth <= 0)
                return OriginalText;

            string[] words = UiHelpers.SplitString(str, Loc.Language);

            int curLineLength = 0;
            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < words.Length; i += 1)
            {
                string word = words[i];
                // If adding the new word to the current line would be too long,
                // then put it on a new line (and split it up if it's too long).
                if (curLineLength + UiText.Font.LoveFont.GetWidth(word) > pixelWidth)
                {
                    // Only move down to a new line if we have text on the current line.
                    // Avoids situation where wrapped whitespace causes emptylines in text.
                    if (curLineLength > 0)
                    {
                        strBuilder.Append(Environment.NewLine);
                        curLineLength = 0;
                    }

                    // If the current word is too long to fit on a line even on it's own then
                    // split the word up.
                    while (word.Length > pixelWidth)
                    {
                        strBuilder.Append(word.Substring(0, pixelWidth - 1) + "-");
                        word = word.Substring(pixelWidth - 1);

                        strBuilder.Append(Environment.NewLine);
                    }

                    // Remove leading whitespace from the word so the new line starts flush to the left.
                    word = word.TrimStart();
                }
                strBuilder.Append(word);
                curLineLength += UiText.Font.LoveFont.GetWidth(word);
            }

            return strBuilder.ToString();
        }
    }
}
