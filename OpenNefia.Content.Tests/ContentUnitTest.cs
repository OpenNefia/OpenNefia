using Moq;
using OpenNefia.Content.UI.Hud;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Content.UI.Layer.Repl;
using OpenNefia.Core.IoC;
using OpenNefia.Tests;
using System.Reflection;

namespace OpenNefia.Content.Tests
{
    public class ContentUnitTest : OpenNefiaUnitTest
    {
        protected override void OverrideIoC()
        {
            base.OverrideIoC();

            ContentIoC.Register();

            IoCManager.RegisterInstance<IFieldLayer>(new Mock<IFieldLayer>().Object, true);
            IoCManager.RegisterInstance<IHudLayer>(new Mock<IHudLayer>().Object, true);
            IoCManager.RegisterInstance<IReplLayer>(new Mock<IReplLayer>().Object, true);
        }

        protected override Assembly[] GetContentAssemblies()
        {
            return new Assembly[2]
            {
                typeof(OpenNefia.Content.EntryPoint).Assembly,
                typeof(ContentUnitTest).Assembly
            };
        }
    }
}
