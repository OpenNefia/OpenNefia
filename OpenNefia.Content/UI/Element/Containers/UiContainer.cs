using OpenNefia.Core;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.UI.Element.Containers
{
    public enum LayoutType
    {
        None,
        Element,
        Spacer,
        XOffset,
        YOffset,
        FullUnset,
        XUnset,
        YUnset,
        YMin,
        XMin
    }

    public class UiContainerEntry
    {
        public UiElement? Element { get; set; }
        public LayoutType Type { get; set;}
        public int Offset { get; set; }

        public UiContainerEntry(UiElement uiElement)
        {
            Element = uiElement;
            Type = LayoutType.Element;
        }

        public UiContainerEntry(LayoutType type, int offset)
        {
            Type = type;
            Offset = offset;
        }

        public static implicit operator UiContainerEntry(UiElement element)
            => new UiContainerEntry(element);
    }

    public abstract class UiContainer : UiElement, ILocalizable
    {
        protected readonly List<UiContainerEntry> _entries = new List<UiContainerEntry>();
        public IReadOnlyList<UiContainerEntry> Entries => _entries;

        public virtual void AddElement(UiContainerEntry element, LayoutType extraType = LayoutType.None, int extraAmount = 0)
        {
            if (extraType != LayoutType.None)
            {
                AddLayout(extraType, extraAmount);
                _entries.Add(element);
                AddLayout(extraType, -extraAmount);
            }
            else
            {
                _entries.Add(element);
            }

            if (element.Element != null)
            {
                if (element.Element.Parent != null)
                {
                    // Logger.WarningS("ui.container", $"Container child element {element.Element} already parented to {element.Element.Parent} in AddElement()");
                    element.Element.Parent.RemoveChild(element.Element);
                }

                UiHelpers.AddChildrenRecursive(this, element.Element);
            }
        }

        public virtual void AddLayout(LayoutType type, int offset)
        {
            _entries.Add(new UiContainerEntry(type, offset));
        }

        public virtual void Relayout()
        {
            RelayoutPreferredSize();
        }

        protected abstract Vector2 RelayoutPreferredSize();

        public override void GetPreferredSize(out Vector2 size)
        {
            size = RelayoutPreferredSize();
        }

        public override void SetPosition(float x, float y)
        {
            float xDiff = X, yDiff = Y;
            base.SetPosition(x, y);
            xDiff = X - xDiff;
            yDiff = Y - yDiff;
            foreach (var entry in _entries)
            {
                if (entry.Element != null)
                    entry.Element.SetPosition(entry.Element.X + xDiff, entry.Element.Y + yDiff);
            }
        }

        public override void Draw()
        {
            foreach (var entry in _entries)
            {
                if (entry.Element != null)
                    entry.Element.Draw();
            }
        }

        public override void Update(float dt)
        {
            foreach (var entry in _entries)
            {
                if (entry.Element != null)
                    entry.Element.Update(dt);
            }
        }

        public virtual void Clear()
        {
            foreach (var entry in _entries)
            {
                if (entry.Element != null && entry.Element.Parent == this)
                {
                    RemoveChild(entry.Element);
                }
            }
            _entries.Clear();
        }

        public override void Dispose()
        {
            Clear();
            base.Dispose();
        }
    }
}
