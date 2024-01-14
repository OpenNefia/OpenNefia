using NUnit.Framework;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Utility;

namespace OpenNefia.Tests.Core.Utility
{
    [TestFixture]
    [TestOf(typeof(NullableHelper))]
    public class NullableHelper_Test
    {
        [SetUp]
        public void Setup()
        {
            //initializing logmanager so it wont error out if nullablehelper logs an error
            var collection = new DependencyCollection();
            collection.Register<ILogManager, LogManager>();
            collection.BuildGraph();
            IoCManager.InitThread(collection, true);
        }

        [Test]
        public void IsNullableTest()
        {
            Assert.Multiple(() =>
            {
                var fields = typeof(NullableTestClass).GetAllFields();
                foreach (var field in fields)
                {
                    Assert.That(NullableHelper.IsMarkedAsNullable(field), Is.True, $"{field}");

                    AbstractFieldInfo afi = new SpecificFieldInfo(field);
                    Assert.That(NullableHelper.IsMarkedAsNullable(afi), Is.True, $"{field}");
                }

                var properties = typeof(NullableTestClass).GetAllProperties();
                foreach (var property in properties)
                {
                    Assert.That(NullableHelper.IsMarkedAsNullable(property), Is.True, $"{property}");

                    AbstractFieldInfo afi = new SpecificPropertyInfo(property);
                    Assert.That(NullableHelper.IsMarkedAsNullable(afi), Is.True, $"{property}");
                }
            });
        }

        [Test]
        public void IsNotNullableTest()
        {
            Assert.Multiple(() =>
            {
                var fields = typeof(NotNullableTestClass).GetAllFields();
                foreach (var field in fields)
                {
                    Assert.That(NullableHelper.IsMarkedAsNullable(field), Is.False, $"{field}");

                    AbstractFieldInfo afi = new SpecificFieldInfo(field);
                    Assert.That(NullableHelper.IsMarkedAsNullable(afi), Is.False, $"{field}");
                }

                var properties = typeof(NotNullableTestClass).GetAllProperties();
                foreach (var property in properties)
                {
                    Assert.That(NullableHelper.IsMarkedAsNullable(property), Is.False, $"{property}");

                    AbstractFieldInfo afi = new SpecificPropertyInfo(property);
                    Assert.That(NullableHelper.IsMarkedAsNullable(afi), Is.False, $"{property}");
                }
            });
        }
    }

#pragma warning disable 169
#pragma warning disable 414
    public class NullableTestClass
    {
        private int? i;
        private double? d;
        public object? o;
        public INullableTestInterface? Itest;
        public NullableTestClass? nTc;
        private char? c;

        private int? pi { get; set; }
        private double? pd { get; set; }
        public object? po { get; set; } 
        public INullableTestInterface? pItest { get; set; }
        public NullableTestClass? pnTc { get; set; }
        private char? pc { get; set; }
    }

    public class NotNullableTestClass
    {
        private int i;
        private double d;
        private object o = null!;
        private INullableTestInterface Itest = null!;
        private NullableTestClass nTc = null!;
        private char c;

        private int pi { get; set; }
        private double pd { get; set; }
        public object po { get; set; } = default!;
        public INullableTestInterface pItest { get; set; } = default!;
        public NullableTestClass pnTc { get; set; } = default!;
        private char pc { get; set; }
    }
#pragma warning restore 414
#pragma warning restore 169

    public interface INullableTestInterface{}
}
