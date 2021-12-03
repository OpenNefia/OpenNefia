using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Prototypes;

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
            this.BindKey(Keys.Return, CoreKeybinds.Enter);
            this.BindKey(Keys.Shift, CoreKeybinds.Cancel);
            this.BindKey(Keys.Escape, CoreKeybinds.Escape);
            this.BindKey(Keys.Escape, CoreKeybinds.Quit);

            this.BindKey(Keys.Up, CoreKeybinds.UIUp);
            this.BindKey(Keys.Down, CoreKeybinds.UIDown);
            this.BindKey(Keys.Left, CoreKeybinds.UILeft);
            this.BindKey(Keys.Right, CoreKeybinds.UIRight);

            this.BindKey(Keys.Up, CoreKeybinds.North);
            this.BindKey(Keys.Down, CoreKeybinds.South);
            this.BindKey(Keys.Left, CoreKeybinds.West);
            this.BindKey(Keys.Right, CoreKeybinds.East);

            this.BindKey(Keys.Period, CoreKeybinds.Wait);
            this.BindKey(Keys.X, CoreKeybinds.Identify);
            this.BindKey(Keys.Z, CoreKeybinds.Mode);
            this.BindKey(Keys.KeypadMultiply, CoreKeybinds.Mode2);

            this.BindKey(Keys.A, CoreKeybinds.SelectionA);
            this.BindKey(Keys.B, CoreKeybinds.SelectionB);
            this.BindKey(Keys.C, CoreKeybinds.SelectionC);
            this.BindKey(Keys.D, CoreKeybinds.SelectionD);
            this.BindKey(Keys.E, CoreKeybinds.SelectionE);
            this.BindKey(Keys.F, CoreKeybinds.SelectionF);
            this.BindKey(Keys.G, CoreKeybinds.SelectionG);
            this.BindKey(Keys.H, CoreKeybinds.SelectionH);
            this.BindKey(Keys.I, CoreKeybinds.SelectionI);
            this.BindKey(Keys.J, CoreKeybinds.SelectionJ);
            this.BindKey(Keys.K, CoreKeybinds.SelectionK);
            this.BindKey(Keys.L, CoreKeybinds.SelectionL);
            this.BindKey(Keys.M, CoreKeybinds.SelectionM);
            this.BindKey(Keys.N, CoreKeybinds.SelectionN);
            this.BindKey(Keys.O, CoreKeybinds.SelectionO);
            this.BindKey(Keys.P, CoreKeybinds.SelectionP);
            this.BindKey(Keys.Q, CoreKeybinds.SelectionQ);
            this.BindKey(Keys.R, CoreKeybinds.SelectionR);

            this.BindKey(Keys.Backquote, CoreKeybinds.Repl);

            this._dirty = false;
        }

        public void BindKey(Keys keyAndModifiers, IKeybind keybind)
        {
            if (this._acceptedKeybinds.Contains(keybind))
            {
                this._translations[keyAndModifiers] = keybind;
            }
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
