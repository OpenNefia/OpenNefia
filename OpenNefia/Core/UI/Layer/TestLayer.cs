using Love;
using OpenNefia.Core.Data.Types;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI.Layer
{
    public class TestLayer : BaseUiLayer<string>
    {
        private UiWindowBacking WindowBacking;

        public TestLayer()
        {
            this.WindowBacking = new UiWindowBacking(UiWindowBacking.WindowBackingType.Normal);

            this.BindKeys();
        }

        private int SquareX = 0;

        protected virtual void BindKeys()
        {
            this.Keybinds[Keybind.Escape] += (_) => this.Cancel();
            this.Keybinds[Keybind.Cancel] += (_) => this.Cancel();

            this.Keybinds[Keybind.Identify] += (_) =>
            {
                var choices = new List<PromptChoice<int>>()
                {
                    new PromptChoice<int>(0),
                    new PromptChoice<int>(24),
                    new PromptChoice<int>(42142132)
                };
                var prompt = new Prompt<int>(choices);
                Console.WriteLine($"Prompt start");
                var result = prompt.Query();
                Console.WriteLine($"Prompt result: {result}");
            };

            this.Keybinds[Keybind.Mode] += (_) =>
            {
                var numberPrompt = new NumberPrompt(minValue: 2, maxValue: 100, initialValue: 50);
                var result = numberPrompt.Query();
                Console.WriteLine($"Number prompt result: {result}");
            };

            this.Keybinds[Keys.Ctrl | Keys.C] += (_) =>
            {
                new ListTestLayer().Query();
            };

            this.Keybinds[Keys.Ctrl | Keys.D] += (_) =>
            {
                var text = new TextPrompt().Query();
                Console.WriteLine($"Get: {text}");
            };

            this.Keybinds[Keys.Ctrl | Keys.E] += (_) =>
            {
                new ProgressBarLayer(new TestProgressJob()).Query();
            };
        }

        private class TestProgressJob : IProgressableJob
        {
            public uint NumberOfSteps => 7;

            public IEnumerator<ProgressStep> GetEnumerator()
            {
                yield return new ProgressStep("Test 1!", Task.Delay(500));
                yield return new ProgressStep("Test 2!", Task.Delay(500));
                yield return new ProgressStep("Test 3!", Task.Delay(500));
                yield return new ProgressStep("Test 4!", Task.Delay(500));
                yield return new ProgressStep("Test 5!", Task.Delay(500));
                yield return new ProgressStep("Test 6!", Task.Delay(500));
                yield return new ProgressStep("Test 7!", Task.Delay(500));
            }

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        }

        public override void GetPreferredBounds(out int x, out int y, out int width, out int height)
        {
            var rect = UiUtils.GetCenteredParams(400, 300);
            x = rect.X;
            y = rect.Y;
            width = rect.Width;
            height = rect.Height;
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);

            this.WindowBacking.SetSize(width, height);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);

            this.WindowBacking.SetPosition(this.Left, this.Top);
        }

        public override void Update(float dt)
        {
            if (this.SquareX > 200)
            {
                this.SquareX = 0;
            }
            else
            {
                this.SquareX++;
            }

            this.WindowBacking.Update(dt);
        }

        public override void Draw()
        {
            Graphics.SetColor(1f, 1f, 1f);
            Graphics.Rectangle(DrawMode.Fill, 100, 100, 100, 100);
            Graphics.SetColor(1f, 0, 1f);
            Graphics.Rectangle(DrawMode.Fill, 50 + this.SquareX, 50, 100, 100);

            Graphics.SetColor(1f, 1f, 1f);
            this.WindowBacking.Draw();
        }

        public override void Dispose()
        {
            this.WindowBacking.Dispose();
        }
    }
}
