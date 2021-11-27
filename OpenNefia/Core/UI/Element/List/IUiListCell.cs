using System;

namespace OpenNefia.Core.UI.Element.List
{
    public interface IUiListCell<T> : IDrawable, IDisposable, IUiDefaultSizeable, ILocalizable
    {
        public T Data { get; set; }
        public UiListChoiceKey? Key { get; set; }
        public int XOffset { get; set; }
        public string? LocalizeKey { get; }

        public void DrawHighlight();
    }
}
