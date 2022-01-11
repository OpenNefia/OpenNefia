using OpenNefia.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.UI
{
    public static class UiKeyHints
    {
        #pragma warning disable format
        public static readonly LocaleKey Back       = new($"Elona.UI.KeyHints.{nameof(Back)}");
        public static readonly LocaleKey Change     = new($"Elona.UI.KeyHints.{nameof(Change)}");
        public static readonly LocaleKey SwitchMenu = new($"Elona.UI.KeyHints.{nameof(SwitchMenu)}");
        public static readonly LocaleKey Close      = new($"Elona.UI.KeyHints.{nameof(Close)}");
        public static readonly LocaleKey Confirm    = new($"Elona.UI.KeyHints.{nameof(Confirm)}");
        public static readonly LocaleKey Help       = new($"Elona.UI.KeyHints.{nameof(Help)}");
        public static readonly LocaleKey KnownInfo  = new($"Elona.UI.KeyHints.{nameof(KnownInfo)}");
        public static readonly LocaleKey Mode       = new($"Elona.UI.KeyHints.{nameof(Mode)}");
        public static readonly LocaleKey Page       = new($"Elona.UI.KeyHints.{nameof(Page)}");
        public static readonly LocaleKey Portrait   = new($"Elona.UI.KeyHints.{nameof(Portrait)}");
        public static readonly LocaleKey Select     = new($"Elona.UI.KeyHints.{nameof(Select)}");
        public static readonly LocaleKey Shortcut   = new($"Elona.UI.KeyHints.{nameof(Shortcut)}");
        #pragma warning restore format
    }

    public static class UiKeyNames
    {
        #pragma warning disable format
        public static readonly LocaleKey Cancel    = new($"Elona.UI.KeyNames.{nameof(Cancel)}");
        public static readonly LocaleKey Cursor    = new($"Elona.UI.KeyNames.{nameof(Cursor)}");
        public static readonly LocaleKey EnterKey  = new($"Elona.UI.KeyNames.{nameof(EnterKey)}");
        public static readonly LocaleKey LeftRight = new($"Elona.UI.KeyNames.{nameof(LeftRight)}");
        public static readonly LocaleKey Shortcut  = new($"Elona.UI.KeyNames.{nameof(Shortcut)}");
        #pragma warning restore format
    }
}
