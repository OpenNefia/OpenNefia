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
        protected readonly List<UiContainerEntry> UiElements = new List<UiContainerEntry>();

        public virtual void AddElement(UiContainerEntry element, LayoutType extraType = LayoutType.None, int extraAmount = 0)
        {
            if (extraType != LayoutType.None)
            {
                AddLayout(extraType, extraAmount);
                UiElements.Add(element);
                AddLayout(extraType, -extraAmount);
            }
            else
            {
                UiElements.Add(element);
            }
        }

        public virtual void AddLayout(params (LayoutType type, int offset)[] layouts)
        {
            foreach(var layout in layouts)
                UiElements.Add(new UiContainerEntry(layout.type, layout.offset));
        }

        public virtual void AddLayout(LayoutType type, int offset)
        {
            UiElements.Add(new UiContainerEntry(type, offset));
        }

        public virtual void Resolve()
        {
            ResolvePreferredSize();
        }

        protected abstract Vector2i ResolvePreferredSize();

        public override void GetPreferredSize(out Vector2i size)
        {
            size = ResolvePreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            int xDiff = X, yDiff = Y;
            base.SetPosition(x, y);
            xDiff = X - xDiff;
            yDiff = Y - yDiff;
            foreach (var element in UiElements)
            {
                if (element.Element != null)
                    element.Element.SetPosition(element.Element.X + xDiff, element.Element.Y + yDiff);
            }
        }

        public override void Draw()
        {
            foreach (var element in UiElements)
            {
                if (element.Element != null)
                    element.Element.Draw();
            }
        }

        public override void Update(float dt)
        {
            foreach (var element in UiElements)
            {
                if (element.Element != null)
                    element.Element.Update(dt);
            }
        }

        public virtual void Clear()
        {
            foreach (var element in UiElements)
                element.Element?.Dispose();
            UiElements.Clear();
        }

        public override void Dispose()
        {
            Clear();
            base.Dispose();
        }

        public override void Localize(LocaleKey key)
        {
            base.Localize(key);
            foreach (var element in UiElements)
            {
                if (element.Element != null)
                    element.Element.Localize(key);
            }
        }
    }
}
