using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Prototypes;
using KeybindPrototypeID = OpenNefia.Core.Prototypes.PrototypeId<OpenNefia.Core.UI.KeybindPrototype>;

namespace OpenNefia.Core.UI
{
    public class KeybindTranslator
    {
        private Dictionary<Keys, IKeybind> _translations = new();
        private HashSet<IKeybind> _acceptedKeybinds = new();
        private bool _dirty = true;

        internal void Enable(IKeybind keybind)
        {
            this._acceptedKeybinds.Add(keybind);
            this._dirty = true;
        }

        internal void Disable(IKeybind keybind)
        {
            this._acceptedKeybinds.Remove(keybind);
            this._dirty = true;
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

            this._dirty = false;
        }

        public void BindKey(Keys keyAndModifiers, IKeybind keybind)
        {
            if (this._acceptedKeybinds.Contains(keybind))
            {
                this._translations[keyAndModifiers] = keybind;
            }
        }

        public void BindKey(Keys keyAndModifiers, KeybindPrototypeID keybind)
        {
            this.BindKey(keyAndModifiers, keybind.ResolvePrototype());
        }

        public IKeybind? KeyToKeybind(Keys keyAndModifiers)
        {
            if (this._dirty)
            {
                // Get the list of all key bindings and bind only the ones that have actions set.
                this.Reload();
            }

            if (this._translations.TryGetValue(keyAndModifiers, out IKeybind? keybind))
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
