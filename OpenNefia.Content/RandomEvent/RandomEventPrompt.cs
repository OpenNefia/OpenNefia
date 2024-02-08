using OpenNefia.Content.Logic;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.List;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Input;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using Microsoft.CodeAnalysis.Text;
using OpenNefia.Core.Configuration;
using System;

namespace OpenNefia.Content.UI.Layer
{
    public class RandomEventPrompt : UiLayerWithResult<RandomEventPrompt.Args, RandomEventPrompt.Result>
    {
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;

        public class Args
        {
            public string Title = string.Empty;
            public string Text = string.Empty;
            public PrototypeId<AssetPrototype> Image = Protos.Asset.BgRe1;
            public IEnumerable<Choice> Choices = Enumerable.Empty<Choice>();
            public bool IsCancellable = false;
            public int DefaultChoiceIndex = -1;

            public Args()
            {
            }

            public Args(IEnumerable<Choice> choices)
            {
                Choices = choices;
            }

            public Args(IEnumerable<string> choices)
                : this(choices.Select((s, i) => new Choice(i, s)))
            {
            }
        }

        public class Result
        {
            public Choice? Choice { get; }

            public Result(Choice? choice)
            {
                Choice = choice;
            }
        }

        public class Choice : IUiListItem
        {
            public int ChoiceIndex;
            public string ChoiceText;
            public Keyboard.Key Key;

            public Choice(int choiceIndex, string text, Keyboard.Key key = Keyboard.Key.Unknown)
            {
                ChoiceIndex = choiceIndex;
                ChoiceText = text;
                Key = key;
            }

            public string GetChoiceText(int index)
            {
                return ChoiceText;
            }

            public UiListChoiceKey? GetChoiceKey(int index)
            {
                if (Key != Keyboard.Key.Unknown)
                    return new UiListChoiceKey(Key, useKeybind: false);

                return UiListChoiceKey.MakeDefault(index);
            }
        }

        [Child] protected UiList<Choice> List { get; } = new();
        [Child] protected UiWindowBacking Window { get; } = new UiWindowBacking(Protos.Asset.Window);
        [Child] protected UiWindowBacking WindowShadow { get; } = new UiWindowBacking(Protos.Asset.Window, UiWindowBacking.WindowBackingType.Shadow);
        [Child] protected UiTextOutlined TextTitle { get; } = new(UiFonts.RandomEventPromptTitle);
        [Child] protected UiWrappedText TextBody { get; } = new(UiFonts.RandomEventPromptBody);
        [Child] protected AssetDrawable AssetImage { get; private set; } = default!;

        public bool IsCancellable { get; private set; }
        public int DefaultChoiceIndex { get; private set; }

        public RandomEventPrompt()
        {
            OnKeyBindDown += HandleKeyBindDown;

            List.OnActivated += (o, e) =>
            {
                Sounds.Play(Protos.Sound.Click1);
                Finish(new(e.SelectedCell.Data));
            };

            EventFilter = UIEventFilterMode.Pass;
        }

        public override void Initialize(Args args)
        {
            List.CreateAndSetCells(args.Choices);
            IsCancellable = args.IsCancellable || List.Count <= 1;
            TextTitle.Text = Loc.GetString("Elona.RandomEvent.Title", ("title", args.Title));
            TextBody.OriginalText = args.Text;
            AssetImage = new AssetDrawable(args.Image);
            DefaultChoiceIndex = args.DefaultChoiceIndex;
            if (DefaultChoiceIndex == -1 && List.Count <= 1)
                DefaultChoiceIndex = 0;
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            List.GrabFocus();
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel)
            {
                if (IsCancellable)
                {
                    Sounds.Play(Protos.Sound.Click1);
                    Finish(new(List.ElementAtOrDefault(DefaultChoiceIndex)?.Data));
                    args.Handle();
                }
            }
        }

        public override void OnQuery()
        {
            Sounds.Play(Protos.Sound.Pop4);

            if (_config.GetCVar(CCVars.DebugSkipRandomEvents) && IsCancellable)
            {
                var index = 0;
                var result = List.ElementAtOrDefault(index);
                _mes.Display(TextBody.OriginalText);
                _mes.Display(Loc.GetString("Elona.RandomEvent.Skip", ("text", result?.Data.ChoiceText ?? "---")));
                Finish(new(result?.Data));
            }
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            List.GetPreferredSize(out var listSize);
            AssetImage.GetPreferredSize(out var imageSize);
            var width = imageSize.X + 36;
            TextBody.WrapAt((int)(width * UIScale) - 80);
            var height = imageSize.Y + TextBody.UiText.Height + 90 + listSize.Y;

            UiUtils.GetCenteredParams(width, height - 16, out bounds, yOffset: 16);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            Window.SetSize(Width, Height);
            WindowShadow.SetSize(Width, Height - Height % 8);
            TextTitle.SetPreferredSize();
            TextBody.SetSize(Width - 80, Height);
            AssetImage.SetPreferredSize();
            List.SetPreferredSize();
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            Window.SetPosition(X, Y);
            WindowShadow.SetPosition(X + 4, Y + 4);
            TextTitle.SetPosition(X + 40, Y + 16 + 16);
            TextBody.SetPosition(X + 24, Y + AssetImage.Height + 20 + 16);
            AssetImage.SetPosition(X + 12, Y + 6 + 16);
            List.SetPosition(X + 38, Y + 30 + TextBody.UiText.Height + AssetImage.Height + 16);
        }

        public override void Update(float dt)
        {
            Window.Update(dt);
            WindowShadow.Update(dt);
            TextTitle.Update(dt);
            TextBody.Update(dt);
            AssetImage.Update(dt);
            List.Update(dt);
        }

        public override void Draw()
        {
            Love.Graphics.SetColor(new Color(255, 255, 255, 80));
            Love.Graphics.SetBlendMode(Love.BlendMode.Subtract);
            WindowShadow.Draw();
            Love.Graphics.SetBlendMode(Love.BlendMode.Alpha);

            Love.Graphics.SetColor(Color.White);
            Window.Draw();

            AssetImage.Draw();

            Love.Graphics.SetColor(new Color(240, 230, 220));
            GraphicsS.RectangleS(UIScale, Love.DrawMode.Line, AssetImage.X, AssetImage.Y, AssetImage.Width, AssetImage.Height);

            TextTitle.Draw();
            TextBody.Draw();
            List.Draw();
        }

        public override void Dispose()
        {
            Window.Dispose();
            WindowShadow.Dispose();
            TextTitle.Dispose();
            TextBody.Dispose();
            AssetImage.Dispose();
            List.Dispose();
        }
    }
}