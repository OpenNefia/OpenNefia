using System;
using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Core.UI.Element
{
    public interface IUiText : IDrawable, IUiDefaultSizeable, IDisposable, ILocalizable
    {
        public string Text { get; set; }
        public Maths.Color Color { get; set; }
        public FontSpec Font { get; set; }
        public int TextWidth { get; }
    }
}
