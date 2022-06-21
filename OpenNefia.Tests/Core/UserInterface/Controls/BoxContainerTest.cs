using NUnit.Framework;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Wisp;
using OpenNefia.Core.UI.Wisp.Controls;
using static OpenNefia.Core.UI.Wisp.Controls.BoxContainer;

namespace OpenNefia.Tests.Core.UserInterface.Controls
{
    [TestFixture]
    [TestOf(typeof(BoxContainer))]
    public sealed class BoxContainerTest : OpenNefiaUnitTest
    {
        [Test]
        public void TestLayoutBasic()
        {
            var root = new LayoutContainer();
            var boxContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                MinSize = (50, 60)
            };
            var control1 = new WispControl { MinSize = (20, 20) };
            var control2 = new WispControl { MinSize = (30, 30) };

            root.AddChild(boxContainer);

            boxContainer.AddChild(control1);
            boxContainer.AddChild(control2);

            root.Arrange(new UIBox2(0, 0, 50, 60));

            Assert.That(control1.Position, Is.EqualTo(Vector2.Zero));
            Assert.That(control1.Size, Is.EqualTo(new Vector2(50, 20)));
            Assert.That(control2.Position, Is.EqualTo(new Vector2(0, 20)));
            Assert.That(control2.Size, Is.EqualTo(new Vector2(50, 30)));
            Assert.That(boxContainer.DesiredSize, Is.EqualTo(new Vector2(50, 60)));
        }

        [Test]
        public void TestLayoutExpand()
        {
            var root = new LayoutContainer();
            var boxContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                MinSize = (50, 60)
            };
            var control1 = new WispControl
            {
                VerticalExpand = true
            };
            var control2 = new WispControl { MinSize = (30, 30) };

            boxContainer.AddChild(control1);
            boxContainer.AddChild(control2);

            root.AddChild(boxContainer);

            root.Arrange(new UIBox2(0, 0, 100, 100));

            Assert.That(control1.Position, Is.EqualTo(Vector2.Zero));
            Assert.That(control1.Size, Is.EqualTo(new Vector2(50, 30)));
            Assert.That(control2.Position, Is.EqualTo(new Vector2(0, 30)));
            Assert.That(control2.Size, Is.EqualTo(new Vector2(50, 30)));
            Assert.That(boxContainer.DesiredSize, Is.EqualTo(new Vector2(50, 60)));
        }

        [Test]
        public void TestCalcMinSize()
        {
            var boxContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical
            };
            var control1 = new WispControl
            {
                MinSize = (50, 30)
            };
            var control2 = new WispControl { MinSize = (30, 50) };

            boxContainer.AddChild(control1);
            boxContainer.AddChild(control2);

            boxContainer.Measure((100, 100));

            Assert.That(boxContainer.DesiredSize, Is.EqualTo(new Vector2(50, 80)));
        }

        [Test]
        public void TestTwoExpand()
        {
            var root = new LayoutContainer();
            var boxContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                MinSize = (30, 80)
            };
            var control1 = new WispControl
            {
                VerticalExpand = true,
            };
            var control2 = new WispControl
            {
                VerticalExpand = true,
            };
            var control3 = new WispControl { MinSize = (0, 50) };

            root.AddChild(boxContainer);

            boxContainer.AddChild(control1);
            boxContainer.AddChild(control3);
            boxContainer.AddChild(control2);

            root.Arrange(new UIBox2(0, 0, 250, 250));

            Assert.That(control1.Position, Is.EqualTo(Vector2.Zero));
            Assert.That(control1.Size, Is.EqualTo(new Vector2(30, 15)));
            Assert.That(control3.Position, Is.EqualTo(new Vector2(0, 15)));
            Assert.That(control3.Size, Is.EqualTo(new Vector2(30, 50)));
            Assert.That(control2.Position, Is.EqualTo(new Vector2(0, 65)));
            Assert.That(control2.Size, Is.EqualTo(new Vector2(30, 15)));
        }
    }
}
