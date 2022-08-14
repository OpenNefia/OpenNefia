using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Core.Input.Keyboard;

namespace OpenNefia.Core.Input
{
    public partial class InputManager
    {
        [Dependency] private readonly ILocalizationManager _loc = default!;

        public string GetKeyName(Key key)
        {
            if (_printableKeyNameMap.TryGetValue(key, out var name))
            {
                return name;
            }

            if (Keyboard.TryGetSpecialKeyName(key, _loc, out name))
                return name;

            return _loc.GetString("OpenNefia.Input.Keys.Unknown");
        }

        private static readonly Dictionary<Key, string> _printableKeyNameMap = new()
        {
            { Key.Exclaim, "!" },
            { Key.Quotedbl, "\"" },
            { Key.Hash, "#" },
            { Key.Percent, "%" },
            { Key.Dollar, "$" },
            { Key.Ampersand, "&" },
            { Key.Quote, "'" },
            { Key.LeftParen, "(" },
            { Key.RightParen, ")" },
            { Key.Asterisk, "*" },
            { Key.Plus, "+" },
            { Key.Comma, "," },
            { Key.Minus, "-" },
            { Key.Period, "." },
            { Key.Slash, "/" },
            { Key.Number0, "0" },
            { Key.Number1, "1" },
            { Key.Number2, "2" },
            { Key.Number3, "3" },
            { Key.Number4, "4" },
            { Key.Number5, "5" },
            { Key.Number6, "6" },
            { Key.Number7, "7" },
            { Key.Number8, "8" },
            { Key.Number9, "9" },
            { Key.Colon, ":" },
            { Key.SemiColon, ";" },
            { Key.Less, "<" },
            { Key.Equals, "=" },
            { Key.Greater, ">" },
            { Key.Question, "?" },
            { Key.At, "@" },

            { Key.LeftBracket, "[" },
            { Key.Backslash, "\\" },
            { Key.RightBracket, "]" },
            { Key.Caret, "^" },
            { Key.Underscore, "_" },
            { Key.Backquote, "`" },
            { Key.A, "a" },
            { Key.B, "b" },
            { Key.C, "c" },
            { Key.D, "d" },
            { Key.E, "e" },
            { Key.F, "f" },
            { Key.G, "g" },
            { Key.H, "h" },
            { Key.I, "i" },
            { Key.J, "j" },
            { Key.K, "k" },
            { Key.L, "l" },
            { Key.M, "m" },
            { Key.N, "n" },
            { Key.O, "o" },
            { Key.P, "p" },
            { Key.Q, "q" },
            { Key.R, "r" },
            { Key.S, "s" },
            { Key.T, "t" },
            { Key.U, "u" },
            { Key.V, "v" },
            { Key.W, "w" },
            { Key.X, "x" },
            { Key.Y, "y" },
            { Key.Z, "z" },

            { Key.F1, "F1" },
            { Key.F2, "F2" },
            { Key.F3, "F3" },
            { Key.F4, "F4" },
            { Key.F5, "F5" },
            { Key.F6, "F6" },
            { Key.F7, "F7" },
            { Key.F8, "F8" },
            { Key.F9, "F9" },
            { Key.F10, "F10" },
            { Key.F11, "F11" },
            { Key.F12, "F12" },

            { Key.NumpadDivide, "Numpad/" },
            { Key.NumpadMultiply, "Numpad*" },
            { Key.NumpadMinus, "Numpad-" },
            { Key.NumpadPlus, "Numpad+" },
            { Key.Numpad0, "Numpad0" },
            { Key.Numpad1, "Numpad1" },
            { Key.Numpad2, "Numpad2" },
            { Key.Numpad3, "Numpad3" },
            { Key.Numpad4, "Numpad4" },
            { Key.Numpad5, "Numpad5" },
            { Key.Numpad6, "Numpad6" },
            { Key.Numpad7, "Numpad7" },
            { Key.Numpad8, "Numpad8" },
            { Key.Numpad9, "Numpad9" },
            { Key.NumpadPeriod, "Numpad." },
            { Key.NumpadComma, "Numpad," },
            { Key.NumpadEquals, "Numpad=" },

            { Key.F13, "F13" },
            { Key.F14, "F14" },
            { Key.F15, "F15" },
            { Key.F16, "F16" },
            { Key.F17, "F17" },
            { Key.F18, "F18" },
            { Key.F19, "F19" },
            { Key.F20, "F20" },
            { Key.F21, "F21" },
            { Key.F22, "F22" },
            { Key.F23, "F23" },
            { Key.F24, "F24" },

			// ちょっと変わった様子のキーたち。

			//{ Key.Execute, "execute" },
			//{ Key.Help, "help" },
			//{ Key.Menu, "menu" },
			//{ Key.Select, "select" },
			//{ Key.Stop, "stop" },
			//{ Key.Again, "again" },
			//{ Key.Undo, "undo" },
			//{ Key.Cut, "cut" },
			//{ Key.Copy, "copy" },
			//{ Key.Paste, "paste" },
			//{ Key.Find, "find" },
			//{ Key.Mute, "mute" },
			//{ Key.VolumeUp, "volumeup" },
			//{ Key.VolumeDown, "volumedown" },

			//{ Key.Alterase, "alterase" },
			//{ Key.Sysreq, "sysreq" },
			//{ Key.Cancel, "cancel" },
			//{ Key.Clear, "clear" },
			//{ Key.Prior, "prior" },
			//{ Key.Return2, "return2" },
			//{ Key.Separator, "separator" },
			//{ Key.Out, "out" },
			//{ Key.Oper, "oper" },
			//{ Key.ClearAgain, "clearagain" },

			//{ Key.ThousandsSeparator, "thsousandsseparator" },
			//{ Key.DecimalSeparator, "decimalseparator" },
			//{ Key.CurrencyUnit, "currencyunit" },
			//{ Key.CurrencySubunit, "currencysubunit" },

			//{ Key.Mode, "mode" },

			//{ Key.AudioNext, "audionext" },
			//{ Key.AudioPrev, "audioprev" },
			//{ Key.AudioStop, "audiostop" },
			//{ Key.AudioPlay, "audioplay" },
			//{ Key.AudioMute, "audiomute" },
			//{ Key.MediaSelect, "mediaselect" },
			//{ Key.WWW, "www" },
			//{ Key.Mail, "mail" },
			//{ Key.Calculator, "calculator" },
			//{ Key.Computer, "computer" },
			//{ Key.AppSearch, "appsearch" },
			//{ Key.AppHome, "apphome" },
			//{ Key.AppBack, "appback" },
			//{ Key.AppForward, "appforward" },
			//{ Key.AppStop, "appstop" },
			//{ Key.AppRefresh, "apprefresh" },
			//{ Key.AppBookmarks, "appbookmarks" },

			//{ Key.BrightnessDown, "brightnessdown" },
			//{ Key.BrightnessUp, "brightnessup" },
			//{ Key.DisplaySwitch, "displayswitch" },
			//{ Key.KBDILLUMToggle, "kbdillumtoggle" },
			//{ Key.KBDILLUMDown, "kbdillumdown" },
			//{ Key.KBDILLUMUp, "kbdillumup" },
			//{ Key.Eject, "eject" },
			//{ Key.Sleep, "sleep" },
		};
    }
}
