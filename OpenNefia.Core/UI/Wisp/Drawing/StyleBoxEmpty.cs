using OpenNefia.Core.Maths;

namespace OpenNefia.Core.UI.Wisp.Drawing
{
    public sealed class StyleBoxEmpty : StyleBox
    {
        protected override void DoDraw(UIBox2 pixelBox, Color tint)
        {
            // It's empty what more do you want?
        }
    }
}
