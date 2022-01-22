using OpenNefia.Content.GameObjects.EntitySystems;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.UI;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
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
        protected DialogModel Model = default!;

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

        public DialogLayer()
        {
            IoCManager.InjectDependencies(this);
        }

        public override void Initialize(Args args)
        {
            base.Initialize(args);
            Model = args.Model;
            Model.OnChoicesChanged += OnChoicesChanged;
            Model.OnShowMessage += OnShowMessage;
            Model.OnDialogClose += OnDialogClose;
            Model.OnSpeakerChanged += OnSpeakerChanged;
            OnKeyBindDown += KeyBindDown;
            //_mes.Display($"Dialog: {string.Join(", ", args.Choices.Select(x => x.Id))}");
        }

        protected override void KeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel)
                Finish(new());
        }

        protected virtual void OnShowMessage(DialogMessage.DialogText message)
        {
        }

        protected virtual void OnChoicesChanged(IOrderedEnumerable<DialogChoice> choices)
        {
        }

        protected virtual void OnDialogClose()
        {
        }

        protected virtual void OnSpeakerChanged(EntityUid speaker)
        {
        }

        public override void Dispose()
        {
            base.Dispose();
            Model.OnChoicesChanged -= OnChoicesChanged;
            Model.OnShowMessage -= OnShowMessage;
            Model.OnDialogClose -= OnDialogClose;
            Model.OnSpeakerChanged -= OnSpeakerChanged;
            OnKeyBindDown -= KeyBindDown;
        }
    }

    public class DefaultDialogLayer : DialogLayer
    {
        private class DialogCell : UiListCell<DialogChoice>
        {
            public DialogCell(DialogChoice data) 
                : base(data)
            {
                UiText.Text = Loc.GetString(data.LocKey + data.ExtraText);
            }
        }

        private const int DialogWidth = 600;
        private const int DialogHeight = 380;
        private IAssetInstance AssetIeChat;

        [Child] private UiList<DialogChoice> List;
        [Child] private UiWrappedText Text;

        public DefaultDialogLayer()
        {
            AssetIeChat = Assets.Get(Protos.Asset.IeChat);
            Text = new UiWrappedText(UiFonts.ListText);
            List = new UiList<DialogChoice>();
            List.OnActivated += ListOnActivated;
        }

        private void ListOnActivated(object? sender, UiListEventArgs<DialogChoice> args)
        {
            Model.SelectChoice(args.SelectedCell.Data);
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            List.GrabFocus();
        }

        public override void OnQuery()
        {
            base.OnQuery();
            Sounds.Play(Protos.Sound.Chat);
        }

        public override void OnQueryFinish()
        {
            base.OnQueryFinish();
            Sounds.Play(Protos.Sound.More1);
        }

        protected override void OnChoicesChanged(IOrderedEnumerable<DialogChoice> choices)
        {
            base.OnChoicesChanged(choices);
            List.Clear();
            List.SetAll(choices.Select(x => new DialogCell(x)));
            SetSize(Width, Height);
            SetPosition(X, Y);
        }

        protected override void OnShowMessage(DialogMessage.DialogText message)
        {
            base.OnShowMessage(message);
            if (!string.IsNullOrEmpty(Text.WrappedText))
                Sounds.Play(Protos.Sound.More1);
            Text.WrappedText = message.Text;
        }

        protected override void OnDialogClose()
        {
            base.OnDialogClose();
            Finish(new());
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            List.SetPreferredSize();
            Text.SetSize(360, 200);
        }

        public override void SetPosition(float x, float y)
        {
            UiUtils.GetCenteredParams(new(DialogWidth, DialogHeight), out var bounds, -20);
            x = bounds.TopLeft.X;
            y = bounds.TopLeft.Y;
            base.SetPosition(x, y);

            List.SetPosition(x + 140, y + DialogHeight - List.Height - 30);
            Text.SetPosition(x + 145, y + 70);
        }

        public override void Draw()
        {
            base.Draw();
            GraphicsEx.SetColor(0, 0, 0, 75);
            AssetIeChat.DrawUnscaled(PixelX + 4, PixelY + 4, DialogWidth * UIScale, DialogHeight * UIScale);
            GraphicsEx.SetColor(Color.White);
            AssetIeChat.DrawUnscaled(PixelX, PixelY, DialogWidth * UIScale, DialogHeight * UIScale);
            List.Draw();
            Text.Draw();
        }
    }
}
