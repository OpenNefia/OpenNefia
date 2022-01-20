using OpenNefia.Content.GameObjects.EntitySystems;
using OpenNefia.Core.UI.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Dialog
{
    public interface IDialogLayer : IUiLayerWithResult<DialogLayer.Args, DialogLayer.Result>
    {

    }

    public class DialogLayer : UiLayerWithResult<DialogLayer.Args, DialogLayer.Result>, IDialogLayer
    {
        private DialogModel Model = default!;

        public class Args
        {
            public DialogModel Model;

            public Args(DialogModel model)
            {
                Model = model;
            }
        }

        public class Result
        {

        }

        public override void Initialize(Args args)
        {
            base.Initialize(args);
            Model = args.Model;
            Model.OnChoicesChanged += OnChoicesChanged;
            Model.OnShowMessage += OnShowMessage;

            //_mes.Display($"Dialog: {string.Join(", ", args.Choices.Select(x => x.Id))}");
        }

        protected virtual void OnShowMessage(DialogMessage.DialogText message)
        {
        }

        protected virtual void OnChoicesChanged(IOrderedEnumerable<DialogChoice> choices)
        {
        }

        public override void Dispose()
        {
            base.Dispose();
            Model.OnChoicesChanged -= OnChoicesChanged;
            Model.OnShowMessage -= OnShowMessage;
        }
    }

    public class DefaultDialogLayer : DialogLayer
    {

    }
}
