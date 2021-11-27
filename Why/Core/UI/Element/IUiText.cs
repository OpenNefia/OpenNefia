using System;
using OpenNefia.Core.Data.Types;

namespace OpenNefia.Core.UI.Element
{
    public interface IUiText : IDrawable, IUiDefaultSizeable, IDisposable, ILocalizable
    {
        public string Text { get; set; }
        public Love.Color? Color { get; set; }
        public FontDef Font { get; set; }
        public int TextWidth { get; }
    }
}
