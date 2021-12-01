using System.Collections.Generic;
using OpenNefia.Core.Maths;
using NUnit.Framework;

namespace OpenNefia.Tests.Core.Maths
{
    [Parallelizable(ParallelScope.All | ParallelScope.Fixtures)]
    [TestFixture]
    internal class DirectionTest
    {
        private const double Epsilon = 1.0e-8;

        private static IEnumerable<TestCaseData> Sources()
        {
            yield return new TestCaseData(1, 0, Direction.East, Direction.East, Angle.FromDegrees(90));
            yield return new TestCaseData(1, 1, Direction.SouthEast, Direction.South, Angle.FromDegrees(135));
            yield return new TestCaseData(0, 1, Direction.South, Direction.South, Angle.FromDegrees(180));
            yield return new TestCaseData(-1, 1, Direction.SouthWest, Direction.West, Angle.FromDegrees(-135));
            yield return new TestCaseData(-1, 0, Direction.West, Direction.West, Angle.FromDegrees(-90));
            yield return new TestCaseData(-1, -1, Direction.NorthWest, Direction.North, Angle.FromDegrees(-45));
            yield return new TestCaseData(0, -1, Direction.North, Direction.North, Angle.FromDegrees(0));
            yield return new TestCaseData(1, -1, Direction.NorthEast, Direction.East, Angle.FromDegrees(45));
        }

        [Test]
        [TestCaseSource(nameof(Sources))]
        [Parallelizable]
        public void TestDirectionToAngle(float x, float y, Direction dir, Direction cardinalDir, Angle angle)
        {
            double control = angle;
            var val = dir.ToAngle();

            Assert.That(val.Theta, Is.EqualTo(control).Within(Epsilon));
        }

        [Test]
        [TestCaseSource(nameof(Sources))]
        [Parallelizable]
        public void TestDirectionToVec(float x, float y, Direction dir, Direction cardinalDir, Angle angle)
        {
            var control = new Vector2(x, y).Normalized;
            var controlInt = new Vector2i((int)x, (int)y);
            var val = dir.ToVec();
            var intVec = dir.ToIntVec();

            Assert.That(val, Is.Approximately(control));
            Assert.That(intVec, Is.EqualTo(controlInt));
        }

        [Test]
        [Parallelizable]
        public void TestDirectionOffset()
        {
            var v = new Vector2i(1, 1);
            var expected = new Vector2i(2, 0);
            var dir = Direction.NorthEast;

            Assert.That(v.Offset(dir), Is.EqualTo(expected));
        }

        [Test]
        [TestCaseSource(nameof(Sources))]
        [Parallelizable]
        public void TestVecToAngle(float x, float y, Direction dir, Direction cardinalDir, Angle angle)
        {
            var target = new Vector2(x, y).Normalized;

            Assert.That(target.ToWorldAngle().Reduced(), Is.Approximately(new Angle(angle)));
        }

        [Test]
        [TestCaseSource(nameof(Sources))]
        [Parallelizable]
        public void TestVector2ToDirection(float x, float y, Direction dir, Direction cardinalDir, Angle angle)
        {
            var target = new Vector2(x, y).Normalized;
            var targetInt = new Vector2i((int)x, (int)y);

            Assert.That(target.GetDir(), Is.EqualTo(dir));
            Assert.That(targetInt.GetDir(), Is.EqualTo(dir));
        }

        [Test]
        [TestCaseSource(nameof(Sources))]
        [Parallelizable]
        public void TestVector2ToCardinalDirection(float x, float y, Direction dir, Direction cardinalDir, Angle angle)
        {
            var targetInt = new Vector2i((int)x, (int)y);

            Assert.That(targetInt.GetCardinalDir(), Is.EqualTo(cardinalDir));
        }

        [Test]
        [TestCaseSource(nameof(Sources))]
        [Parallelizable]
        public void TestAngleToDirection(float x, float y, Direction dir, Direction _, Angle angle)
        {
            var target = new Angle(angle);

            Assert.That(target.GetDir(), Is.EqualTo(dir));
        }
    }
}
