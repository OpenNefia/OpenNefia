﻿using OpenNefia.Content.DisplayName;
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
    public interface IDialogLayer : IUiLayerWithResult<DialogLayer.Args, UINone>
    {
    }

    public class DialogLayer : UiLayerWithResult<DialogLayer.Args, UINone>, IDialogLayer
    {
        protected DialogLogic Logic = default!;

        public class Args
        {
            public DialogLogic Model;

            public Args(DialogLogic model)
            {
                Model = model;
            }
        }

        public override void Initialize(Args args)
        {
            base.Initialize(args);
            Logic = args.Model;
            Logic.OnChoicesChanged += OnChoicesChanged;
            Logic.OnShowMessage += OnShowMessage;
            Logic.OnDialogClose += OnDialogClose;
            Logic.OnSpeakerChanged += OnSpeakerChanged;
            OnKeyBindDown += KeyBindDown;
        }

        protected override void KeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel)
                Finish(new());
        }

        protected virtual void OnShowMessage(string message)
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
            Logic.OnChoicesChanged -= OnChoicesChanged;
            Logic.OnShowMessage -= OnShowMessage;
            Logic.OnDialogClose -= OnDialogClose;
            Logic.OnSpeakerChanged -= OnSpeakerChanged;
            OnKeyBindDown -= KeyBindDown;
        }
    }

    public class DefaultDialogLayer : DialogLayer
    {
        private class DialogCell : UiListCell<DialogChoice>
        {
            public DialogCell(DialogChoice data, DialogContextData context) 
                : base(data)
            {
                if (data.FormatData == null)
                    UiText.Text = Loc.GetString(data.LocKey);
                else
                    UiText.Text = Loc.GetString(data.LocKey, data.FormatData.Select((x, ind) => new LocaleArg($"_{ind}", x.GetFormatText(context))).ToArray());
            }
        }

        [Dependency] private readonly IDisplayNameSystem _nameSys = default!;

        private const int DialogWidth = 600;
        private const int DialogHeight = 380;
        private IAssetInstance AssetIeChat;

        [Child] private FaceFrame FaceFrame;
        [Child] private UiList<DialogChoice> List;
        [Child] private UiWrappedText Text;
        [Child] private UiText NameText;
        [Child] private UiTextTopic ImpressTopic;
        [Child] private UiTextTopic AttractTopic;

        public DefaultDialogLayer()
        {
            AssetIeChat = Assets.Get(Protos.Asset.IeChat);
            Text = new UiWrappedText(UiFonts.DialogText);
            List = new UiList<DialogChoice>();
            List.OnActivated += ListOnActivated;
            NameText = new UiText(UiFonts.DialogName);
            FaceFrame = new FaceFrame(renderFrame: false);
            ImpressTopic = new UiTextTopic(Loc.GetString("Elona.Dialog.Topic.Impress"));
            AttractTopic = new UiTextTopic(Loc.GetString("Elona.Dialog.Topic.Attract"));
        }

        private void ListOnActivated(object? sender, UiListEventArgs<DialogChoice> args)
        {
            Logic.SelectChoice(args.SelectedCell.Data);
            Sounds.Play(Protos.Sound.More1);
        }

        protected override void KeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel)
                Sounds.Play(Protos.Sound.More1);
            base.KeyBindDown(args);
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
            Logic.Next();
        }

        protected override void OnChoicesChanged(IOrderedEnumerable<DialogChoice> choices)
        {
            base.OnChoicesChanged(choices);
            List.Clear();
            List.SetAll(choices.Select(x => new DialogCell(x, Logic.ContextData)));
            SetSize(Width, Height);
            SetPosition(X, Y);
        }

        protected override void OnShowMessage(string message)
        {
            base.OnShowMessage(message);
            Text.WrappedText = message;
        }

        protected override void OnDialogClose()
        {
            base.OnDialogClose();
            Finish(new());
        }

        protected override void OnSpeakerChanged(EntityUid speaker)
        {
            base.OnSpeakerChanged(speaker);
            NameText.Text = _nameSys.GetDisplayName(speaker);
            FaceFrame.RefreshFromEntity(speaker);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            List.SetPreferredSize();
            Text.SetSize(360, 200);
            NameText.SetPreferredSize();
            FaceFrame.SetPreferredSize();
            ImpressTopic.SetPreferredSize();
            AttractTopic.SetPreferredSize();
        }

        public override void SetPosition(float x, float y)
        {
            UiUtils.GetCenteredParams(new(DialogWidth, DialogHeight), out var bounds, -20);
            x = bounds.TopLeft.X;
            y = bounds.TopLeft.Y;
            base.SetPosition(x, y);

            List.SetPosition(x + 140, y + DialogHeight - List.Height - 30);
            Text.SetPosition(x + 145, y + 65);
            NameText.SetPosition(x + 110, y + 35);
            FaceFrame.SetPosition(x + 40, y + 55);
            ImpressTopic.SetPosition(x + 30, y + 190);
            AttractTopic.SetPosition(ImpressTopic.X, y + 235);
        }

        public override void Draw()
        {
            GraphicsEx.SetColor(0, 0, 0, 75);
            AssetIeChat.DrawUnscaled(PixelX + 4, PixelY + 4, DialogWidth * UIScale, DialogHeight * UIScale);
            GraphicsEx.SetColor(Color.White);
            AssetIeChat.DrawUnscaled(PixelX, PixelY, DialogWidth * UIScale, DialogHeight * UIScale);
            List.Draw();
            Text.Draw();
            NameText.Draw();
            FaceFrame.Draw();
            ImpressTopic.Draw();
            AttractTopic.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            List.Update(dt);
            Text.Update(dt);
            NameText.Update(dt);
            FaceFrame.Update(dt);
            ImpressTopic.Update(dt);
            AttractTopic.Update(dt);
        }
    }
}
