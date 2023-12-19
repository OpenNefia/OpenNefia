using NUnit.Framework;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Utility;

namespace OpenNefia.Tests.Core.Utility
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(PosHelpers))]
    public class PosHelpers_Tests
    {
        public record class EnumerateLineTestCase(Vector2i From, Vector2i To, Vector2i[] Expected);
        
        public static IEnumerable<EnumerateLineTestCase> EnumerateLineTestCases = new EnumerateLineTestCase[]
        {
#pragma warning disable format
            new((0, 0), (0, 0),  new Vector2i[] { (0, 0) }),
            new((0, 0), (3, 0),  new Vector2i[] { (0, 0), (1, 0), (2, 0), (3, 0) }),
            new((0, 0), (-3, 0), new Vector2i[] { (0, 0), (-1, 0), (-2, 0), (-3, 0) }),
            new((0, 0), (0, 3),  new Vector2i[] { (0, 0), (0, 1), (0, 2), (0, 3) }),
            new((0, 0), (0, -3), new Vector2i[] { (0, 0), (0, -1), (0, -2), (0, -3) }),
            new((0, 0), (3, 3),  new Vector2i[] { (0, 0), (1, 1), (2, 2), (3, 3) }),
            new((0, 0), (-3, 3), new Vector2i[] { (0, 0), (-1, 1), (-2, 2), (-3, 3) }),
            new((0, 0), (3, 3),  new Vector2i[] { (0, 0), (1, 1), (2, 2), (3, 3) }),
            new((0, 0), (3, -3), new Vector2i[] { (0, 0), (1, -1), (2, -2), (3, -3) }),
            new((0, 0), (3, 6),  new Vector2i[] { (0, 0), (0, 1), (1, 2), (1, 3), (2, 4), (2, 5), (3, 6) }),
            new((0, 0), (-3, 6), new Vector2i[] { (0, 0), (0, 1), (-1, 2), (-1, 3), (-2, 4), (-2, 5), (-3, 6) }),
            new((0, 0), (3, 6),  new Vector2i[] { (0, 0), (0, 1), (1, 2), (1, 3), (2, 4), (2, 5), (3, 6) }),
            new((0, 0), (3, -6), new Vector2i[] { (0, 0), (0, -1), (1, -2), (1, -3), (2, -4), (2, -5), (3, -6) }),
            new((0, 0), (6, 3),  new Vector2i[] { (0, 0), (1, 0), (2, 1), (3, 1), (4, 2), (5, 2), (6, 3) }),
            new((0, 0), (-3, 3), new Vector2i[] { (0, 0), (-1, 1), (-2, 2), (-3, 3) }),
            new((0, 0), (6, 3),  new Vector2i[] { (0, 0), (1, 0), (2, 1), (3, 1), (4, 2), (5, 2), (6, 3) }),
            new((0, 0), (6, -3), new Vector2i[] { (0, 0), (1, 0), (2, -1), (3, -1), (4, -2), (5, -2), (6, -3) }),
            new((0, 0), (9, 2),  new Vector2i[] { (0, 0), (1, 0), (2, 0), (3, 1), (4, 1), (5, 1), (6, 1), (7, 2), (8, 2), (9, 2) }),
            new((0, 0), (-9, 2), new Vector2i[] { (0, 0), (-1, 0), (-2, 0), (-3, 1), (-4, 1), (-5, 1), (-6, 1), (-7, 2), (-8, 2), (-9, 2) }),
            new((0, 0), (9, 2),  new Vector2i[] { (0, 0), (1, 0), (2, 0), (3, 1), (4, 1), (5, 1), (6, 1), (7, 2), (8, 2), (9, 2) }),
            new((0, 0), (9, -2), new Vector2i[] { (0, 0), (1, 0), (2, 0), (3, -1), (4, -1), (5, -1), (6, -1), (7, -2), (8, -2), (9, -2) }),
#pragma warning restore format
        };

        [Test]
        public void Test_EnumerateLine([ValueSource(nameof(EnumerateLineTestCases))] EnumerateLineTestCase data)
        {
            Assert.That(PosHelpers.EnumerateLine(data.From, data.To, includeStartPos: true).ToArray(), Is.EquivalentTo(data.Expected));
        }

        public record class EnumerateBallTestCase(Vector2i Origin, int Range, UIBox2i Bounds, Vector2i[] Expected);

        public static IEnumerable<EnumerateBallTestCase> EnumerateBallTestCases = new EnumerateBallTestCase[]
        {
#pragma warning disable format
            new((10, 10), 0, new UIBox2i(0, 0, 20, 20), new Vector2i[] { (10, 10) }),
            new((10, 10), 1, new UIBox2i(0, 0, 20, 20), new Vector2i[] { (9, 9), (10, 9), (11, 9), (9, 10), (10, 10), (11, 10), (9, 11), (10, 11), (11, 11) }),
            new((10, 10), 2, new UIBox2i(0, 0, 20, 20), new Vector2i[] { (8, 8), (9, 8), (10, 8), (11, 8), (12, 8), (8, 9), (9, 9), (10, 9), (11, 9), (12, 9), (8, 10), (9, 10), (10, 10), (11, 10), (12, 10), (8, 11), (9, 11), (10, 11), (11, 11), (12, 11), (8, 12), (9, 12), (10, 12), (11, 12), (12, 12) }),
            new((10, 10), 3, new UIBox2i(0, 0, 20, 20), new Vector2i[] { (8, 7), (9, 7), (10, 7), (11, 7), (12, 7), (7, 8), (8, 8), (9, 8), (10, 8), (11, 8), (12, 8), (13, 8), (7, 9), (8, 9), (9, 9), (10, 9), (11, 9), (12, 9), (13, 9), (7, 10), (8, 10), (9, 10), (10, 10), (11, 10), (12, 10), (13, 10), (7, 11), (8, 11), (9, 11), (10, 11), (11, 11), (12, 11), (13, 11), (7, 12), (8, 12), (9, 12), (10, 12), (11, 12), (12, 12), (13, 12), (8, 13), (9, 13), (10, 13), (11, 13), (12, 13) }),
            new((10, 10), 4, new UIBox2i(0, 0, 20, 20), new Vector2i[] { (8, 6), (9, 6), (10, 6), (11, 6), (12, 6), (7, 7), (8, 7), (9, 7), (10, 7), (11, 7), (12, 7), (13, 7), (6, 8), (7, 8), (8, 8), (9, 8), (10, 8), (11, 8), (12, 8), (13, 8), (14, 8), (6, 9), (7, 9), (8, 9), (9, 9), (10, 9), (11, 9), (12, 9), (13, 9), (14, 9), (6, 10), (7, 10), (8, 10), (9, 10), (10, 10), (11, 10), (12, 10), (13, 10), (14, 10), (6, 11), (7, 11), (8, 11), (9, 11), (10, 11), (11, 11), (12, 11), (13, 11), (14, 11), (6, 12), (7, 12), (8, 12), (9, 12), (10, 12), (11, 12), (12, 12), (13, 12), (14, 12), (7, 13), (8, 13), (9, 13), (10, 13), (11, 13), (12, 13), (13, 13), (8, 14), (9, 14), (10, 14), (11, 14), (12, 14) }),

#pragma warning restore format
        };

        [Test]
        public void Test_EnumerateBallPositions([ValueSource(nameof(EnumerateBallTestCases))] EnumerateBallTestCase data)
        {
            Assert.That(PosHelpers.EnumerateBallPositions(data.Origin, data.Range, data.Bounds, includeStartPos: true).ToArray(), Is.EquivalentTo(data.Expected));
        }
    }
}
