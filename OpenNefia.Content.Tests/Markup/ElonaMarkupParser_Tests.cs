using NuGet.ContentModel;
using NUnit.Framework;
using OpenNefia.Content.Markup;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Content.Tests.Markup
{
    [TestFixture]
    [TestOf(typeof(ElonaMarkupParser))]
    public class ElonaMarkupParser_Tests
    {
        [Test]
        public void TestParse()
        {
            var markupStr = $@"<color=#404040><size=14><style=bold>This is a line of text
<size=10>This is more text
This is plain text";

            var parser = new ElonaMarkupParser();
            var markup = parser.Parse(markupStr);

            Assert.That(markup.Lines.Count, Is.EqualTo(3));

            Assert.That(markup.Lines[0].Text, Is.EqualTo("This is a line of text"));
            Assert.That(markup.Lines[0].Font.Color, Is.EqualTo(Color.FromHex("#404040")));
            Assert.That(markup.Lines[0].Font.Size, Is.EqualTo(14));
            Assert.That(markup.Lines[0].Font.Style, Is.EqualTo(FontStyle.Bold));

            Assert.That(markup.Lines[1].Text, Is.EqualTo("This is more text"));
            Assert.That(markup.Lines[1].Font.Color, Is.EqualTo(Color.Black));
            Assert.That(markup.Lines[1].Font.Size, Is.EqualTo(10));
            Assert.That(markup.Lines[1].Font.Style, Is.EqualTo(FontStyle.None));

            Assert.That(markup.Lines[2].Text, Is.EqualTo("This is plain text"));
            Assert.That(markup.Lines[2].Font.Color, Is.EqualTo(Color.Black));
            Assert.That(markup.Lines[2].Font.Size, Is.EqualTo(12));
            Assert.That(markup.Lines[2].Font.Style, Is.EqualTo(FontStyle.None));
        }
    }
}
