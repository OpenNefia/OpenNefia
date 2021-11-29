using OpenNefia.Core.Data.Types;
using KeybindPrototypeID = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Core.UI.KeybindPrototype>;

namespace OpenNefia.Core.UI
{
    public class KeybindTranslator
    {
        private Dictionary<Keys, KeybindPrototypeID> Translations;
        private HashSet<KeybindPrototypeID> AcceptedKeybinds;
        private bool Dirty;

        public KeybindTranslator()
        {
            this.Translations = new Dictionary<Keys, KeybindPrototypeID>();
            this.AcceptedKeybinds = new HashSet<KeybindPrototypeID>();
            this.Dirty = true;
        }

        internal void Enable(KeybindPrototypeID keybind)
        {
            this.AcceptedKeybinds.Add(keybind);
            this.Dirty = true;
        }

        internal void Disable(KeybindPrototypeID keybind)
        {
            this.AcceptedKeybinds.Remove(keybind);
            this.Dirty = true;
        }

        public void Reload()
        {
            this.BindKey(Keys.Return, Keybind.Enter);
            this.BindKey(Keys.Shift, Keybind.Cancel);
            this.BindKey(Keys.Escape, Keybind.Escape);
            this.BindKey(Keys.Escape, Keybind.Quit);

            this.BindKey(Keys.Up, Keybind.UIUp);
            this.BindKey(Keys.Down, Keybind.UIDown);
            this.BindKey(Keys.Left, Keybind.UILeft);
            this.BindKey(Keys.Right, Keybind.UIRight);

            this.BindKey(Keys.Up, Keybind.North);
            this.BindKey(Keys.Down, Keybind.South);
            this.BindKey(Keys.Left, Keybind.West);
            this.BindKey(Keys.Right, Keybind.East);

            this.BindKey(Keys.Period, Keybind.Wait);
            this.BindKey(Keys.X, Keybind.Identify);
            this.BindKey(Keys.Z, Keybind.Mode);
            this.BindKey(Keys.KeypadMultiply, Keybind.Mode2);

            this.BindKey(Keys.A, Keybind.SelectionA);
            this.BindKey(Keys.B, Keybind.SelectionB);
            this.BindKey(Keys.C, Keybind.SelectionC);
            this.BindKey(Keys.D, Keybind.SelectionD);
            this.BindKey(Keys.E, Keybind.SelectionE);
            this.BindKey(Keys.F, Keybind.SelectionF);
            this.BindKey(Keys.G, Keybind.SelectionG);
            this.BindKey(Keys.H, Keybind.SelectionH);
            this.BindKey(Keys.I, Keybind.SelectionI);
            this.BindKey(Keys.J, Keybind.SelectionJ);
            this.BindKey(Keys.K, Keybind.SelectionK);
            this.BindKey(Keys.L, Keybind.SelectionL);
            this.BindKey(Keys.M, Keybind.SelectionM);
            this.BindKey(Keys.N, Keybind.SelectionN);
            this.BindKey(Keys.O, Keybind.SelectionO);
            this.BindKey(Keys.P, Keybind.SelectionP);
            this.BindKey(Keys.Q, Keybind.SelectionQ);
            this.BindKey(Keys.R, Keybind.SelectionR);

            this.BindKey(Keys.Backquote, Keybind.Repl);

            this.Dirty = false;
        }

        public void BindKey(Keys keyAndModifiers, KeybindPrototypeID keybind)
        {
            if (this.AcceptedKeybinds.Contains(keybind))
            {
                this.Translations[keyAndModifiers] = keybind;
            }
        }

        public KeybindPrototypeID? KeyToKeybind(Keys keyAndModifiers)
        {
            if (this.Dirty)
            {
                // Get the list of all key bindings and bind only the ones that have actions set.
                this.Reload();
            }

            if (this.Translations.TryGetValue(keyAndModifiers, out KeybindPrototypeID? keybind))
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
