using System.Collections.Generic;

namespace Why.Core.Serialization.Markdown.Validation
{
    public abstract class ValidationNode
    {
        public abstract bool Valid { get; }

        public abstract IEnumerable<ErrorNode> GetErrors();
    }
}
