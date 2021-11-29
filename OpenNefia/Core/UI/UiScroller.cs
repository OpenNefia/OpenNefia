using OpenNefia.Core.Data.Types;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.UI
{
    public class UiScroller
    {
        private bool Up;
        private bool Down;
        private bool Left;
        private bool Right;

        public int Dx { get; private set; } = 0;
        public int Dy { get; private set; } = 0;

        public UiScroller()
        {
        }

        public void BindKeys(IUiInputElement parent)
        {
            parent.Keybinds[Keybind.UIUp].BindKey((state) => this.MoveUp(state), trackReleased: true);
            parent.Keybinds[Keybind.UIDown].BindKey((state) => this.MoveDown(state), trackReleased: true);
            parent.Keybinds[Keybind.UILeft].BindKey((state) => this.MoveLeft(state), trackReleased: true);
            parent.Keybinds[Keybind.UIRight].BindKey((state) => this.MoveRight(state), trackReleased: true);
        }

        private void MoveUp(UiKeyInputEventArgs evt)
        {
            this.Up = (evt.State != KeyPressState.Released);
        }

        private void MoveDown(UiKeyInputEventArgs evt)
        {
            this.Down = (evt.State != KeyPressState.Released);
        }

        private void MoveLeft(UiKeyInputEventArgs evt)
        {
            this.Left = (evt.State != KeyPressState.Released);
        }

        private void MoveRight(UiKeyInputEventArgs evt)
        {
            this.Right = (evt.State != KeyPressState.Released);
        }

        public void GetPositionDiff(float dt, out int outDx, out int outDy)
        {
            var dx = 0;
            var dy = 0;

            if (this.Up) dy += 1;
            if (this.Down) dy -= 1;
            if (this.Left) dx += 1;
            if (this.Right) dx -= 1;

            var delta = 1000f;
            var amount = (int)(dt * delta);

            outDx = amount * dx;
            outDy = amount * dy;
        }
    }
}
