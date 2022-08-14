using OpenNefia.Content.Dialog;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Manager.Result;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;

namespace OpenNefia.Content.Skills
{
    [TypeSerializer]
    public class QualifiedDialogNodeIDSerializer : ITypeSerializer<QualifiedDialogNodeID, ValueDataNode>
    {
        public DeserializationResult Read(
            ISerializationManager serializationManager,
            ValueDataNode node,
            IDependencyCollection dependencies, bool skipHook,
            ISerializationContext? context = null)
        {
            var split = node.Value.Split(':');
            if (split.Length != 2)
                throw new ArgumentException($"Dialog node must be in format 'DialogID:NodeID', got: {node.Value}");

            var dialogID = new PrototypeId<DialogPrototype>(split[0]);
            var nodeID = split[1];

            return new DeserializedValue<QualifiedDialogNodeID>(new QualifiedDialogNodeID(dialogID, nodeID));
        }

        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            var split = node.Value.Split(':');
            if (split.Length != 2)
                return new ErrorNode(node, "Dialog node must be in format 'DialogID:NodeID'");

            var dialogID = new PrototypeId<DialogPrototype>(split[0]);
            var nodeID = split[1];

            var protos = dependencies.Resolve<IPrototypeManager>();
            if (!protos.TryIndex(dialogID, out var dialogProto))
                throw new ArgumentException($"{nameof(DialogPrototype)} with ID {dialogID} not found.");

            if (!dialogProto.Nodes.ContainsKey(nodeID))
                throw new ArgumentException($"{nameof(DialogPrototype)} with ID {dialogID} does not contain node ID {nodeID}.");

            return new ValidatedValueNode(node);
        }

        public DataNode Write(ISerializationManager serializationManager, QualifiedDialogNodeID value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return serializationManager.GetDefinition<QualifiedDialogNodeID>()!
                .Serialize(value, serializationManager, context, alwaysWrite);
        }

        public QualifiedDialogNodeID Copy(ISerializationManager serializationManager, QualifiedDialogNodeID source, QualifiedDialogNodeID target,
            bool skipHook,
            ISerializationContext? context = null)
        {
            return source;
        }

        public bool Compare(ISerializationManager serializationManager, QualifiedDialogNodeID left, QualifiedDialogNodeID right, bool skipHook,
            ISerializationContext? context = null)
        {
            return left.DialogID == right.DialogID && left.NodeID == right.NodeID;
        }
    }
}
