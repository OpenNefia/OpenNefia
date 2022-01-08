using NUnit.Framework;
using OpenNefia.Core.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Tests.Core.Stats
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(Stat<>))]
    public class Stat_Tests
    {
        [Test]
        public void TestStatEquality()
        {
            Assert.That(new Stat<int>(42), Is.EqualTo(new Stat<int>(42)));
            Assert.That(new Stat<int>(42, 0), Is.EqualTo(new Stat<int>(42, 10000)));
            Assert.That(new Stat<int>(0, 42), Is.Not.EqualTo(new Stat<int>(10000, 42)));
        }

        [Test]
        public void TestStatHashCode()
        {
            Assert.That(new Stat<int>(42).GetHashCode(), Is.EqualTo(new Stat<int>(42).GetHashCode()));
            Assert.That(new Stat<int>(42, 0).GetHashCode(), Is.EqualTo(new Stat<int>(42, 10000).GetHashCode()));
            Assert.That(new Stat<int>(0, 42).GetHashCode(), Is.Not.EqualTo(new Stat<int>(10000, 42).GetHashCode()));
        }

        [Test]
        public void TestStatImplicitOperators()
        {
            var stat = new Stat<int>(12, 34);

            Assert.That((int)stat, Is.EqualTo(12));
            Assert.That(stat == 12, Is.True);
            Assert.That(stat != 12, Is.False);
            Assert.That(stat == 34, Is.False);
            Assert.That(stat != 34, Is.True);
        }

        [Test]
        public void TestStatImplicitConstructor()
        {
            Stat<int> stat = 12;

            Assert.That(stat.Base, Is.EqualTo(12));
            Assert.That(stat.Buffed, Is.EqualTo(12));
        }

        [Test]
        public void TestStatReset()
        {
            var stat = new Stat<int>(12, 34);

            Assert.That(stat.Base, Is.EqualTo(12));
            Assert.That(stat.Buffed, Is.EqualTo(34));

            stat.Reset();

            Assert.That(stat.Base, Is.EqualTo(12));
            Assert.That(stat.Buffed, Is.EqualTo(12));
        }
    }
}
