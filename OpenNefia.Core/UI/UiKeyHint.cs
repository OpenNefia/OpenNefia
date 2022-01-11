using OpenNefia.Core.Input;
using OpenNefia.Core.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI
{
    public sealed class UiKeyHint
    {
        /// <summary>
        /// Name of the action for the key hint ("Identify", "Known Info", etc.)
        /// </summary>
        public string ActionText { get; set; } = string.Empty;

        /// <summary>
        /// Bound key functions for this action. This is where the actual key names will
        /// be generated from.
        /// </summary>
        public BoundKeyFunction[] KeyFunctions { get; set; }

        /// <summary>
        /// Keybind names. This overrides the names generated from looking at <see cref="KeyFunctions"/>
        /// if it's set.
        /// </summary>
        public string? KeybindNamesText { get; set; }

        public UiKeyHint(string text, BoundKeyFunction[] functions)
        {
            ActionText = text;
            KeyFunctions = functions;
        }

        public UiKeyHint(LocaleKey localeKey, BoundKeyFunction[] functions)
            : this(Loc.GetString(localeKey), functions)
        {
        }

        public UiKeyHint(string text, BoundKeyFunction function)
            : this(text, new BoundKeyFunction[] { function })
        {
        }

        public UiKeyHint(LocaleKey localeKey, BoundKeyFunction function)
            : this(Loc.GetString(localeKey), new BoundKeyFunction[] { function })
        {
        }

        public UiKeyHint(LocaleKey localeKey, LocaleKey keyNamesLocaleKey)
            : this(Loc.GetString(localeKey), Array.Empty<BoundKeyFunction>())
        {
            KeybindNamesText = Loc.GetString(keyNamesLocaleKey);
        }
    }
}
