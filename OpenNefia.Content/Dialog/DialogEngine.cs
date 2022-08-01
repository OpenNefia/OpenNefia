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
        EntityUid? Target { get; }
        IDialogLayer DialogLayer { get; }
        Blackboard<IDialogData> Data { get; }

        TurnResult StartDialog();
        QualifiedDialogNode GetNodeByID(string nodeID);
    }

    public interface IDialogData
    {
    }

    public sealed record QualifiedDialogNode(DialogPrototype Proto, IDialogNode Node);

    public sealed class DialogEngine : IDialogEngine
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;

        public DialogPrototype Dialog { get; private set; }
        public EntityUid? Target { get; private set; }
        public IDialogLayer DialogLayer { get; }
        public Blackboard<IDialogData> Data { get; }

        public DialogEngine(EntityUid? target, DialogPrototype proto, IDialogLayer dialogLayer)
        {
            Target = target;
            Dialog = proto;
            DialogLayer = dialogLayer;
            Data = new();
        }

        public QualifiedDialogNode GetNodeByID(string nodeID)
        {
            var dialog = Dialog;
            
            if (nodeID.IndexOf(':') != -1)
            {
                var split = nodeID.Split(':');
                var dialogID = split[0];
                nodeID = split[1];

                if (!_protos.TryIndex(new PrototypeId<DialogPrototype>(dialogID), out dialog))
                    throw new InvalidDataException($"Dialog with ID {dialogID} not found.");
            }

            if (!dialog.Nodes.TryGetValue(nodeID, out var node))
                throw new InvalidDataException($"Dialog node {nodeID} not found in dialog {dialog.ID}.");

            return new(dialog, node);
        }

        public TurnResult StartDialog()
        {
            QualifiedDialogNode? next = GetNodeByID(Dialog.StartNode);
            
            while (next != null)
            {
                Dialog = next.Proto;
                next = StepDialog(next.Node);
            }

            return TurnResult.Aborted;
        }

        private QualifiedDialogNode? StepDialog(IDialogNode? node)
        {
            var evStepDialog = new BeforeStepDialogEvent(this, node);
            if (_entityManager.IsAlive(Target))
                _entityManager.EventBus.RaiseEvent(Target.Value, evStepDialog, broadcast: true);
            else
                _entityManager.EventBus.RaiseEvent(evStepDialog);
            node = evStepDialog.OutCurrentNode;

            if (node == null)
                return null;

            var nextNodeID = node.Invoke(this);
            if (nextNodeID == null)
                return null;

            var next = GetNodeByID(nextNodeID);

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
