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
