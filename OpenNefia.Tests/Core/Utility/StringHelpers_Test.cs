using NUnit.Framework;
using OpenNefia.Core.Utility;
using System.Text;

namespace OpenNefia.Tests.Core.Utility
{
    [Parallelizable(ParallelScope.All | ParallelScope.Fixtures)]
    [TestFixture]
    [TestOf(typeof(StringHelpers))]
    public class StringHelpers_Test
    {
        [Test]
        public void ToCamelCaseTest()
        {
            Assert.That("URLValue".ToLowerCamelCase(), Is.EqualTo("urlValue"));
            Assert.That("URL".ToLowerCamelCase(), Is.EqualTo("url"));
            Assert.That("ID".ToLowerCamelCase(), Is.EqualTo("id"));
            Assert.That("I".ToLowerCamelCase(), Is.EqualTo("i"));
            Assert.That("".ToLowerCamelCase(), Is.EqualTo(""));
            Assert.That("Person".ToLowerCamelCase(), Is.EqualTo("person"));
            Assert.That("iPhone".ToLowerCamelCase(), Is.EqualTo("iPhone"));
            Assert.That("IPhone".ToLowerCamelCase(), Is.EqualTo("iPhone"));
            Assert.That("I Phone".ToLowerCamelCase(), Is.EqualTo("i Phone"));
            Assert.That("I  Phone".ToLowerCamelCase(), Is.EqualTo("i  Phone"));
            Assert.That(" IPhone".ToLowerCamelCase(), Is.EqualTo(" IPhone"));
            Assert.That(" IPhone ".ToLowerCamelCase(), Is.EqualTo(" IPhone "));
            Assert.That("IsCIA".ToLowerCamelCase(), Is.EqualTo("isCIA"));
            Assert.That("VmQ".ToLowerCamelCase(), Is.EqualTo("vmQ"));
            Assert.That("Xml2Json".ToLowerCamelCase(), Is.EqualTo("xml2Json"));
            Assert.That("SnAkEcAsE".ToLowerCamelCase(), Is.EqualTo("snAkEcAsE"));
            Assert.That("SnA__kEcAsE".ToLowerCamelCase(), Is.EqualTo("snA__kEcAsE"));
            Assert.That("SnA__ kEcAsE".ToLowerCamelCase(), Is.EqualTo("snA__ kEcAsE"));
            Assert.That("already_snake_case_ ".ToLowerCamelCase(), Is.EqualTo("already_snake_case_ "));
            Assert.That("IsJSONProperty".ToLowerCamelCase(), Is.EqualTo("isJSONProperty"));
            Assert.That("SHOUTING_CASE".ToLowerCamelCase(), Is.EqualTo("shoutinG_CASE"));
            Assert.That("9999-12-31T23:59:59.9999999Z".ToLowerCamelCase(), Is.EqualTo("9999-12-31T23:59:59.9999999Z"));
            Assert.That("Hi!! This is text. Time to test.".ToLowerCamelCase(), Is.EqualTo("hi!! This is text. Time to test."));
            Assert.That("BUILDING".ToLowerCamelCase(), Is.EqualTo("building"));
            Assert.That("BUILDING Property".ToLowerCamelCase(), Is.EqualTo("building Property"));
            Assert.That("Building Property".ToLowerCamelCase(), Is.EqualTo("building Property"));
            Assert.That("BUILDING PROPERTY".ToLowerCamelCase(), Is.EqualTo("building PROPERTY"));
        }

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

        [Test]
        public void TestWidePadRight()
        {
            Assert.That("ほげ".WidePadRight(6), Is.EqualTo("ほげ  "));
            Assert.That("ほげ".WidePadRight(6, 'a'), Is.EqualTo("ほげaa"));
            Assert.That("ほげ".WidePadRight(7), Is.EqualTo("ほげ   "));
            Assert.That("ほげ".WidePadRight(0), Is.EqualTo("ほげ"));
            Assert.That("ほげ".WidePadRight(-99), Is.EqualTo("ほげ"));
            Assert.That("ほげa".WidePadRight(5), Is.EqualTo("ほげa"));
            Assert.That("ほげa".WidePadRight(6), Is.EqualTo("ほげa "));
        }
    }
}
