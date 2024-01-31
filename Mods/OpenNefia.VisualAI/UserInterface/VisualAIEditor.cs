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

        public bool Enabled => _chara?.Enabled ?? true;

        public VisualAIEditor()
        {
            Window.Title = Loc.GetString("VisualAI.UI.Editor.Title");

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
            Grid.Plan = _plan;
            Trail.Plan = _plan;
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel)
            {
                Cancel();
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

            hints.AddRange(Grid.MakeKeyHints());
            hints.Add(new(UiKeyHints.Close, EngineKeyFunctions.UICancel));

            return hints;
        }

        public void Refresh()
        {
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
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            Window.SetPosition(X, Y);
            Grid.SetPosition(X + 40, Y + 40);
            Trail.SetPosition(X + Grid.Width, Y + 20);
        }

        public override void Draw()
        {
            Window.Draw();
            Grid.Draw();
            Trail.Draw();
        }

        public override void Update(float dt)
        {
            Window.Update(dt);
            Grid.Update(dt);
            Trail.Update(dt);
        }
    }
}
