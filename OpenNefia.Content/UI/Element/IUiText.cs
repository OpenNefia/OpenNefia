using OpenNefia.Core;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Content.UI.Element
{
    public interface IUiText : IDrawable, IUiElement, IDisposable, ILocalizable
    {
        public string Text { get; set; }
        public Color Color { get; set; }
        public Color BgColor { get; set; }
        public FontSpec Font { get; set; }

        /// <summary>
        /// Width of the text contained inside of the UI element, in virtual pixels.
        /// </summary>
        public float TextWidth { get; }

        /// <summary>
        /// Width of the text contained inside of the UI element, in physical pixels.
        /// </summary>
        public int TextPixelWidth { get; }
    }
}
