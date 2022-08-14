using NUnit.Framework;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Tests.Core.Serialization
{
    [TestFixture]
    [TestOf(typeof(SerializationManager))]
    public class SerializationComparisonTest : OpenNefiaUnitTest
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            var serializationManager = IoCManager.Resolve<ISerializationManager>();
            serializationManager.Initialize();
        }

        [Test]
        public void TestCompareNullable()
        {
            var serializationManager = IoCManager.Resolve<ISerializationManager>();

            Assert.Multiple(() =>
            {
                Assert.That(serializationManager.Compare(null, null), Is.True);
                Assert.That(serializationManager.Compare("foo", null), Is.False);
                Assert.That(serializationManager.Compare(null, "foo"), Is.False);
            });
        }

        [Test]
        public void TestComparePrimitives()
        {
            var serializationManager = IoCManager.Resolve<ISerializationManager>();

            Assert.Multiple(() =>
            {
                Assert.That(serializationManager.Compare(true, true), Is.True);
                Assert.That(serializationManager.Compare(1, 1), Is.True);
                Assert.That(serializationManager.Compare(1L, 1L), Is.True);
                Assert.That(serializationManager.Compare(1UL, 1UL), Is.True);
                Assert.That(serializationManager.Compare(1f, 1f), Is.True);
                Assert.That(serializationManager.Compare(1d, 1d), Is.True);
                Assert.That(serializationManager.Compare("foo", "foo"), Is.True);

                Assert.That(serializationManager.Compare(1L, 1), Is.False);
                Assert.That(serializationManager.Compare(1L, 1UL), Is.False);
            });
        }

        [Flags]
        public enum TestFlags
        {
            A = 0b0001,
            B = 0b0010,
        }

        [Test]
        public void TestCompareEnums()
        {
            var serializationManager = IoCManager.Resolve<ISerializationManager>();

            Assert.Multiple(() =>
            {
                Assert.That(serializationManager.Compare(TestFlags.A, TestFlags.A), Is.True, "Equal");
                Assert.That(serializationManager.Compare(TestFlags.A, TestFlags.B), Is.False, "Not equal");
                Assert.That(serializationManager.Compare(TestFlags.A | TestFlags.B, TestFlags.B | TestFlags.A), Is.True, "Equal (bitflags)");
            });
        }

        [Test]
        public void TestCompareArrays()
        {
            var serializationManager = IoCManager.Resolve<ISerializationManager>();

            Assert.Multiple(() =>
            {
                Assert.That(serializationManager.Compare(new int[] { 1, 2, 3 }, new int[] { 1, 2, 3 }), Is.True, "Equal");
                Assert.That(serializationManager.Compare(new int[] { 1, 2 }, new int[] { 1, 2, 3 }), Is.False, "Not equal (length)");
                Assert.That(serializationManager.Compare(new int[] { 1, 2, 3 }, new int[] { 4, 2, 3 }), Is.False, "Not equal (elements)");
                Assert.That(serializationManager.Compare(new int[] { 1, 2, 3 }, new long[] { 1, 2, 3 }), Is.False, "Not equal (types)");
            });
        }

        [DataDefinition]
        public class CompareTestClass
        {
            [DataField("value")]
            public int Value { get; set; }
        }

        [DataDefinition]
        public class CompareTestClassInherited : CompareTestClass
        {
        }

        [Test]
        public void TestCompareDifferingTypes()
        {
            var serializationManager = IoCManager.Resolve<ISerializationManager>();

            Assert.Multiple(() =>
            {
                Assert.That(serializationManager.Compare(new CompareTestClass(), new CompareTestClass()), Is.True, "Equal");
                Assert.That(serializationManager.Compare(new CompareTestClass(), new CompareTestClassInherited()), Is.False, "Not equal");
            });
        }

        [Test]
        public void TestCompareFields()
        {
            var serializationManager = IoCManager.Resolve<ISerializationManager>();

            var compLeft = new CompareTestDefinition()
            {
                A = "A",
                B = "B",
            };

            var compRight = new CompareTestDefinition()
            {
                A = "A",
                B = "ビー",
            };

            Assert.That(serializationManager.Compare(compLeft, compRight), Is.True);

            compRight.A = "C";

            Assert.That(serializationManager.Compare(compLeft, compRight), Is.False);
        }

        [Test]
        public void TestCompareDataDefinitions()
        {
            var serializationManager = IoCManager.Resolve<ISerializationManager>();

            var compLeft = new CompareTestNestedDefinition()
            {
                First = new CompareTestDefinition()
                {
                    A = "A",
                    B = "B",
                },
                Second = new CompareTestDefinition()
                {
                    A = "C",
                    B = "D",
                }
            };

            var compRight = new CompareTestNestedDefinition()
            {
                First = new CompareTestDefinition()
                {
                    A = "A",
                    B = "B",
                },
                Second = new CompareTestDefinition()
                {
                    A = "シー",
                    B = "デー",
                }
            };

            Assert.That(serializationManager.Compare(compLeft, compRight), Is.True);

            compRight.First.A = "E";

            Assert.That(serializationManager.Compare(compLeft, compRight), Is.False);
        }

        [DataDefinition]
        public class CompareTestDefinition
        {
            [DataField("A")]
            public string A { get; set; } = string.Empty;

            [DataField("B", noCompare: true)]
            public string B { get; set; } = string.Empty;
        }

        [DataDefinition]
        public class CompareTestNestedDefinition
        {
            [DataField("first")]
            public CompareTestDefinition First { get; set; } = new();

            [DataField("second", noCompare: true)]
            public CompareTestDefinition Second { get; set; } = new();
        }
    }
}
