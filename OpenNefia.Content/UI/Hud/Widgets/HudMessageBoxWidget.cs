using OpenNefia.Content.UI.Element.Containers;
using OpenNefia.Content.UI.Hud;
using OpenNefia.Core.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Hud
{
    public class HudMessageBoxWidget : BaseHudWidget, IHudMessageWindow, IBacklog
    {
        private HudMessageWindow MessageWindow = default!;
        private UiContainer MessageBoxContainer = default!;
        private UiContainer BacklogContainer = default!;

        public bool IsShowingBacklog { get; private set; }

        public override void Initialize()
        {
            base.Initialize();
            MessageBoxContainer = new UiVerticalContainer();
            BacklogContainer = new UiVerticalContainer();
            MessageWindow = new HudMessageWindow(MessageBoxContainer, BacklogContainer);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            MessageWindow.SetSize(width, height);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            MessageBoxContainer.SetPosition(x, y);
            BacklogContainer.SetPosition(x, y - 373);
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
    }
}
