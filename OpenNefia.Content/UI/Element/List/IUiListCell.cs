using OpenNefia.Core;
using OpenNefia.Core.UI.Element;
using System;

namespace OpenNefia.Content.UI.Element.List
{
    public interface IUiListCell : IDrawable, IDisposable, IUiElement, ILocalizable
    {
        public UiListChoiceKey? Key { get; set; }
        public float XOffset { get; set; }
        public string? LocalizeKey { get; }
        public int IndexInList { get; set; }
        int PixelXOffset { get; }

        public void DrawHighlight();
    }

    public interface IUiListCell<T> : IUiListCell
    {
        public T Data { get; set; }
    }
}
