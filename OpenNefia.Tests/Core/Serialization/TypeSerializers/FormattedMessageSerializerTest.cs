using NUnit.Framework;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Implementations;
using OpenNefia.Core.Utility;
using OpenNefia.Core.Utility.Markup;

// ReSharper disable AccessToStaticMemberViaDerivedType

namespace OpenNefia.Tests.Core.Serialization.TypeSerializers;

[TestFixture]
[TestOf(typeof(FormattedMessageSerializer))]
public class FormattedMessageSerializerTest : SerializationTest
{
    [Test]
    [TestCase("message")]
    [TestCase("[color=#FF0000FF]message[/color]")]
    public void SerializationTest(string text)
    {
        var message = new Basic();
        message.AddMarkup(text);
        var node = Serialization.WriteValueAs<ValueDataNode>(message.Render());
        Assert.That(node.Value, NUnit.Framework.Is.EqualTo(text));
    }

    [Test]
    [TestCase("message")]
    [TestCase("[color=#FF0000FF]message[/color]")]
    public void DeserializationTest(string text)
    {
        var node = new ValueDataNode(text);
        var deserializedMessage = Serialization.ReadValueOrThrow<FormattedMessage>(node);
        Assert.That(deserializedMessage.ToMarkup(), NUnit.Framework.Is.EqualTo(text));
    }
}
