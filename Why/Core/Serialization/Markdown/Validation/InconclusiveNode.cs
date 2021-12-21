using System.Collections.Generic;
using System.Linq;

namespace Why.Core.Serialization.Markdown.Validation
{
    public class InconclusiveNode : ValidationNode
    {
        public InconclusiveNode(DataNode dataNode)
        {
            DataNode = dataNode;
        }

        public DataNode DataNode { get; }

        public override bool Valid => true;

        public override IEnumerable<ErrorNode> GetErrors() => Enumerable.Empty<ErrorNode>();

        public override string? ToString()
        {
            return DataNode.ToString();
        }
    }
}
