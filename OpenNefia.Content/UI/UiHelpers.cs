using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.UI
{
    public static class UiHelpers
    {
        private static char[] splitChars = new char[] { ' ', '　' };

        public static string[] SplitString(string str, PrototypeId<LanguagePrototype> lang)
        {
            switch (lang)
            {
                case var jp when jp == LanguagePrototypeOf.Japanese:
                    return str.Select(x => $"{x}").ToArray();
                default:
                    return SplitStringDefault(str, splitChars);
            }
        }

        private static string[] SplitStringDefault(string str, char[] splitChars)
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
    }
}
