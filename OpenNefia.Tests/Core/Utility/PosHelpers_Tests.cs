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
    }
}
