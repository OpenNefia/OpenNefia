using OpenNefia.Core.Maths;

namespace OpenNefia.Core.UI.Wisp.Drawing
{
    public sealed class StyleBoxEmpty : StyleBox
    {
        protected override void DoDraw(UIBox2 box, float uiScale)
        {
            // It's empty what more do you want?
        }
    }
}
