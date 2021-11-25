using System.Collections.Generic;
using Why.Core.Serialization.Markdown.Sequence;
using Why.Core.Serialization.Markdown.Value;

namespace Why.Core.Serialization.Markdown.Mapping
{
    public static class MappingDataNodeExtensions
    {
        public static MappingDataNode Add(this MappingDataNode mapping, string key, DataNode node)
        {
            mapping.Add(new ValueDataNode(key), node);
            return mapping;
        }

        public static MappingDataNode Add(this MappingDataNode mapping, string key, string value)
        {
            mapping.Add(new ValueDataNode(key), new ValueDataNode(value));
            return mapping;
        }

        public static MappingDataNode Add(this MappingDataNode mapping, string key, List<string> sequence)
        {
            mapping.Add(new ValueDataNode(key), new SequenceDataNode(sequence));
            return mapping;
        }
    }
}
