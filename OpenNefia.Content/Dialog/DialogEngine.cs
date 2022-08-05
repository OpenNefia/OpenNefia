using NativeFileDialogSharp;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Dialog
{
    public interface IDialogEngine
    {
        DialogPrototype Dialog { get; }
        EntityUid Player { get; }
        EntityUid? Speaker { get; }
        IDialogLayer DialogLayer { get; }
        Blackboard<IDialogExtraData> Data { get; }

        TurnResult StartDialog();
        QualifiedDialogNode GetNodeByID(QualifiedDialogNodeID nodeID);
        QualifiedDialogNode GetNodeByID(PrototypeId<DialogPrototype> protoID, string nodeID);
    }

    public interface IDialogExtraData
    {
    }

    public record struct QualifiedDialogNodeID(PrototypeId<DialogPrototype> DialogID, string NodeID)
    {
        public static QualifiedDialogNodeID Empty => new(new(""), "");
    }
    public sealed record QualifiedDialogNode(PrototypeId<DialogPrototype> DialogID, IDialogNode Node);

    public sealed class DialogEngine : IDialogEngine
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;

        public DialogPrototype Dialog { get; private set; }
        public EntityUid Player { get; private set; }
        public EntityUid? Speaker { get; private set; }
        public IDialogLayer DialogLayer { get; }
        public Blackboard<IDialogExtraData> Data { get; }

        public DialogEngine(EntityUid player, EntityUid? target, DialogPrototype proto, IDialogLayer dialogLayer)
        {
            EntitySystem.InjectDependencies(this);

            Player = player;
            Speaker = target;
            Dialog = proto;
            DialogLayer = dialogLayer;
            Data = new();
        }

        public QualifiedDialogNode GetNodeByID(QualifiedDialogNodeID nodeID)
        {
            if (!_protos.TryIndex(nodeID.DialogID, out var dialog))
                throw new InvalidDataException($"Dialog with ID {nodeID.DialogID} not found.");

            if (!dialog.Nodes.TryGetValue(nodeID.NodeID, out var node))
                throw new InvalidDataException($"Dialog node {nodeID} not found in dialog {dialog.ID}.");

            return new(dialog.GetStrongID(), node);
        }

        public QualifiedDialogNode GetNodeByID(PrototypeId<DialogPrototype> protoID, string nodeID)
        {
            var dialog = _protos.Index(protoID);

            if (!dialog.Nodes.TryGetValue(nodeID, out var node))
                throw new InvalidDataException($"Dialog node {nodeID} not found in dialog {dialog.ID}.");

            return new(protoID, node);
        }

        public TurnResult StartDialog()
        {
            QualifiedDialogNode? next = GetNodeByID(Dialog.GetStrongID(), Dialog.StartNode);

            while (next != null)
            {
                Dialog = _protos.Index(next.DialogID);
                next = StepDialog(next.Node);
            }

            return TurnResult.Aborted;
        }

        private QualifiedDialogNode? StepDialog(IDialogNode? node)
        {
            var evStepDialog = new BeforeStepDialogEvent(this, node);
            if (_entityManager.IsAlive(Speaker))
                _entityManager.EventBus.RaiseEvent(Speaker.Value, evStepDialog, broadcast: true);
            else
                _entityManager.EventBus.RaiseEvent(evStepDialog);
            node = evStepDialog.OutCurrentNode;

            if (node == null)
                return null;

            var next = node.Invoke(this);

            return next;
        }
    }

    public sealed class BeforeStepDialogEvent : EntityEventArgs
    {
        public IDialogEngine DialogEngine { get; }

        public IDialogNode? OutCurrentNode { get; set; }

        public BeforeStepDialogEvent(IDialogEngine engine, IDialogNode? node)
        {
            DialogEngine = engine;
            OutCurrentNode = node;
        }
    }
}
