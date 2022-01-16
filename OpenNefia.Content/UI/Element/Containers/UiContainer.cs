using OpenNefia.Core;
using OpenNefia.Core.Maths;
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
        protected readonly List<UiContainerEntry> Entries = new List<UiContainerEntry>();

        public virtual void AddElement(UiContainerEntry element, LayoutType extraType = LayoutType.None, int extraAmount = 0)
        {
            if (extraType != LayoutType.None)
            {
                AddLayout(extraType, extraAmount);
                Entries.Add(element);
                AddLayout(extraType, -extraAmount);
            }
            else
            {
                Entries.Add(element);
            }
        }

        public virtual void AddLayout(LayoutType type, int offset)
        {
            Entries.Add(new UiContainerEntry(type, offset));
        }

        public virtual void Relayout()
        {
            RelayoutPreferredSize();
        }

        protected abstract Vector2i RelayoutPreferredSize();

        public override void GetPreferredSize(out Vector2i size)
        {
            size = RelayoutPreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            int xDiff = X, yDiff = Y;
            base.SetPosition(x, y);
            xDiff = X - xDiff;
            yDiff = Y - yDiff;
            foreach (var entry in Entries)
            {
                if (entry.Element != null)
                    entry.Element.SetPosition(entry.Element.X + xDiff, entry.Element.Y + yDiff);
            }
        }

        public override void Draw()
        {
            foreach (var entry in Entries)
            {
                if (entry.Element != null)
                    entry.Element.Draw();
            }
        }

        public override void Update(float dt)
        {
            foreach (var entry in Entries)
            {
                if (entry.Element != null)
                    entry.Element.Update(dt);
            }
        }

        public virtual void Clear()
        {
            foreach (var entry in Entries)
                entry.Element?.Dispose();
            Entries.Clear();
        }

        public override void Dispose()
        {
            Clear();
            base.Dispose();
        }

        public override void Localize(LocaleKey key)
        {
            base.Localize(key);
            foreach (var entry in Entries)
            {
                if (entry.Element != null)
                    entry.Element.Localize(key);
            }
        }
    }
}
