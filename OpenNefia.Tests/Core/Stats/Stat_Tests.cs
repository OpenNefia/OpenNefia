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
        public void TestStatIsBuffed()
        {
            Assert.That(new Stat<int>(42).IsBuffed, Is.False);
            Assert.That(new Stat<int>(42, 0).IsBuffed, Is.True);
            Assert.That(new Stat<int>(0, 42).IsBuffed, Is.True);
        }

        [Test]
        public void TestStatBuffedAssignment()
        {
            var stat = new Stat<int>(12);

            Assert.That(stat.Base, Is.EqualTo(12));
            Assert.That(stat.Buffed, Is.EqualTo(12));
            Assert.That(stat.IsBuffed, Is.False);

            stat.Base = 5;

            Assert.That(stat.Base, Is.EqualTo(5));
            Assert.That(stat.Buffed, Is.EqualTo(5));
            Assert.That(stat.IsBuffed, Is.False);

            stat.Buffed = 6;

            Assert.That(stat.Base, Is.EqualTo(5));
            Assert.That(stat.Buffed, Is.EqualTo(6));
            Assert.That(stat.IsBuffed, Is.True);

            stat.Base = 7;

            Assert.That(stat.Base, Is.EqualTo(7));
            Assert.That(stat.Buffed, Is.EqualTo(6));
            Assert.That(stat.IsBuffed, Is.True);

            stat.Base = 6;

            Assert.That(stat.Base, Is.EqualTo(6));
            Assert.That(stat.Buffed, Is.EqualTo(6));
            Assert.That(stat.IsBuffed, Is.True);
        }

        [Test]
        public void TestStatReset()
        {
            var stat = new Stat<int>(12, 34);

            Assert.That(stat.Base, Is.EqualTo(12));
            Assert.That(stat.Buffed, Is.EqualTo(34));
            Assert.That(stat.IsBuffed, Is.True);

            stat.Reset();

            Assert.That(stat.Base, Is.EqualTo(12));
            Assert.That(stat.Buffed, Is.EqualTo(12));
            Assert.That(stat.IsBuffed, Is.False);
        }
    }
}
