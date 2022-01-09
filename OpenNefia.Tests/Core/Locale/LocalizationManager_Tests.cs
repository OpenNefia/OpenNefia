using NUnit.Framework;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Tests.Core.Locale
{
    [TestFixture]
    [TestOf(typeof(LocalizationManager))]
    public class LocalizationManager_Tests : LocalizationUnitTest
    {
        [Test]
        public void TestMissing()
        {
            var locMan = IoCManager.Resolve<ILocalizationManager>();
            locMan.Resync();

            Assert.That(locMan.GetString("Test.Core.Missing"), Is.EqualTo("<Missing key: Test.Core.Missing>"));
        }

        [Test]
        public void TestStrings()
        {
            var locMan = IoCManager.Resolve<ILocalizationManager>();

            locMan.LoadString(@"
Test.Core = {
    String = 'foo',

    Nested = {
        String = 'bar',
    }
}
");

            locMan.Resync();

            Assert.That(locMan.GetString("Test.Core.String"), Is.EqualTo("foo"));
            Assert.That(locMan.GetString("Test.Core.Nested.String"), Is.EqualTo("bar"));
        }

        [Test]
        public void TestRandomLists()
        {
            var locMan = IoCManager.Resolve<ILocalizationManager>();

            locMan.LoadString(@"
Test.Core = {
    List = { 'foo', 'bar', 'baz' },
}
");

            locMan.Resync();

            Assert.That(locMan.GetString("Test.Core.List"), Is.EqualTo("baz"));
            Assert.That(locMan.GetString("Test.Core.List"), Is.EqualTo("baz"));
            Assert.That(locMan.GetString("Test.Core.List"), Is.EqualTo("baz"));
            Assert.That(locMan.GetString("Test.Core.List"), Is.EqualTo("bar"));
            Assert.That(locMan.GetString("Test.Core.List"), Is.EqualTo("foo"));
            Assert.That(locMan.GetString("Test.Core.List"), Is.EqualTo("bar"));
        }

        [Test]
        public void TestFunctions()
        {
            var locMan = IoCManager.Resolve<ILocalizationManager>();

            locMan.LoadString(@"
Test.Core = {
    Function = function(arg1, arg2, arg3)
        return ('%d %s %s'):format(arg1, arg2, arg3)
    end
}
");

            locMan.Resync();

            Assert.That(locMan.GetString("Test.Core.Function", ("arg1", 42), ("arg2", "foo"), ("arg3", true)), Is.EqualTo("42 foo true"));
        }

        [Test]
        public void TestMultipleFiles()
        {
            var locMan = IoCManager.Resolve<ILocalizationManager>();

            locMan.LoadString(@"
Test.Core.Foo = {
    Bar = 'bar'
}
");

            locMan.LoadString(@"
Test.Core.Baz = {
    Quux = 'quux'
}
");

            locMan.Resync();

            Assert.That(locMan.GetString("Test.Core.Foo.Bar"), Is.EqualTo("bar"));
            Assert.That(locMan.GetString("Test.Core.Baz.Quux"), Is.EqualTo("quux"));
        }

        [Test]
        public void TestMultipleFilesSameNamespace()
        {
            var locMan = IoCManager.Resolve<ILocalizationManager>();

            locMan.LoadString(@"
Test.Core.Foo = {
    Bar = 'bar'
}
");

            locMan.LoadString(@"
Test.Core.Foo = {
    Baz = 'baz'
}
");

            locMan.Resync();

            Assert.That(locMan.GetString("Test.Core.Foo.Bar"), Is.EqualTo("bar"));
            Assert.That(locMan.GetString("Test.Core.Foo.Baz"), Is.EqualTo("baz"));
        }

        [Test]
        public void TestMultipleFilesParentNamespace()
        {
            var locMan = IoCManager.Resolve<ILocalizationManager>();

            locMan.LoadString(@"
Test.Core = {
    Foo = {
        Baz = 'baz'
    }
}
");

            locMan.LoadString(@"
Test.Core.Foo = {
    Bar = 'bar'
}
");

            locMan.Resync();

            Assert.That(locMan.GetString("Test.Core.Foo.Bar"), Is.EqualTo("bar"));
            Assert.That(locMan.GetString("Test.Core.Foo.Baz"), Is.EqualTo("baz"));
        }

        [Test]
        public void TestMultipleFilesParentNamespace2()
        {
            var locMan = IoCManager.Resolve<ILocalizationManager>();

            locMan.LoadString(@"
Test.Core.Foo = {
    Bar = 'bar'
}
");

            locMan.LoadString(@"
Test.Core = {
    Foo = {
        Baz = 'baz'
    }
}
");

            locMan.Resync();

            Assert.That(locMan.GetString("Test.Core.Foo.Bar"), Is.EqualTo("bar"));
            Assert.That(locMan.GetString("Test.Core.Foo.Baz"), Is.EqualTo("baz"));
        }
    }
}
