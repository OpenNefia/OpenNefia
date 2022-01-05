using Moq;
using OpenNefia.Content.UI.Hud;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Content.UI.Layer.Repl;
using OpenNefia.Core.IoC;
using OpenNefia.Tests;
using System.Reflection;

namespace OpenNefia.Content.Tests
{
    public partial class ContentUnitTest : OpenNefiaUnitTest
    {
        protected override void OverrideIoC()
        {
            base.OverrideIoC();

            RegisterIoC();
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
