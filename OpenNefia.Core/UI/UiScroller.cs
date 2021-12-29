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

        public void BindKeys(UiElement parent)
        {
            //parent.Keybinds[CoreKeybinds.UIUp].BindKey((state) => this.MoveUp(state), trackReleased: true);
            //parent.Keybinds[CoreKeybinds.UIDown].BindKey((state) => this.MoveDown(state), trackReleased: true);
            //parent.Keybinds[CoreKeybinds.UILeft].BindKey((state) => this.MoveLeft(state), trackReleased: true);
            //parent.Keybinds[CoreKeybinds.UIRight].BindKey((state) => this.MoveRight(state), trackReleased: true);
        }

        private void MoveUp(GUIBoundKeyEventArgs evt)
        {
            this.Up = (evt.State == Input.BoundKeyState.Down);
        }

        private void MoveDown(GUIBoundKeyEventArgs evt)
        {
            this.Down = (evt.State == Input.BoundKeyState.Down);
        }

        private void MoveLeft(GUIBoundKeyEventArgs evt)
        {
            this.Left = (evt.State == Input.BoundKeyState.Down);
        }

        private void MoveRight(GUIBoundKeyEventArgs evt)
        {
            this.Right = (evt.State == Input.BoundKeyState.Down);
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
