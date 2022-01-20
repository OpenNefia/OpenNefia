using OpenNefia.Content.GameObjects.EntitySystems;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
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
        public record Continue() : DialogMessage;
        public record Complete() : DialogMessage;
        public record Cancelled() : DialogMessage;

        public record DialogText(string Text) : DialogMessage;
        public record DialogChoices : DialogText
        {
            public IOrderedEnumerable<DialogChoice> Choices { get; }
            public DialogChoices(IOrderedEnumerable<DialogChoice> choices, string text) : base(text)
            {
                Choices = choices;
            }
        }
    }

    public interface IDialogModel
    {
        void Inititialize(EntityUid entity);
        DialogMessage SelectChoice(DialogChoice choice);
    }

    public class DialogModel : IDialogModel
    {
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
        [Dependency] private readonly IPrototypeManager _protoMan = default!;
        [Dependency] private readonly IDialogSystem _dialog = default!;

        private EntityUid Entity = default!;

        public delegate void ShowChoicesDelegate(IOrderedEnumerable<DialogChoice> choices);
        public event ShowChoicesDelegate OnChoicesChanged = default!;

        public delegate void ShowMessageResultDelegate(DialogMessage.DialogText message);
        public event ShowMessageResultDelegate OnShowMessage = default!;

        public DialogModel()
        {
            EntitySystem.InjectDependencies(this);
        }

        public void Inititialize(EntityUid entity)
        {
            Entity = entity;
            var args = _dialog.HandleTalk(Entity);

            var layerArgs = new DialogLayer.Args(this);
            _uiManager.Query<DialogLayer.Result, DialogLayer, DialogLayer.Args>(args.Layer, layerArgs);

            OnChoicesChanged?.Invoke(args.Choices.OrderByDescending(x => x.Priority));
        }

        public DialogMessage SelectChoice(DialogChoice choice)
        {
            if (!_protoMan.TryIndex(choice.Id, out var node))
                return new DialogMessage.Cancelled();

            var res = node.Node.GetResult(Entity);
            switch (res)
            {
                case DialogMessage.DialogChoices dialogChoices:
                    OnChoicesChanged?.Invoke(dialogChoices.Choices);
                    OnShowMessage?.Invoke(dialogChoices);
                    break;
                case DialogMessage.DialogText dialogText:
                    OnShowMessage?.Invoke(dialogText);
                    break;
            }
            return new DialogMessage.Continue();
        }
    }
}
