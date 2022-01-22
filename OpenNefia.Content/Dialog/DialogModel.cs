using OpenNefia.Content.GameObjects;
using OpenNefia.Content.GameObjects.EntitySystems;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.UserInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Dialog
{
    public record DialogMessage
    {
        public record Continue()    : DialogMessage;
        public record Complete()    : DialogMessage;
        public record Cancelled()   : DialogMessage;
        public record Return()      : DialogMessage;

        public record DialogText(string Text) : DialogMessage;
        public record DialogSingle(string Text) : DialogText(Text);
        public record DialogChoices(string Text, IOrderedEnumerable<DialogChoice> Choices, IDialogBranchNode Node) : DialogText(Text);

        public record SpeakerSwap(string id) : DialogMessage;
    }

    public interface IDialogModel
    {
        void Inititialize(EntityUid entity);
    }

    public class DialogModel : IDialogModel
    {
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IPrototypeManager _protoMan = default!;
        [Dependency] private readonly IDialogSystem _dialog = default!;
        [Dependency] private readonly IEntityManager _entMan = default!;

        public EntityUid Speaker = default!;
        private EntityUid DialogEntity = default!;
        private IEnumerator<DialogMessage>? MessageEnumerator = default!;
        private IDialogBranchNode? BranchNode;

        public delegate void ShowChoicesDelegate(IOrderedEnumerable<DialogChoice> choices);
        public event ShowChoicesDelegate OnChoicesChanged = default!;

        public delegate void ShowMessageResultDelegate(DialogMessage.DialogText message);
        public event ShowMessageResultDelegate OnShowMessage = default!;

        public delegate void DialogCloseDelegate();
        public event DialogCloseDelegate OnDialogClose = default!;

        public delegate void SpeakerChangedDelegate(EntityUid speaker);
        public event SpeakerChangedDelegate OnSpeakerChanged = default!;

        public DialogModel()
        {
            EntitySystem.InjectDependencies(this);
        }

        public void Inititialize(EntityUid entity)
        {
            DialogEntity = entity;
            var args = ResetDialog(initalizeLayer: true);
            Next();
            _uiManager.Query(args.Layer);
        }

        private GetDialogueOptionsEventArgs ResetDialog(bool initalizeLayer)
        {
            var args = _dialog.HandleTalk(DialogEntity);
            Speaker = DialogEntity;
            if (initalizeLayer)
            {
                var layerArgs = new DialogLayer.Args(this);
                args.Layer.Initialize(layerArgs);
            }
            OnChoicesChanged?.Invoke(args.Choices.OrderByDescending(x => x.Priority));
            return args;
        }

        public void SelectChoice(DialogChoice choice)
        {
            if (choice.Flags.HasFlag(DialogChoiceFlag.Branch) && BranchNode != null)
            {
                BranchNode.SelectedChoice = choice;
            }

            if (MessageEnumerator != null)
            {
                if (Next(cancelAtEnd: true))
                    return;
                else
                    ResetDialog(false);
            }

            if (choice.Node != null && MessageEnumerator == null)
            {
                MessageEnumerator = GetChoiceMessages(choice).GetEnumerator();
                Next();
            }
        }

        public IEnumerable<DialogMessage> GetComponentMessages()
        {
            if (!_entMan.TryGetComponent(DialogEntity, out DialogComponent component))
            {
                yield return new DialogMessage.Cancelled();
                yield break;
            }

            if (!component.DialogID.HasValue
                || !_protoMan.TryIndex(component.DialogID.Value, out var diaProto))
            {
                yield return new DialogMessage.Cancelled();
                yield break;
            }

            var node = diaProto.Node ?? new DefaultDialog();

            foreach(var mes in node.GetResults(DialogEntity))
            {
                yield return mes;
            }
        }

        public IEnumerable<DialogMessage> GetChoiceMessages(DialogChoice choice)
        {
            if (choice.Node == null)
            {
                yield return new DialogMessage.Cancelled();
                yield break;
            }

            foreach (var res in choice.Node.GetResults(DialogEntity))
            {
                yield return res;
            }
        }

        public void HandleMessage(DialogMessage message)
        {
            switch (message)
            {
                case DialogMessage.Return:
                    ResetDialog(initalizeLayer: false);
                    break;
                case DialogMessage.Complete:
                case DialogMessage.Cancelled:
                    OnDialogClose?.Invoke();
                    break;
                case DialogMessage.DialogSingle single:
                    OnShowMessage?.Invoke(single);
                    break;
                case DialogMessage.DialogChoices dialogChoices:
                    BranchNode = dialogChoices.Node;
                    OnChoicesChanged?.Invoke(dialogChoices.Choices);
                    OnShowMessage?.Invoke(dialogChoices);
                    break;
                case DialogMessage.DialogText dialogText:
                    OnChoicesChanged?.Invoke(GetMoreChoice().OrderBy(x => x.Priority));
                    OnShowMessage?.Invoke(dialogText);
                    break;
                case DialogMessage.Continue:
                    break;
            }
        }

        public virtual bool Next(bool cancelAtEnd = false)
        {
            if (MessageEnumerator == null || !MessageEnumerator.MoveNext())
            {
                if (cancelAtEnd)
                {
                    MessageEnumerator = null;
                    return false;
                }
                MessageEnumerator = GetComponentMessages().GetEnumerator();
                MessageEnumerator.MoveNext();
            }

            HandleMessage(MessageEnumerator.Current);
            return true;
        }

        private IEnumerable<DialogChoice> GetMoreChoice()
        {
            yield return new DialogChoice(id: Protos.Dialog.TalkMore, flags: DialogChoiceFlag.More);
        }
    }
}
