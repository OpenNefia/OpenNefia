using OpenNefia.Core;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Layer;

namespace OpenNefia.Tests
{
    public class DummyLayer : DummyDrawable, IUiLayer
    {
        public int ZOrder { get; set; } = 0;


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

        public void OnQuery()
        {
        }

        public void OnQueryFinish()
        {
        }

        public void SetPreferredSize()
        {
        }
    }
}