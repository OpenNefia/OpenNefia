using OpenNefia.Core.Serialization.Manager;
using YamlDotNet.Core;

namespace OpenNefia.Core.Serialization.Markdown
{
    public abstract class DataNode
    {
        public string? Tag;
        public NodeMark Start;
        public NodeMark End;

        public DataNode(NodeMark start, NodeMark end)
        {
            Start = start;
            End = end;
        }

        public abstract bool IsEmpty { get; }

        public abstract DataNode Copy();

        public abstract DataNode? Except(DataNode node);

        public abstract DataNode PushInheritance(DataNode parent);

        public override bool Equals(object? obj)
        {
            if (obj is not DataNode other)
                return false;

            // mapping and sequences nodes are equal if removing duplicate entires leaves us with nothing. Value nodes
            // override this and directly check equality.
            return Except(other) == null;
        }

        public T CopyCast<T>() where T : DataNode
        {
            return (T)Copy();
        }
    }

    public abstract class DataNode<T> : DataNode where T : DataNode<T>
    {
        protected DataNode(NodeMark start, NodeMark end) : base(start, end)
        {
        }

        public abstract override T Copy();

        public abstract T? Except(T node);

        public abstract T PushInheritance(T node);

        public override DataNode? Except(DataNode node)
        {
            return node is not T tNode ? throw new InvalidNodeTypeException() : Except(tNode);
        }

        public override DataNode PushInheritance(DataNode parent)
        {
            return parent is not T tNode ? throw new InvalidNodeTypeException() : PushInheritance(tNode);
        }
    }
}