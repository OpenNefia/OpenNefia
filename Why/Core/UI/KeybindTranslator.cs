using OpenNefia.Core.Data.Types;
using System;
using System.Collections.Generic;

namespace OpenNefia.Core.UI
{
    public class KeybindTranslator
    {
        private Dictionary<Keys, IKeybind> Translations;
        private HashSet<IKeybind> AcceptedKeybinds;
        private bool Dirty;

        public KeybindTranslator()
        {
            this.Translations = new Dictionary<Keys, IKeybind>();
            this.AcceptedKeybinds = new HashSet<IKeybind>();
            this.Dirty = true;
        }

        internal void Enable(IKeybind keybind)
        {
            this.AcceptedKeybinds.Add(keybind);
            this.Dirty = true;
        }

        internal void Disable(IKeybind keybind)
        {
            this.AcceptedKeybinds.Remove(keybind);
            this.Dirty = true;
        }

        public void Reload()
        {
            this.BindKey(Keys.Return, Keybind.Entries.Enter);
            this.BindKey(Keys.Shift, Keybind.Entries.Cancel);
            this.BindKey(Keys.Escape, Keybind.Entries.Escape);
            this.BindKey(Keys.Escape, Keybind.Entries.Quit);

            this.BindKey(Keys.Up, Keybind.Entries.UIUp);
            this.BindKey(Keys.Down, Keybind.Entries.UIDown);
            this.BindKey(Keys.Left, Keybind.Entries.UILeft);
            this.BindKey(Keys.Right, Keybind.Entries.UIRight);

            this.BindKey(Keys.Up, Keybind.Entries.North);
            this.BindKey(Keys.Down, Keybind.Entries.South);
            this.BindKey(Keys.Left, Keybind.Entries.West);
            this.BindKey(Keys.Right, Keybind.Entries.East);

            this.BindKey(Keys.Period, Keybind.Entries.Wait);
            this.BindKey(Keys.X, Keybind.Entries.Identify);
            this.BindKey(Keys.Z, Keybind.Entries.Mode);
            this.BindKey(Keys.KeypadMultiply, Keybind.Entries.Mode2);

            this.BindKey(Keys.A, Keybind.Entries.SelectionA);
            this.BindKey(Keys.B, Keybind.Entries.SelectionB);
            this.BindKey(Keys.C, Keybind.Entries.SelectionC);
            this.BindKey(Keys.D, Keybind.Entries.SelectionD);
            this.BindKey(Keys.E, Keybind.Entries.SelectionE);
            this.BindKey(Keys.F, Keybind.Entries.SelectionF);
            this.BindKey(Keys.G, Keybind.Entries.SelectionG);
            this.BindKey(Keys.H, Keybind.Entries.SelectionH);
            this.BindKey(Keys.I, Keybind.Entries.SelectionI);
            this.BindKey(Keys.J, Keybind.Entries.SelectionJ);
            this.BindKey(Keys.K, Keybind.Entries.SelectionK);
            this.BindKey(Keys.L, Keybind.Entries.SelectionL);
            this.BindKey(Keys.M, Keybind.Entries.SelectionM);
            this.BindKey(Keys.N, Keybind.Entries.SelectionN);
            this.BindKey(Keys.O, Keybind.Entries.SelectionO);
            this.BindKey(Keys.P, Keybind.Entries.SelectionP);
            this.BindKey(Keys.Q, Keybind.Entries.SelectionQ);
            this.BindKey(Keys.R, Keybind.Entries.SelectionR);

            this.BindKey(Keys.Backquote, Keybind.Entries.Repl);

            this.Dirty = false;
        }

        public void BindKey(Keys keyAndModifiers, Keybind keybind)
        {
            if (this.AcceptedKeybinds.Contains(keybind))
            {
                this.Translations[keyAndModifiers] = keybind;
            }
        }

        public IKeybind? KeyToKeybind(Keys keyAndModifiers)
        {
            if (this.Dirty)
            {
                // Get the list of all key bindings and bind only the ones that have actions set.
                this.Reload();
            }

            if (this.Translations.TryGetValue(keyAndModifiers, out IKeybind? keybind))
            {
                return keybind;
            }

            if (RawKey.AllKeys.TryGetValue(keyAndModifiers, out RawKey? rawKey))
            {
                return rawKey;
            }

            return null;
        }
    }
}
