using NativeFileDialogSharp;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Dialog
{
    /*
     * Implementing an extensible dialog system for Elona is a difficult problem that has always
     * vexed me. This is due to the fact that it sits squarely at the intersection of localization,
     * declarative programming and one-off scripting logic. The current implementation is probably
     * going to be more complex than it needs to be before the usability concerns are ironed out, so
     * it will likely undergo substantial changes in the near future.
     */

    /// <summary>
    /// Responsible for stepping through dialog nodes and applying branch/other logic contained
    /// within each.
    /// </summary>
    public interface IDialogEngine
    {
        /// <summary>
        /// Prototype of the current dialog. This can be changed to something different from the
        /// original dialog within the current dialog session depending on what nodes are entered.
        /// </summary>
        DialogPrototype Dialog { get; }

        /// <summary>
        /// Character who initiated the dialog. Always the current player for now; however this
        /// could potentially be used to allow an ally to speak on the behalf of the player sometime
        /// in the future.
        /// </summary>
        EntityUid Player { get; }

        /// <summary>
        /// Entity acting as the speaker. This can be null if there is no speaker.
        /// </summary>
        EntityUid? Speaker { get; set; }

        /// <summary>
        /// Dialog layer that will handle presenting text and choices.
        /// </summary>
        IDialogLayer DialogLayer { get; }

        /// <summary>
        /// Extra data that dialog nodes can set, in case more advanced behavior spanning multiple
        /// nodes is required.
        /// </summary>
        Blackboard<IDialogExtraData> Data { get; }

        /// <summary>
        /// Starts a new dialog session with this engine's given parameters.
        /// </summary>
        /// <returns>
        /// <para>
        /// Turn result to be applied when control is returned to the player.
        /// </para> 
        /// <para>
        /// Usually <see cref="TurnResult.Succeeded"/>, but in cases such as when the player is
        /// teleported to another map following the conversation, <see cref="TurnResult.Aborted"/>
        /// could be returned instead.
        /// </para>
        /// </returns>
        TurnResult StartDialog(QualifiedDialogNodeID nodeID);

        /// <summary>
        /// Retrieves a node in this dialog with the given ID. This query is global across *all*
        /// known dialog prototypes, not just those in <see cref="IDialogEngine.Dialog"/>.
        /// </summary>
        /// <param name="nodeID">The qualified dialog node ID.</param>
        /// <returns>The dialog node.</returns>
        QualifiedDialogNode GetNodeByID(QualifiedDialogNodeID nodeID);

        /// <summary>
        /// Retrieves a node in this dialog with the given ID. This query is global across *all*
        /// known dialog prototypes, not just those in <see cref="IDialogEngine.Dialog"/>.
        /// </summary>
        /// <param name="protoID">The prototype ID of the dialog containing the node.</param>
        /// <param name="nodeID">The node's short ID.</param>
        /// <returns>The dialog node.</returns>
        QualifiedDialogNode GetNodeByID(PrototypeId<DialogPrototype> protoID, string nodeID);
    }

    /// <summary>
    /// Arbitrary data that can be passed along by the dialog engine. It is stored in the dialog
    /// engine's <see cref="IDialogEngine.Data"/> structure.
    /// </summary>
    /// <remarks>
    /// An example of this would be the selected skill ID when speaking to a trainer. This is
    /// persisted for when the training is applied in a later dialog node.
    /// </remarks>
    [ImplicitDataDefinitionForInheritors]
    public interface IDialogExtraData
    {
    }

    /// <summary>
    /// A dialog node ID that is qualified with the ID of the dialog that contains it.
    /// </summary>
    /// <param name="DialogID">Containing dialog.</param>
    /// <param name="NodeID">Node ID in the dialog.</param>
    public record struct QualifiedDialogNodeID(PrototypeId<DialogPrototype> DialogID, string NodeID)
    {
        public static QualifiedDialogNodeID Empty => new(new(""), "");
    }

    /// <summary>
    /// A complete dialog node that is qualified with the ID of the dialog that contains it.
    /// </summary>
    /// <param name="DialogID">Containing dialog.</param>
    /// <param name="NodeID">Full node in the dialog. If <c>null</c>, this is a dynamically created node.</param>
    /// <param name="Node">The dialog node itself.</param>
    public sealed record QualifiedDialogNode(PrototypeId<DialogPrototype> DialogID, string? NodeID, IDialogNode Node)
    {
        public QualifiedDialogNode(PrototypeId<DialogPrototype> dialogID, IDialogNode node) : this(dialogID, null, node)
        {
        }
    }

    public sealed class DialogEngine : IDialogEngine
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;

        /// <inheritdoc/>
        public DialogPrototype Dialog { get; private set; } = default!;

        /// <inheritdoc/>
        public EntityUid Player { get; private set; }

        /// <inheritdoc/>
        public EntityUid? Speaker { get; set; }

        /// <inheritdoc/>
        public IDialogLayer DialogLayer { get; }

        /// <inheritdoc/>
        public Blackboard<IDialogExtraData> Data { get; }

        private bool _dialogActive = false;

        public DialogEngine(EntityUid player, EntityUid? target, IDialogLayer dialogLayer, Blackboard<IDialogExtraData>? extraData = null)
        {
            EntitySystem.InjectDependencies(this);

            Player = player;
            Speaker = target;
            DialogLayer = dialogLayer;
            Data = extraData ?? new();
        }

        /// <inheritdoc/>
        public QualifiedDialogNode GetNodeByID(QualifiedDialogNodeID nodeID)
        {
            if (!_protos.TryIndex(nodeID.DialogID, out var dialog))
                throw new InvalidDataException($"Dialog with ID {nodeID.DialogID} not found.");

            if (!dialog.Nodes.TryGetValue(nodeID.NodeID, out var node))
                throw new InvalidDataException($"Dialog node {nodeID.NodeID} not found in dialog {dialog.ID}.");

            return new(dialog.GetStrongID(), nodeID.NodeID, node);
        }

        /// <inheritdoc/>
        public QualifiedDialogNode GetNodeByID(PrototypeId<DialogPrototype> protoID, string nodeID)
        {
            var dialog = _protos.Index(protoID);

            if (!dialog.Nodes.TryGetValue(nodeID, out var node))
                throw new InvalidDataException($"Dialog node {nodeID} not found in dialog {dialog.ID}.");

            return new(protoID, nodeID, node);
        }

        /// <inheritdoc/>
        public TurnResult StartDialog(QualifiedDialogNodeID nodeID)
        {
            if (_dialogActive)
            {
                Logger.ErrorS("dialog", $"Dialog is already active! {Dialog.ID}");
                return TurnResult.Succeeded;
            }

            try
            {
                _dialogActive = true;

                Dialog = _protos.Index(nodeID.DialogID);
                QualifiedDialogNode? next = GetNodeByID(nodeID);
                QualifiedDialogNode? last = null;

                while (next != null)
                {
                    last = next;
                    Dialog = _protos.Index(next.DialogID);
                    next = StepDialog(next.Node);
                }

                var ev = new AfterDialogEndedEvent(last);
                if (_entityManager.IsAlive(Speaker))
                    _entityManager.EventBus.RaiseEvent(Speaker.Value, ev);
                else
                    _entityManager.EventBus.RaiseEvent(ev);
            }
            catch (Exception ex)
            {
                Logger.ErrorS("dialog", ex, "Exception during dialog");
            }
            finally
            {
                _dialogActive = false;
            }

            return TurnResult.Succeeded;
        }

        private QualifiedDialogNode? StepDialog(IDialogNode? node)
        {
            var evStepDialog = new BeforeStepDialogEvent(this, node);

            // If there is a speaker, raise the "step dialog" event on them as well as broadcast it.
            // If there is no speaker, then just broadcast it.
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

    /// <summary>
    /// Raised before a dialog node is entered. This can be used to programmatically change the
    /// current node based on a condition. 
    /// </summary>
    public sealed class BeforeStepDialogEvent : EntityEventArgs
    {
        /// <summary>
        /// Current dialog engine.
        /// </summary>
        public IDialogEngine DialogEngine { get; }

        /// <summary>
        /// The next node to enter into. If null, then the dialog will end instead.
        /// </summary>
        public IDialogNode? OutCurrentNode { get; set; }

        public BeforeStepDialogEvent(IDialogEngine engine, IDialogNode? node)
        {
            DialogEngine = engine;
            OutCurrentNode = node;
        }
    }

    public sealed class AfterDialogEndedEvent : EntityEventArgs
    {
        public AfterDialogEndedEvent(QualifiedDialogNode? last)
        {
            NodeEndedOn = last;
        }

        /// <summary>
        /// Node that the dialog ended on.
        /// </summary>
        public QualifiedDialogNode? NodeEndedOn { get; }
    }
}
