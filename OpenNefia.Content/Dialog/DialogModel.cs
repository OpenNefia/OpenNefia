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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Dialog
{
    public record DialogMessage
    {
        public record Continue()    : DialogMessage;
        public record Complete()    : DialogMessage;
        public record Return()      : DialogMessage;
        public record Cancelled(string Message) : DialogMessage;

        public record DialogText(string Text) : DialogMessage;
        public record DialogSingle(string Text) : DialogText(Text);
        public record DialogChoices(string Text, IOrderedEnumerable<DialogChoice> Choices, IDialogBranchNode Node) : DialogText(Text);

        public record SpeakerSwap(EntityUid Entity) : DialogMessage;
    }

    public class DialogContextData
    {
        private Dictionary<string, object> Data { get; } = new();

        public T Ensure<T>(string key)
        {
            T res = default!;
            if (!Data.TryGetValue(key, out var val))
            {
                res = Activator.CreateInstance<T>()!;
                Data[key] = res;
            }
            else if (Data[key] is T tVal)
            {
                res = tVal;
            }    
            else
            {
                Logger.WarningS("dialog", $"Context tried to get key {key} as type {typeof(T)}, " +
                    $"but data has a value for type {val?.GetType()}");
            }
            return res;
        }

        public bool TryGet<T>(string key, [NotNullWhen(true)] out T? val)
        {
            if (Data.TryGetValue(key, out var oVal))
            {
                if (oVal is T tVal)
                {
                    val = tVal;
                    return true;
                }
            }
            val = default!;
            return false;
        }

        public void Set(string key, object data)
        {
            Data[key] = data;
        }
    }

    public interface IDialogModel
    {
        void Inititialize(EntityUid entity, IDialogNode? nodeOverride = null, DialogContextData? contextData = null);
    }

    public class DialogModel : IDialogModel
    {
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IPrototypeManager _protoMan = default!;
        [Dependency] private readonly IDialogSystem _dialog = default!;
        [Dependency] private readonly IEntityManager _entMan = default!;

        public EntityUid Speaker = default!;
        private EntityUid DialogEntity = default!;
        private IDialogNode? NodeOverride = null;
        private IEnumerator<DialogMessage>? MessageEnumerator = default!;
        private IDialogBranchNode? BranchNode;

        public DialogContextData ContextData = default!;

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

        public void Inititialize(EntityUid entity, IDialogNode? nodeOverride = null, DialogContextData? contextData = null)
        {
            ContextData = contextData ?? new DialogContextData();
            NodeOverride = nodeOverride;
            DialogEntity = entity;
            var args = ResetDialog(initalizeLayer: true);
            OnSpeakerChanged?.Invoke(entity);
            _uiManager.Query(args.Layer);
        }

        private GetDialogueOptionsEventArgs ResetDialog(bool initalizeLayer)
        {
            MessageEnumerator = null;
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
            }

            if ((choice.Node != null && MessageEnumerator == null)
                || choice.Flags.HasFlag(DialogChoiceFlag.OverrideNode))
            {
                MessageEnumerator = GetChoiceMessages(choice).GetEnumerator();
                Next();
            }
        }

        public IEnumerable<DialogMessage> GetComponentMessages()
        {
            if (NodeOverride != null)
            {
                foreach(var mes in NodeOverride.GetResults(DialogEntity, ContextData))
                {
                    yield return mes;
                }
                yield break;
            }

            if (!_entMan.TryGetComponent(DialogEntity, out DialogComponent component))
            {
                yield return new DialogMessage.Cancelled("Dialog entity does not have a DialogComponent");
                yield break;
            }

            if (!component.DialogID.HasValue
                || !_protoMan.TryIndex(component.DialogID.Value, out var diaProto))
            {
                yield return new DialogMessage.Cancelled("Dialog doesn not have a prototype");
                yield break;
            }

            var node = diaProto.Node ?? new DefaultDialog();

            foreach(var mes in node.GetResults(DialogEntity, ContextData))
            {
                yield return mes;
            }
        }

        public IEnumerable<DialogMessage> GetChoiceMessages(DialogChoice choice)
        {
            if (choice.Node == null)
            {
                yield return new DialogMessage.Cancelled("Dialog choice node was null");
                yield break;
            }

            foreach (var res in choice.Node.GetResults(DialogEntity, ContextData))
            {
                yield return res;
            }
        }

        public void HandleMessage(DialogMessage message)
        {
            switch (message)
            {
                case DialogMessage.Continue:
                    break;
                case DialogMessage.SpeakerSwap swap:
                    OnSpeakerChanged?.Invoke(swap.Entity);
                    Next();
                    break;
                case DialogMessage.Return:
                    ResetDialog(initalizeLayer: false);
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
                case DialogMessage.Complete:
                    OnDialogClose?.Invoke();
                    break;
                case DialogMessage.Cancelled cancel:
                    Logger.WarningS("dialog", cancel.Message);
                    OnDialogClose?.Invoke();
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
