using OpenNefia.Content.UI.Element;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using OpenNefia.VisualAI.Engine;
using OpenNefia.Core.IoC;
using OpenNefia.Core.GameObjects;
using OpenNefia.Content.UI;
using OpenNefia.Core.Input;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Audio;
using OpenNefia.Content.Prototypes;

namespace OpenNefia.VisualAI.UserInterface
{
    public sealed class VisualAIConfigureBlockMenu : UiLayerWithResult<VisualAIConfigureBlockMenu.Args, VisualAIConfigureBlockMenu.Result>
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IAudioManager _audio = default!;

        public class Args
        {
            public Args(VisualAIBlock block)
            {
                Block = block;
            }

            public VisualAIBlock Block { get; }
        }

        public class Result
        {
            public Result(VisualAIBlock block)
            {
                Block = block;
            }

            public VisualAIBlock Block { get; }
        }

        [Child] private UiWindow Window { get; } = new();
        [Child] private VisualAIBlockCard Card { get; } = new();
        [Child] private VisualAIConfigureBlockList List { get; } = new();
        [Child] private UiTextTopic TopicOptions { get; } = new();

        private VisualAIBlock _block = default!;
        private VisualAIVariableSet _variables = new(new());

        public VisualAIConfigureBlockMenu()
        {
            // TODO
            EntitySystem.InjectDependencies(this);

            Window.Title = Loc.GetString("VisualAI.UI.ConfigureBlock.Window.Title");
            Window.KeyHints = MakeKeyHints();
            TopicOptions.Text = Loc.GetString("VisualAI.UI.ConfigureBlock.Topic.Options");

            OnKeyBindDown += HandleKeyBindDown;
            List.OnRefreshConfig += HandleRefreshConfig;
        }

        public override void Initialize(Args args)
        {
            var proto = _protos.Index(args.Block.ProtoID);
            Card.Icon.Color = proto.Color;
            Card.Icon.Icon = proto.Icon;
            _block = args.Block;
            _variables = args.Block.Variables;

            var items = new List<VisualAIConfigureBlockList.Item>();
            foreach (var (category, vars) in _variables.Variables)
            {
                items.AddRange(vars.Select(v => new VisualAIConfigureBlockList.Item(category, v.Key, v.Value)));
            }
            List.SetItems(items);

            UpdateCardText();
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
                Cancel();
                args.Handle();
            }
            else if (args.Function == EngineKeyFunctions.UISelect)
            {
                _audio.Play(Protos.Sound.Ok1);
                Finish(new(_block));
                args.Handle();
            }
        }

        private void HandleRefreshConfig()
        {
            UpdateCardText();
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            var hints = base.MakeKeyHints();
            hints.AddRange(List.MakeKeyHints());
            hints.Add(new(UiKeyHints.Back, EngineKeyFunctions.UICancel));
            return hints;
        }

        private void UpdateCardText()
        {
            Card.Text.OriginalText = VisualAIHelpers.FormatBlockDescription(_block.Proto, _variables);
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            UiUtils.GetCenteredParams(440, 380, out bounds, yOffset: -12);
        }

        public override void SetSize(float width, float height)
        {
            if (List.Count > 8)
                height += 10 + 30 * (List.Count - 9);

            base.SetSize(width, height);
            Window.SetSize(Width, Height);
            Card.SetSize(Width - 80, 80);
            List.SetSize(Width, height - 80);
            TopicOptions.SetPreferredSize();
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            Window.SetPosition(X, Y);
            Card.SetPosition(X + 40, Y + 40);
            List.SetPosition(X + 56, Y + 66 + 80);
            TopicOptions.SetPosition(X + 34, Card.Rect.Bottom + 0);
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            Window.Update(dt);
            Card.Update(dt);
            List.Update(dt);
            TopicOptions.Update(dt);
        }

        public override void Draw()
        {
            Window.Draw();
            Card.Draw();
            List.Draw();
            TopicOptions.Draw();
        }
    }
}
