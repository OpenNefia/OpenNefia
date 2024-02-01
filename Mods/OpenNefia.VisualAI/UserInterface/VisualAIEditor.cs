using OpenNefia.Content.UI.Element;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Input;
using OpenNefia.Core.Locale;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Layer;
using OpenNefia.VisualAI.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenNefia.Core.IoC;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Audio;
using OpenNefia.Content.UI;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Content.Input;
using OpenNefia.Core;

namespace OpenNefia.VisualAI.UserInterface
{
    public sealed class VisualAIEditor : UiLayerWithResult<VisualAIEditor.Args, UINone>
    {
        [Dependency] private readonly IAudioManager _audio = default!;

        public class Args
        {
            public Args(VisualAIPlan plan, VisualAIComponent? chara = null)
            {
                Plan = plan;
                Chara = chara;
            }

            public VisualAIPlan Plan { get; }
            public VisualAIComponent? Chara { get; }
        }

        private VisualAIPlan _plan = new();
        private VisualAIComponent? _chara;

        [Child] UiWindow Window { get; } = new();
        [Child] VisualAIEditorGrid Grid { get; } = new();
        [Child] VisualAIEditorTrail Trail { get; } = new();
        [Child] UiText TextDisabled { get; } = new UiTextOutlined(new FontSpec(30, 30, color: UiColors.TextWhite, bgColor: UiColors.TextBlack), "(Disabled)");

        public bool Enabled
        {
            get => _chara?.Enabled ?? true;
            set
            {
                if (_chara != null)
                {
                    _chara.Enabled = value;
                    Grid.Enabled = value;
                }
            }
        }

        public VisualAIEditor()
        {
            Window.Title = Loc.GetString("VisualAI.UI.Editor.Window.Title");

            OnKeyBindDown += HandleKeyBindDown;
            Grid.OnRefreshed += Refresh;
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            Grid.GrabFocus();
        }

        public override void Initialize(Args args)
        {
            _plan = args.Plan;
            _chara = args.Chara;
            Grid.RootPlan = _plan;
            Grid.Enabled = Enabled;
            Trail.RootPlan = _plan;

            Grid.RebuildCanvas();
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel)
            {
                Cancel();
                args.Handle();
            }
            else if (args.Function == ContentKeyFunctions.UIMode)
            {
                if (_chara != null)
                {
                    _audio.Play(Protos.Sound.Ok1);
                    Enabled = !Enabled;
                    Window.KeyHints = MakeKeyHints();
                }
                args.Handle();
            }
        }

        public override void OnQuery()
        {
            _audio.Play(Protos.Sound.Pop2);
        }

        public override List<UiKeyHint> MakeKeyHints()
        {
            var hints = base.MakeKeyHints();

            hints.Add(new(new LocaleKey("VisualAI.UI.Editor.KeyHints").With(Enabled ? "Disable" : "Enable"), ContentKeyFunctions.UIMode));
            hints.AddRange(Grid.MakeKeyHints());
            hints.Add(new(UiKeyHints.Close, EngineKeyFunctions.UICancel));

            return hints;
        }

        public void Refresh()
        {
            Window.KeyHints = MakeKeyHints();
            Trail.Refresh(Grid.TrailData);
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            UiUtils.GetCenteredParams(800, 480, out bounds, yOffset: -16);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            Window.SetSize(Width, Height);
            Grid.SetSize(Width - 272, Height - 80);
            Trail.SetSize(320, Height - 50);
            TextDisabled.SetPreferredSize();
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            Window.SetPosition(X, Y);
            Grid.SetPosition(X + 40, Y + 40);
            Trail.SetPosition(X + Grid.Width, Y + 20);
            TextDisabled.SetPosition(X + Grid.Width / 2 - TextDisabled.Width / 4, Y + Grid.Height / 2 - TextDisabled.Height / 4);
        }

        public override void Draw()
        {
            Window.Draw();
            Grid.Draw();
            Trail.Draw();

            if (!Enabled)
            {
                Love.Graphics.SetColor(Color.Black.WithAlphaB(128));
                Love.Graphics.Rectangle(Love.DrawMode.Fill, Grid.GlobalPixelRect);
                Love.Graphics.Rectangle(Love.DrawMode.Fill, Trail.GlobalPixelRect);
                TextDisabled.Draw();
            }
        }

        public override void Update(float dt)
        {
            Window.Update(dt);
            Grid.Update(dt);
            Trail.Update(dt);
            TextDisabled.Update(dt);
        }
    }
}
