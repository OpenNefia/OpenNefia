using OpenNefia.Core;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Layer;

namespace OpenNefia.Tests
{
    public class DummyLayer : DummyDrawable, IUiLayer
    {
        public int ZOrder { get; set; } = 0;

        public Vector2 ExactSize { get; set; } = new(800, 600);
        public Vector2 MinSize { get; set; } = new(800, 600);
        public bool Visible { get; set; } = true;

        public bool IsInActiveLayerList()
        {
            return true;
        }

        public bool IsQuerying()
        {
            return true;
        }

        public void Localize(LocaleKey key)
        {
        }

        public void Localize()
        {
        }

        public void OnQuery()
        {
        }

        public void OnQueryFinish()
        {
        }

        public void GetPreferredPosition(out Vector2 pos)
        {
            pos = Vector2.Zero;
        }

        public void SetPreferredPosition()
        {
        }

        public void SetPreferredSize()
        {
        }
    }
}