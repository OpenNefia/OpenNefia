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
    public class DialogContextData
    {
        private Dictionary<string, object> Data { get; } = new();

        public T Ensure<T>(string key) 
            where T : new()
        {
            T res = default!;
            if (!Data.TryGetValue(key, out var val))
            {
                res = new T();
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

    public interface IDialogLogic
    {
        void Initialize(EntityUid entity, IDialogNode? nodeOverride = null, DialogContextData? contextData = null);
    }

    public class DialogLogic : IDialogLogic
    {
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IPrototypeManager _protoMan = default!;
        [Dependency] private readonly IDialogSystem _dialog = default!;
        [Dependency] private readonly IEntityManager _entMan = default!;

        public EntityUid Speaker = default!;
        private EntityUid DialogEntity = default!;
        private IDialogNode? NodeOverride = null;
        private IEnumerator<IDialogMessage>? MessageEnumerator = default!;
        private IDialogBranchNode? BranchNode;

        public DialogLayer Layer { get; set; } = default!;
        public DialogContextData ContextData = default!;

        public delegate void ShowChoicesDelegate(IOrderedEnumerable<DialogChoice> choices);
        public event ShowChoicesDelegate? OnChoicesChanged;

        public delegate void ShowMessageResultDelegate(string message);
        public event ShowMessageResultDelegate? OnShowMessage;

        public delegate void DialogCloseDelegate();
        public event DialogCloseDelegate? OnDialogClose;

        public delegate void SpeakerChangedDelegate(EntityUid speaker);
        public event SpeakerChangedDelegate? OnSpeakerChanged;

        public void Initialize(EntityUid entity, IDialogNode? nodeOverride = null, DialogContextData? contextData = null)
        {
            EntitySystem.InjectDependencies(this);
            ContextData = contextData ?? new DialogContextData();
            NodeOverride = nodeOverride;
            DialogEntity = entity;
            var args = ResetDialog(initalizeLayer: true);
            EntitySystem.InjectDependencies(args.Layer);
            ChangeSpeaker(entity);
            _uiManager.Query(args.Layer);
        }

        public GetDialogOptionsEventArgs ResetDialog(bool initalizeLayer)
        {
            MessageEnumerator = null;
            var args = _dialog.HandleTalk(DialogEntity);
            Speaker = DialogEntity;
            if (initalizeLayer)
            {
                Layer = args.Layer;
                var layerArgs = new DialogLayer.Args(this);
                Layer.Initialize(layerArgs);
            }
            OnChoicesChanged?.Invoke(args.Choices.OrderByDescending(x => x.Priority));
            return args;
        }

        public void SelectChoice(DialogChoice choice)
        {
            if (BranchNode != null)
            {
                BranchNode.SelectedChoice = choice;
                BranchNode = null;
            }

            if (MessageEnumerator != null)
            {
                if (Next(cancelAtEnd: true))
                    return;
            }

            if ((choice.Node != null && MessageEnumerator == null))
            {
                MessageEnumerator = GetChoiceMessages(choice).GetEnumerator();
                Next();
            }
        }

        public IEnumerable<IDialogMessage> GetComponentMessages()
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
                yield return new DialogCancelMessage("Dialog entity does not have a DialogComponent");
                yield break;
            }

            if (!_protoMan.TryIndex(component.DialogID, out var diaProto))
            {
                yield return new DialogCancelMessage("Dialog doesn not have a prototype");
                yield break;
            }

            var node = diaProto.Node ?? new DefaultDialog();

            foreach(var mes in node.GetResults(DialogEntity, ContextData))
            {
                yield return mes;
            }
        }

        public IEnumerable<IDialogMessage> GetChoiceMessages(DialogChoice choice)
        {
            if (choice.Node == null)
            {
                yield return new DialogCancelMessage("Dialog choice node was null");
                yield break;
            }

            foreach (var res in choice.Node.GetResults(DialogEntity, ContextData))
            {
                yield return res;
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

            MessageEnumerator.Current?.Apply(this);
            return true;
        }

        public void ChangeSpeaker(EntityUid newSpeaker)
        {
            OnSpeakerChanged?.Invoke(newSpeaker);
        }

        public void CloseDialog()
        {
            OnDialogClose?.Invoke();
        }

        public void ShowMessage(string message)
        {
            OnShowMessage?.Invoke(message);
        }

        public void ChangeChoices(IOrderedEnumerable<DialogChoice> choices)
        {
            OnChoicesChanged?.Invoke(choices);
        }

        public IOrderedEnumerable<DialogChoice> GetMoreChoices()
        {
            return new[] { new DialogChoice(id: Protos.Dialog.TalkMore) }
                .OrderByDescending(x => x.Priority);
        }
    }
}
