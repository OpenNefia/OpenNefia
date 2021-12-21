﻿using NUnit.Framework;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Reflection;
using System.Collections.Generic;

namespace OpenNefia.Tests.Core.Reflection
{
    public sealed class ReflectionManagerTest : ReflectionManager
    {
        protected override IEnumerable<string> TypePrefixes => new[] { "", "OpenNefia.Tests.", "OpenNefia.Core." };
    }

    [TestFixture]
    public class ReflectionManager_Test : OpenNefiaUnitTest
    {
        protected override void OverrideIoC()
        {
            base.OverrideIoC();

            IoCManager.Register<IReflectionManager, ReflectionManagerTest>(overwrite: true);
        }

        [Test]
        public void ReflectionManager_TestGetAllChildren()
        {
            IReflectionManager reflectionManager = IoCManager.Resolve<IReflectionManager>();

            // I have no idea how to better do this.
            bool did1 = false;
            bool did2 = false;
            foreach (var type in reflectionManager.GetAllChildren<IReflectionManagerTest>())
            {
                if (!did1 && type == typeof(TestClass1))
                {
                    did1 = true;
                }
                else if (!did2 && type == typeof(TestClass2))
                {
                    did2 = true;
                }
                else if (type == typeof(TestClass3))
                {
                    // Not possible since it has [Reflect(false)]
                    Assert.Fail("ReflectionManager returned the [Reflect(false)] class.");
                }
                else if (type == typeof(TestClass4))
                {
                    Assert.Fail("ReflectionManager returned the abstract class");
                }
                else
                {
                    Assert.Fail("ReflectionManager returned too many types.");
                }
            }
            Assert.That(did1 && did2, Is.True, "IoCManager did not return both expected types. First: {0}, Second: {1}", did1, did2);
        }

        public interface IReflectionManagerTest { }

        // These two pass like normal.
        public class TestClass1 : IReflectionManagerTest { }
        public class TestClass2 : IReflectionManagerTest { }

        // These two should both NOT be passed.
        [Reflect(false)]
        public class TestClass3 : IReflectionManagerTest { }
        public abstract class TestClass4 : IReflectionManagerTest { }

        [Test]
        public void ReflectionManager_TestGetType()
        {
            IReflectionManager reflectionManager = IoCManager.Resolve<IReflectionManager>();
            Assert.Multiple(() =>
            {
                Assert.That(reflectionManager.GetType("Core.Reflection.TestGetType1"), Is.EqualTo(typeof(TestGetType1)));
                Assert.That(reflectionManager.GetType("Core.Reflection.TestGetType2"), Is.EqualTo(typeof(TestGetType2)));
                Assert.That(reflectionManager.GetType("Core.Reflection.ITestGetType3"), Is.EqualTo(typeof(ITestGetType3)));
            });
        }
    }

    public class TestGetType1 { }
    public abstract class TestGetType2 { }
    public interface ITestGetType3 { }
}
