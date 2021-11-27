using System.Collections.Generic;

namespace OpenNefia.Core.Serialization.Markdown.Validation
{
    public abstract class ValidationNode
    {
        public abstract bool Valid { get; }

        public abstract IEnumerable<ErrorNode> GetErrors();
    }
}
