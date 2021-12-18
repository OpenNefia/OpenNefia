using NUnit.Framework;
using OpenNefia.Core.Utility;
using System.Text;

namespace OpenNefia.Tests.Core.Utility
{
    [TestFixture, Parallelizable]
    [TestOf(typeof(StringHelpers))]
    public class StringHelpers_Test
    {
        [Test]
        public void TestGetWideWidth()
        {
            Assert.That(Rune.GetRuneAt("ほ", 0).GetWideWidth(), Is.EqualTo(2));
            Assert.That(Rune.GetRuneAt("　", 0).GetWideWidth(), Is.EqualTo(2));
            Assert.That(Rune.GetRuneAt("a", 0).GetWideWidth(), Is.EqualTo(1));

            // East Asian Ambiguous
            Assert.That(Rune.GetRuneAt("Æ", 0).GetWideWidth(), Is.EqualTo(1));
        }

        [Test]
        public void TestWideSub()
        {
            Assert.That("abcdÆ".WideSubstring(0, 3), Is.EqualTo("abcdÆ".Substring(0, 3)));
            Assert.That("abcdÆ".WideSubstring(2, 3), Is.EqualTo("abcdÆ".Substring(2, 3)));
            Assert.That("ほげaぴよ".WideSubstring(0, 0), Is.EqualTo(""));
            Assert.That("ほげaぴよ".WideSubstring(0, 1), Is.EqualTo(""));
            Assert.That("ほげaぴよ".WideSubstring(0, 2), Is.EqualTo("ほ"));
            Assert.That("ほげaぴよ".WideSubstring(0, 3), Is.EqualTo("ほ"));
            Assert.That("ほげaぴよ".WideSubstring(0, 4), Is.EqualTo("ほげ"));
            Assert.That("ほげaぴよ".WideSubstring(0, 5), Is.EqualTo("ほげa"));
            Assert.That("ほげaぴよ".WideSubstring(0, 6), Is.EqualTo("ほげa"));
            Assert.That("ほげaぴよ".WideSubstring(0, 7), Is.EqualTo("ほげaぴ"));
            Assert.That("ほげaぴよ".WideSubstring(0, 8), Is.EqualTo("ほげaぴ"));
            Assert.That("ほげaぴよ".WideSubstring(0, 9), Is.EqualTo("ほげaぴよ"));
            Assert.That("ほげaぴよ".WideSubstring(0, 10), Is.EqualTo("ほげaぴよ"));
            Assert.That("ほげaぴよ".WideSubstring(0, 99), Is.EqualTo("ほげaぴよ"));
            Assert.That("ほげaぴよ".WideSubstring(99, 0), Is.EqualTo(""));
            Assert.That("ほげaぴよ".WideSubstring(0), Is.EqualTo("ほげaぴよ"));
            Assert.That("ほげaぴよ".WideSubstring(3, 6), Is.EqualTo("aぴよ"));
            Assert.That("ほげaぴよ".WideSubstring(3), Is.EqualTo("aぴよ"));
        }
    }
}
