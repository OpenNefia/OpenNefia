using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.UI.Element
{
    public class UiWrapText : UiText
    {
        public UiWrapText(FontSpec font, string text = "") : base(font, text)
        {
            Text = text;
        }

        public override string Text 
        { 
            get => base.Text;
            set
            {
                base.Text = WordWrap(value, PixelWidth);
                SetPreferredSize();
            }
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            Text = Text;
        }

        static char[] splitChars = new char[] { ' ', '　' };

        // function from https://stackoverflow.com/questions/17586/best-word-wrap-algorithm
        private string WordWrap(string str, int pixelWidth)
        {
            if (pixelWidth <= 0)
                return base.Text;

            var locManager = IoCManager.Resolve<ILocalizationManager>();

            string[] words = SplitString(str, locManager.Language);

            int curLineLength = 0;
            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < words.Length; i += 1)
            {
                string word = words[i];
                // If adding the new word to the current line would be too long,
                // then put it on a new line (and split it up if it's too long).
                if (curLineLength + Font.LoveFont.GetWidth(word) > pixelWidth)
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
                curLineLength += Font.LoveFont.GetWidth(word);
            }

            return strBuilder.ToString();
        }

        private string[] SplitStringEn(string str, char[] splitChars)
        {
            List<string> parts = new List<string>();
            int startIndex = 0;
            while (true)
            {
                int index = str.IndexOfAny(splitChars, startIndex);

                if (index == -1)
                {
                    parts.Add(str.Substring(startIndex));
                    return parts.ToArray();
                }

                string word = str.Substring(startIndex, index - startIndex);
                char nextChar = str.Substring(index, 1)[0];
                // Dashes and the likes should stick to the word occuring before it. Whitespace doesn't have to.
                if (char.IsWhiteSpace(nextChar))
                {
                    parts.Add(word);
                    parts.Add(nextChar.ToString());
                }
                else
                {
                    parts.Add(word + nextChar);
                }

                startIndex = index + 1;
            }
        }

        // function from https://stackoverflow.com/questions/17586/best-word-wrap-algorithm
        private string[] SplitString(string str, PrototypeId<LanguagePrototype> lang)
        {
            switch (lang)
            {
                case var en when en == LanguagePrototypeOf.English:
                    return SplitStringEn(str, splitChars);
                default:
                    return str.Select(x => $"{x}").ToArray();
            }
        }
    }
}
