using NUnit.Framework;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Implementations;
using OpenNefia.Core.Utility;

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
        var message = FormattedMessage.FromMarkup(text);
        var node = Serialization.WriteValueAs<ValueDataNode>(message);
        Assert.That(node.Value, Is.EqualTo(text));
    }

    [Test]
    [TestCase("message")]
    [TestCase("[color=#FF0000FF]message[/color]")]
    public void DeserializationTest(string text)
    {
        var node = new ValueDataNode(text);
        var deserializedMessage = Serialization.ReadValueOrThrow<FormattedMessage>(node);
        Assert.That(deserializedMessage.ToMarkup(), Is.EqualTo(text));
    }
}
