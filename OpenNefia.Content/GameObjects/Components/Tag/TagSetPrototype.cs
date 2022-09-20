using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;

namespace OpenNefia.Content.GameObjects
{
    /// </summary>
    [Prototype("TagSet")]
    public class TagSetPrototype : IPrototype
    {
        [IdDataField]
        public string ID { get; } = default!;

        [DataField("tags")]
        private HashSet<TagSetEntry> _tags { get; } = new();
        public IReadOnlySet<TagSetEntry> Tags => _tags;
    }

    [DataDefinition]
    public sealed class TagSetEntry
    {
        public TagSetEntry() {}

        public TagSetEntry(PrototypeId<TagPrototype> tag, int weight = DefaultWeight)
        {
            Tag = tag;
            Weight = weight;
        }

        public const int DefaultWeight = 1000;

        [DataField(required: true)]
        public PrototypeId<TagPrototype> Tag { get; }

        [DataField]
        public int Weight { get; set; } = DefaultWeight;
    }
}

