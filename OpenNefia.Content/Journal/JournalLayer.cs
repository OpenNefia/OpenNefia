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

namespace OpenNefia.Content.Journal
{
    public sealed class JournalLayer : UiLayerWithResult<JournalLayer.Args, UINone>
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

        [Child] private AssetDrawable _assetBook { get; } = default!;
        
        public const int MaxPageLines = 20;
        private float MaxPageWidth => Width / 2;

        private UiPageModel<ElonaMarkupLine> _pages = new(MaxPageLines * 2, wrap: false);

        public JournalLayer()
        {
            _assetBook = new AssetDrawable(Protos.Asset.Book);

            CanControlFocus = true;
            OnKeyBindDown += HandleKeyBindDown;
            EventFilter = UIEventFilterMode.Stop;
        }

        private static int PreviousPageNumber = 0;

        public override void Initialize(Args args)
        {
            _pages.SetElements(args.ElonaMarkup.Lines);
            _pages.SetPage(PreviousPageNumber);
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
            else if (args.Function == EngineKeyFunctions.UIRight)
            {
                if (_pages.PageForward())
                    _audio.Play(Protos.Sound.Card1);
                PreviousPageNumber = _pages.CurrentPage;
                args.Handle();
            }
            else if (args.Function == EngineKeyFunctions.UILeft)
            {
                if (_pages.PageBackward())
                    _audio.Play(Protos.Sound.Card1);
                PreviousPageNumber = _pages.CurrentPage;
                args.Handle();
            }
        }

        public override void OnQuery()
        {
            _audio.Play(Protos.Sound.Book1);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            _assetBook.SetPreferredSize();
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            _assetBook.SetPosition(X, Y);
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            UiUtils.GetCenteredParams(_assetBook.Width, _assetBook.Height, out bounds);
        }

        public override void Update(float dt)
        {
            _assetBook.Update(dt);
        }

        public override void Draw()
        {
            _assetBook.Draw();

            for (var i = 0; i < _pages.CurrentElements.Count; i++)
            {
                var line = _pages.CurrentElements[i];

                var x = X + 80 + (i / MaxPageLines) * 306;
                var y = Y + 45 + (i % MaxPageLines) * 16;

                if (!string.IsNullOrEmpty(line.Text))
                {
                    GraphicsEx.SetFont(line.Font);
                    GraphicsS.PrintS(UIScale, line.Text, x, y);
                }

                if (i % MaxPageLines == 0)
                {
                    var page = (i / MaxPageLines) + 1 + _pages.CurrentPage * 2;
                    GraphicsEx.SetFont(UiFonts.JournalPageNumber);
                    GraphicsS.PrintS(UIScale, $"- {page} -", x + 90, y + 330);

                    if (i % (MaxPageLines * 2) == 0 && _pages.CurrentPage < _pages.PageCount - 1)
                    {
                        GraphicsS.PrintS(UIScale, "(More)", x + 506, y + 330);
                    }
                }
            }
        }

        public override void Dispose()
        {
        }
    }
}