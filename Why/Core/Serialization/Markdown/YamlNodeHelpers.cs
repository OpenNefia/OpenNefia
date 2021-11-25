using System;
using Why.Core.Serialization.Markdown.Mapping;
using Why.Core.Serialization.Markdown.Sequence;
using Why.Core.Serialization.Markdown.Value;
using YamlDotNet.RepresentationModel;

namespace Why.Core.Serialization.Markdown
{
    public static class YamlNodeHelpers
    {
        public static DataNode ToDataNode(this YamlNode node)
        {
            return node switch
            {
                YamlScalarNode scalarNode => new ValueDataNode(scalarNode),
                YamlMappingNode mappingNode => new MappingDataNode(mappingNode),
                YamlSequenceNode sequenceNode => new SequenceDataNode(sequenceNode),
                _ => throw new ArgumentOutOfRangeException(nameof(node))
            };
        }

        public static T ToDataNodeCast<T>(this YamlNode node) where T : DataNode
        {
            return (T) node.ToDataNode();
        }

        public static YamlNode ToYamlNode(this DataNode node)
        {
            return node switch
            {
                ValueDataNode valueDataNode => new YamlScalarNode(valueDataNode.Value){Tag = valueDataNode.Tag},
                MappingDataNode mappingDataNode => mappingDataNode.ToYaml(),
                SequenceDataNode sequenceNode => sequenceNode.ToSequenceNode(),
                _ => throw new ArgumentOutOfRangeException(nameof(node))
            };
        }
    }
}
