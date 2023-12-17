using OpenNefia.Content.Equipment;
using OpenNefia.Core.Rendering;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using Love;
using OpenNefia.Core.UserInterface;
using System.Collections.Generic;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Maths;
using OpenNefia.Content.UI;
using OpenNefia.Content.CharaInfo;
using Melanchall.DryWetMidi.MusicTheory;
using OpenNefia.Content.Markup;

namespace OpenNefia.Content.Spells
{
    public sealed class SpellsLayer : UiLayerWithResult<SpellsLayer.Args, SpellsLayer.Result>
    {
        [Dependency] private readonly IAudioManager _audio = default!;

        public class Args
        {
            public Args(ElonaMarkup elonaMarkup)
            {
                ElonaMarkup = elonaMarkup;
            }

            public ElonaMarkup ElonaMarkup { get; }
        }

        public class Result
        {
        }

        public const int MaxPageLines = 20;
        private float MaxPageWidth => Width / 2;

        public SpellsLayer()
        {
            CanControlFocus = true;
            OnKeyBindDown += HandleKeyBindDown;
            EventFilter = UIEventFilterMode.Stop;
        }

        private static int PreviousListIndex = 0;

        public override void Initialize(Args args)
        {
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel || args.Function == EngineKeyFunctions.UISelect)
            {
                Cancel();
                args.Handle();
            }
        }

        public override void OnQuery()
        {
            _audio.Play(Protos.Sound.Spell);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            UiUtils.GetCenteredParams(730, 438, out bounds);
        }

        public override void Update(float dt)
        {
        }

        public override void Draw()
        {
        }

        public override void Dispose()
        {
        }
    }
}