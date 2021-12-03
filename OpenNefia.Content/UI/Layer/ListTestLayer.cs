using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.UI.Layer
{
    internal class ListTestLayer : BaseUiLayer<string>
    {
        [Localize]
        public UiWindow Window { get; }

        public UiList<string> List1 { get; }
        public UiList<string> List2 { get; }
        public UiList<string> List3 { get; }

        private int Index;

        public ListTestLayer()
        {
            Window = new UiWindow();
            List1 = new UiList<string>(new List<string>() { "abc", "def", "ghi" });
            List2 = new UiList<string>(new List<string>() { "hoge", "piyo", "fuga" });
            List3 = new UiList<string>(new List<string>() { "あいうえお", "アイウエオ", "ってコト？！" });

            SelectList(List1);

            Keybinds[CoreKeybinds.Escape] += (_) => Cancel();
            Keybinds[CoreKeybinds.Cancel] += (_) => Cancel();
            Keybinds[CoreKeybinds.UILeft] += (_) => NextList(-1);
            Keybinds[CoreKeybinds.UIRight] += (_) => NextList(1);

            UiListEventHandler<string> printIt = (_, evt) =>
            {
                Sounds.Play(SoundPrototypeOf.Ok1);
                Console.WriteLine($"Get item: {evt.SelectedCell.Data}");
            };
            List1.EventOnActivate += printIt;
            List2.EventOnActivate += printIt;
            List3.EventOnActivate += printIt;

            UiListEventHandler<string> selectIt = (_, evt) =>
            {
                List1.SelectedIndex = evt.SelectedIndex;
                List2.SelectedIndex = evt.SelectedIndex;
                List3.SelectedIndex = evt.SelectedIndex;
            };
            List1.EventOnSelect += selectIt;
            List2.EventOnSelect += selectIt;
            List3.EventOnSelect += selectIt;

            List2.Keybinds[CoreKeybinds.Mode] += (_) => Console.WriteLine("Dood!");

            MouseMoved.Callback += (evt) =>
            {
                if (List1.ContainsPoint(evt.Pos) && Index != 1)
                {
                    Sounds.Play(SoundPrototypeOf.Cursor1);
                    Index = 1;
                    SelectList(List1);
                }
                else if (List2.ContainsPoint(evt.Pos) && Index != 2)
                {
                    Sounds.Play(SoundPrototypeOf.Cursor1);
                    Index = 2;
                    SelectList(List2);
                }
                else if (List3.ContainsPoint(evt.Pos) && Index != 3)
                {
                    Sounds.Play(SoundPrototypeOf.Cursor1);
                    Index = 3;
                    SelectList(List3);
                }

                evt.Pass();
            };

            Index = 1;
            SelectList(List1);
        }

        public override void OnQuery()
        {
            Sounds.Play(SoundPrototypeOf.Pop2);
        }

        private void NextList(int delta)
        {
            Sounds.Play(SoundPrototypeOf.Cursor1);

            Index += delta;
            if (Index > 3)
                Index = 1;
            else if (Index < 1)
                Index = 3;

            switch (Index)
            {
                default:
                case 1:
                    SelectList(List1);
                    break;
                case 2:
                    SelectList(List2);
                    break;
                case 3:
                    SelectList(List3);
                    break;
            }
        }

        private void SelectList(IUiList<string> list)
        {
            ClearAllForwards();
            List1.HighlightSelected = false;
            List2.HighlightSelected = false;
            List3.HighlightSelected = false;

            list.HighlightSelected = true;
            Forwards += list;
        }

        public override void GetPreferredBounds(out UIBox2i bounds)
        {
            UiUtils.GetCenteredParams(400, 170, out bounds);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);

            Window.SetSize(Width, Height);
            var listWidth = (Width - 40) / 3;
            List1.SetSize(listWidth, Height - 40);
            List2.SetSize(listWidth, Height - 40);
            List3.SetSize(listWidth, Height - 40);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);

            Window.SetPosition(X, Y);
            List1.SetPosition(X + 20, Y + 40);
            List2.SetPosition(X + 20 + (int)((Width - 40) * 0.33), Y + 40);
            List3.SetPosition(X + 20 + (int)((Width - 40) * 0.66), Y + 40);
        }

        public override void Update(float dt)
        {
            Window.Update(dt);
            List1.Update(dt);
            List2.Update(dt);
            List3.Update(dt);
        }

        public override void Draw()
        {
            Window.Draw();
            List1.Draw();
            List2.Draw();
            List3.Draw();
        }

        public override void Dispose()
        {
            Window.Dispose();
            List1.Dispose();
            List2.Dispose();
            List3.Dispose();
        }
    }
}
