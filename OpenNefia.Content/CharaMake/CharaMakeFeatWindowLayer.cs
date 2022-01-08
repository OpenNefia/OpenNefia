using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Locale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.CharaMake
{
    [Localize("Elona.CharaMake.FeatSelect")]
    public class CharaMakeFeatWindowLayer : CharaMakeLayer
    {
        private FeatWindow Window = default!;
        public CharaMakeFeatWindowLayer()
        {
            Window = new FeatWindow(default!, default!);
            AddChild(Window.List);
            OnKeyBindDown += Window.OnKeyDown;
        }

        public override void OnFocused()
        {
            base.OnFocused();
            Window.List.GrabFocus();
        }

        public override void Initialize(CharaMakeData args)
        {
            base.Initialize(args);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            Window.SetPreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            Center(Window, 10);
        }

        public override void Draw()
        {
            base.Draw();
            Window.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            Window.Update(dt);
        }
    }
}

