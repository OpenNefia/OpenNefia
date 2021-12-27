using OpenNefia.Core;
using OpenNefia.Core.UI.Element;
using System;

namespace OpenNefia.Content.UI.Element.List
{
    public interface IUiListCell<T> : IDrawable, IDisposable, IUiDefaultSizeable, ILocalizable
    {
        public T Data { get; set; }
        public UiListChoiceKey? Key { get; set; }
        public int XOffset { get; set; }
        public string? LocalizeKey { get; }
        public int IndexInList { get; set; }

        public void DrawHighlight();
    }
}
