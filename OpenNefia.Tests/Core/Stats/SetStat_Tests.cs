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
    [TestOf(typeof(SetStat<,,>))]
    public class SetStat_Tests
    {
        [Test]
        public void TestStatIsBuffed()
        {
            Assert.That(new HashSetStat<int>(new() { 42 }).IsBuffed, Is.False);
            Assert.That(new HashSetStat<int>(new() { 42 }, new() { 0 }).IsBuffed, Is.True);
            Assert.That(new HashSetStat<int>(new() { 0 }, new() { 42 }).IsBuffed, Is.True);
        }

        [Test]
        public void TestStatBaseModification()
        {
            var stat = new HashSetStat<int>(new() { 12 });

            Assert.That(stat.Base, Is.EquivalentTo(new[] { 12 }));
            Assert.That(stat.Buffed, Is.EquivalentTo(new[] { 12 }));
            Assert.That(stat.IsBuffed, Is.False);

            stat.AddBase(14);

            Assert.That(stat.Base, Is.EquivalentTo(new[] { 12, 14 }));
            Assert.That(stat.Buffed, Is.EquivalentTo(new[] { 12, 14 }));
            Assert.That(stat.IsBuffed, Is.False);

            stat.ClearBase();

            Assert.That(stat.Base, Is.EquivalentTo(new int[] {}));
            Assert.That(stat.Buffed, Is.EquivalentTo(new int[] {}));
            Assert.That(stat.IsBuffed, Is.False);

            stat.AddBase(18);

            Assert.That(stat.Base, Is.EquivalentTo(new[] { 18 }));
            Assert.That(stat.Buffed, Is.EquivalentTo(new[] { 18 }));
            Assert.That(stat.IsBuffed, Is.False);

            stat.RemoveBase(18);

            Assert.That(stat.Base, Is.EquivalentTo(new int[] { }));
            Assert.That(stat.Buffed, Is.EquivalentTo(new int[] { }));
            Assert.That(stat.IsBuffed, Is.False);
        }

        [Test]
        public void TestStatBuffedModification()
        {
            var stat = new HashSetStat<int>(new() { 12 });

            Assert.That(stat.Base, Is.EquivalentTo(new[] { 12 }));
            Assert.That(stat.Buffed, Is.EquivalentTo(new[] { 12 }));
            Assert.That(stat.IsBuffed, Is.False);

            stat.AddBase(14);

            Assert.That(stat.Base, Is.EquivalentTo(new[] { 12, 14 }));
            Assert.That(stat.Buffed, Is.EquivalentTo(new[] { 12, 14 }));
            Assert.That(stat.IsBuffed, Is.False);

            stat.Add(16);

            Assert.That(stat.Base, Is.EquivalentTo(new[] { 12, 14 }));
            Assert.That(stat.Buffed, Is.EquivalentTo(new[] { 12, 14, 16 }));
            Assert.That(stat.IsBuffed, Is.True);

            stat.AddBase(18);

            Assert.That(stat.Base, Is.EquivalentTo(new[] { 12, 14, 18 }));
            Assert.That(stat.Buffed, Is.EquivalentTo(new[] { 12, 14, 16 }));
            Assert.That(stat.IsBuffed, Is.True);

            stat.RemoveBase(18);
            stat.AddBase(16);

            Assert.That(stat.Base, Is.EquivalentTo(new[] { 12, 14, 16 }));
            Assert.That(stat.Buffed, Is.EquivalentTo(new[] { 12, 14, 16 }));
            Assert.That(stat.IsBuffed, Is.True);
        }

        [Test]
        public void TestStatReset()
        {
            var stat = new HashSetStat<int>(new() { 12 }, new() { 34 });

            Assert.That(stat.Base, Is.EquivalentTo(new[] { 12 }));
            Assert.That(stat.Buffed, Is.EquivalentTo(new[] { 34 }));
            Assert.That(stat.IsBuffed, Is.True);

            stat.Reset();

            Assert.That(stat.Base, Is.EquivalentTo(new[] { 12 }));
            Assert.That(stat.Buffed, Is.EquivalentTo(new[] { 12 }));
            Assert.That(stat.IsBuffed, Is.False);
        }
    }
}
