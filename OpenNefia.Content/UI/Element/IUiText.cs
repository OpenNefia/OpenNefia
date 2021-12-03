using OpenNefia.Core;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Content.UI.Element
{
    public interface IUiText : IDrawable, IUiDefaultSizeable, IDisposable, ILocalizable
    {
        public string Text { get; set; }
        public Color Color { get; set; }
        public Color BgColor { get; set; }
        public FontSpec Font { get; set; }
        public int TextWidth { get; }
    }
}
