using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Dialog
{
    public interface IDialogMessage
    {
        public void Apply(DialogLogic logic);
    }

    public sealed class DialogContinueMessage : IDialogMessage
    {
        public void Apply(DialogLogic logic)
        {
            logic.Next();
        }
    }

    public sealed class DialogCompleteMessage : IDialogMessage
    {
        public void Apply(DialogLogic logic)
        {
            logic.CloseDialog();
        }
    }

    public sealed class DialogCancelMessage : IDialogMessage
    {
        private string Reason;

        public DialogCancelMessage(string reason)
        {
            Reason = reason;
        }

        public void Apply(DialogLogic logic)
        {
            Logger.WarningS("dialog", Reason);
            logic.CloseDialog();
        }
    }

    public sealed class DialogSpeakerSwapMessage : IDialogMessage
    {
        private EntityUid Speaker;

        public DialogSpeakerSwapMessage(EntityUid speaker)
        {
            Speaker = speaker;
        }

        public void Apply(DialogLogic logic)
        {
            logic.ChangeSpeaker(Speaker);
            logic.Next();
        }
    }

    public sealed class DialogResetMessage : IDialogMessage
    {
        public void Apply(DialogLogic logic)
        {
            logic.ResetDialog(initalizeLayer: false);
        }
    }

    public sealed class DialogSingleMessage : IDialogMessage
    {
        private string Message;

        public DialogSingleMessage(string message)
        {
            Message = message;
        }

        public void Apply(DialogLogic logic)
        {
            logic.ShowMessage(Message);
        }
    }

    public sealed class DialogTextMessage : IDialogMessage
    {
        private string Message;

        public DialogTextMessage(string message)
        {
            Message = message;
        }

        public void Apply(DialogLogic logic)
        {
            logic.ChangeChoices(logic.GetMoreChoices());
            logic.ShowMessage(Message);
        }
    }

    public sealed class DialogChoicesMessage : IDialogMessage
    {
        private string Message;
        private IOrderedEnumerable<DialogChoice> Choices;

        public DialogChoicesMessage(string message, IOrderedEnumerable<DialogChoice> choices)
        {
            Message = message;
            Choices = choices;
        }

        public void Apply(DialogLogic logic)
        {
            logic.ChangeChoices(Choices);
        }
    }
}
