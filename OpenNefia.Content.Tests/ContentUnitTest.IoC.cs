using Moq;
using OpenNefia.Content.Repl;
using OpenNefia.Content.UI.Hud;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Tests
{
    public partial class ContentUnitTest
    {
        public static void RegisterIoC()
        {
            ContentIoC.Register();

            IoCManager.RegisterInstance<IFieldLayer>(new Mock<IFieldLayer>().Object, true);
            IoCManager.RegisterInstance<IHudLayer>(new TestingHudLayer(), true);
            IoCManager.RegisterInstance<IReplLayer>(new Mock<IReplLayer>().Object, true);
        }
    }

    public class TestingHudLayer : DummyLayer, IHudLayer
    {
        public IHudMessageWindow MessageWindow { get; } = new DummyMessageWindow();

        public Vector2i HudScreenOffset => new();

        public void Initialize()
        {
            
        }

        public void ToggleBacklog(bool visible)
        {
        }

        public void UpdateTime()
        {
        }
    }

    public class DummyMessageWindow : DummyDrawable, IHudMessageWindow
    {
        public void Print(string queryText, Color? color = null)
        {
        }
        public void Newline()
        {
        }
    }
}
