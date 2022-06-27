using OpenNefia.Content.UI.Element.Containers;
using OpenNefia.Content.UI.Hud;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Hud
{
    public class HudMessageBoxWidget : BaseHudWidget, IHudMessageWindow, IBacklog
    {
        [Child] private HudMessageWindow MessageWindow;
        [Child] private UiContainer MessageBoxContainer;
        [Child] private UiContainer BacklogContainer;
        
        public bool IsShowingBacklog { get; private set; }

        public HudMessageBoxWidget()
        {
            MessageBoxContainer = new UiVerticalContainer();
            BacklogContainer = new UiVerticalContainer();
            MessageWindow = new HudMessageWindow(MessageBoxContainer, BacklogContainer);
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            MessageWindow.SetSize(Width, Height);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            MessageBoxContainer.SetPosition(X, Y);
            BacklogContainer.SetPosition(X, Y - 373);
        }

        public override void Draw()
        {
            base.Draw();
            MessageBoxContainer.Draw();
            if (IsShowingBacklog)
                BacklogContainer.Draw();
        }

        public void ToggleBacklog(bool visible)
        {
            IsShowingBacklog = visible;
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            MessageWindow.Update(dt);
        }

        public void Newline() => MessageWindow.Newline();
        public void Print(string queryText, Color? color = null) => MessageWindow.Print(queryText, color);
        public void Clear() => MessageWindow.Clear();
    }
}
