using Love;
using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using System.Collections;

namespace OpenNefia.Content.UI.Layer
{
    public class TestLayer : UiLayerWithResult<UINone, string>
    {
        private UiWindowBacking WindowBacking;

        public TestLayer()
        {
            WindowBacking = new UiWindowBacking(UiWindowBacking.WindowBackingType.Normal);

            BindKeys();
        }

        private int SquareX = 0;

        protected virtual void BindKeys()
        {
            //Keybinds[CoreKeybinds.Escape] += (_) => Cancel();
            //Keybinds[CoreKeybinds.Cancel] += (_) => Cancel();

            //Keybinds[CoreKeybinds.Identify] += (_) =>
            //{
            //    var choices = new List<PromptChoice<int>>()
            //    {
            //        new PromptChoice<int>(0),
            //        new PromptChoice<int>(24),
            //        new PromptChoice<int>(42142132)
            //    };
            //    var prompt = new Prompt<int>(choices);
            //    Console.WriteLine($"Prompt start");
            //    var result = prompt.Query();
            //    Console.WriteLine($"Prompt result: {result}");
            //};

            //Keybinds[CoreKeybinds.Mode] += (_) =>
            //{
            //    var numberPrompt = new NumberPrompt(minValue: 2, maxValue: 100, initialValue: 50);
            //    var result = numberPrompt.Query();
            //    Console.WriteLine($"Number prompt result: {result}");
            //};

            //Keybinds[Keys.Ctrl | Keys.C] += (_) =>
            //{
            //    new ListTestLayer().Query();
            //};

            //Keybinds[Keys.Ctrl | Keys.D] += (_) =>
            //{
            //    var text = new TextPrompt().Query();
            //    Console.WriteLine($"Get: {text}");
            //};

            //Keybinds[Keys.Ctrl | Keys.E] += (_) =>
            //{
            //    new ProgressBarLayer(new TestProgressJob()).Query();
            //};
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

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public override void GetPreferredBounds(out UIBox2i bounds)
        {
            UiUtils.GetCenteredParams(400, 300, out bounds);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);

            WindowBacking.SetSize(width, height);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);

            WindowBacking.SetPosition(X, Y);
        }

        public override void Update(float dt)
        {
            if (SquareX > 200)
            {
                SquareX = 0;
            }
            else
            {
                SquareX++;
            }

            WindowBacking.Update(dt);
        }

        public override void Draw()
        {
            Graphics.SetColor(1f, 1f, 1f);
            Graphics.Rectangle(DrawMode.Fill, 100, 100, 100, 100);
            Graphics.SetColor(1f, 0, 1f);
            Graphics.Rectangle(DrawMode.Fill, 50 + SquareX, 50, 100, 100);

            Graphics.SetColor(1f, 1f, 1f);
            WindowBacking.Draw();
        }

        public override void Dispose()
        {
            WindowBacking.Dispose();
        }
    }
}
