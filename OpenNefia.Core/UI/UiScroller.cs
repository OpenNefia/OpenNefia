using OpenNefia.Core.Input;
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

        public void HandleKeyBindDown(GUIBoundKeyEventArgs evt)
        {
            if (evt.Function == EngineKeyFunctions.UIUp)
            {
                Up = true;
            }
            else if (evt.Function == EngineKeyFunctions.UIDown)
            {
                Down = true;
            }
            else if (evt.Function == EngineKeyFunctions.UILeft)
            {
                Left = true;
            }
            else if (evt.Function == EngineKeyFunctions.UIRight)
            {
                Right = true;
            }
        }

        public void HandleKeyBindUp(GUIBoundKeyEventArgs evt)
        {
            if (evt.Function == EngineKeyFunctions.UIUp)
            {
                Up = false;
            }
            else if (evt.Function == EngineKeyFunctions.UIDown)
            {
                Down = false;
            }
            else if (evt.Function == EngineKeyFunctions.UILeft)
            {
                Left = false;
            }
            else if (evt.Function == EngineKeyFunctions.UIRight)
            {
                Right = false;
            }
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
