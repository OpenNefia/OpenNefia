using OpenNefia.Core.Locale;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.RandomText
{
    public static class RandomTextHelpers
    {
        public static string CapitalizeTitleText(string text, ILocalizationManager localeMan)
        {
            if (localeMan.Language != LanguagePrototypeOf.English || string.IsNullOrEmpty(text))
                return text;

            if (text[0] == '*')
            {
                if (text.Length == 1)
                    return text;

                text = text.Substring(1);
            }

            return text.FirstCharToUpper();
        }
    }
}
