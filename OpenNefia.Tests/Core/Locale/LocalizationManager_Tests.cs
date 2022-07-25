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
            Assert.That(locMan.TryGetString("Test.Core.Missing", out var str), Is.False);
            Assert.That(str, Is.Null);
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
        public void TestRandomLists_Function()
        {
            var locMan = IoCManager.Resolve<ILocalizationManager>();

            locMan.LoadString(@"
Test.Core = {
    List = { 
        function() return 'foo' end, 
        function() return 'bar' end, 
        function() return 'baz' end 
    },
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

        [Test]
        public void TestRootedTable()
        {
            var locMan = IoCManager.Resolve<ILocalizationManager>();

            locMan.LoadString(@"
Test = {
    Core = {
        String = 'foo',
    }
}
");

            locMan.Resync();

            Assert.That(locMan.GetString("Test.Core.String"), Is.EqualTo("foo"));
        }

        [Test]
        public void TestReferences_Missing()
        {
            var locMan = IoCManager.Resolve<ILocalizationManager>();

            locMan.LoadString(@"
Test = {
    Bar = {
        String = _.ref 'Test.Foo.String'
    },
}
");

            locMan.Resync();

            Assert.That(locMan.GetString("Test.Bar.String"), Is.EqualTo("<missing reference: Test.Foo.String -> Test.Bar.String>"));
        }
        
        [Test]
        public void TestReferences_Found()
        {
            var locMan = IoCManager.Resolve<ILocalizationManager>();

            locMan.LoadString(@"
Test = {
    Foo = {
        String = 'foo',
    },
    Bar = {
        String = _.ref 'Test.Foo.String'
    },
}
");

            locMan.Resync();

            Assert.That(locMan.GetString("Test.Foo.String"), Is.EqualTo("foo"));
            Assert.That(locMan.GetString("Test.Bar.String"), Is.EqualTo("foo"));
        }

        [Test]
        public void TestReferences_FoundPrototype()
        {
            var locMan = IoCManager.Resolve<ILocalizationManager>();

            locMan.LoadString(@"
OpenNefia.Prototypes.Test = {
    Foo = {
        String = 'foo',
    },
    Bar = {
        String = _.refp 'Test.Foo.String'
    },
}
");

            locMan.Resync();

            Assert.That(locMan.GetString("OpenNefia.Prototypes.Test.Foo.String"), Is.EqualTo("foo"));
            Assert.That(locMan.GetString("OpenNefia.Prototypes.Test.Bar.String"), Is.EqualTo("foo"));
        }

        [Test]
        public void TestReferences_Recursive()
        {
            var locMan = IoCManager.Resolve<ILocalizationManager>();

            locMan.LoadString(@"
Test = {
    Foo = {
        String = 'foo',
    },
    Bar = {
        String = _.ref 'Test.Foo.String'
    },
    Baz = {
        String = _.ref 'Test.Bar.String'
    },
}
");

            locMan.Resync();

            Assert.That(locMan.GetString("Test.Foo.String"), Is.EqualTo("foo"));
            Assert.That(locMan.GetString("Test.Bar.String"), Is.EqualTo("foo"));
            Assert.That(locMan.GetString("Test.Baz.String"), Is.EqualTo("foo"));
        }

        [Test]
        public void TestReferences_LocalizationData()
        {
            var locMan = IoCManager.Resolve<ILocalizationManager>();

            locMan.LoadString(@"
Test = {
    Foo = {
        String = 'foo',
    },
    Bar = {
        String = _.ref 'Test.Foo.String'
    },
}
");

            locMan.Resync();

            Assert.That(locMan.TryGetTable("Test.Bar", out var table), Is.True);
            Assert.That(table!.GetStringOrNull("String"), Is.EqualTo("foo"));
        }

        [Test]
        public void TestReferences_LocalizationData_NotFound()
        {
            var locMan = IoCManager.Resolve<ILocalizationManager>();

            locMan.LoadString(@"
Test = {
    Bar = {
        String = _.ref 'Test.Foo.String'
    },
}
");

            locMan.Resync();

            Assert.That(locMan.TryGetTable("Test.Bar", out var table), Is.True);
            Assert.That(table!.GetStringOrNull("String"), Is.EqualTo("<missing reference: Test.Foo.String -> Test.Bar.String>"));
        }
    }
}
