using Moq;
using OpenNefia.Content.UI.Hud;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Content.UI.Layer.Repl;
using OpenNefia.Core.IoC;
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
            IoCManager.RegisterInstance<IHudLayer>(new Mock<IHudLayer>().Object, true);
            IoCManager.RegisterInstance<IReplLayer>(new Mock<IReplLayer>().Object, true);
        }
    }
}
