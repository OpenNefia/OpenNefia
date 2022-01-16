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
        private int MaxWidth;
        public UiWrapText(int maxWidth, FontSpec font, string text = "") : base(font, text)
        {
            MaxWidth = maxWidth;
            Text = text;
        }

        public override string Text 
        { 
            get => base.Text; 
            set => base.Text = WordWrap(value, MaxWidth); 
        }

        // function from https://stackoverflow.com/questions/17586/best-word-wrap-algorithm
        private string WordWrap(string str, int width)
        {
            if (width <= 0)
                return base.Text;

            string[] words = UiHelpers.SplitString(str, Loc.Language);

            int curLineLength = 0;
            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < words.Length; i += 1)
            {
                string word = words[i];
                // If adding the new word to the current line would be too long,
                // then put it on a new line (and split it up if it's too long).
                if (curLineLength + Font.LoveFont.GetWidth(word) > width)
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
                    while (word.Length > width)
                    {
                        strBuilder.Append(word.Substring(0, width - 1) + "-");
                        word = word.Substring(width - 1);

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
    }
}
