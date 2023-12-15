using OpenNefia.Content.CharaInfo;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Input;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.IoC;

namespace OpenNefia.Content.Journal
{
    /// <summary>
    /// Wraps the <see cref="JournalLayer"/> in a UI layer that's groupable.
    /// </summary>
    public class JournalUiLayer : LogGroupUiLayer
    {
        [Dependency] private readonly IJournalSystem _journal = default!;

        [Child] private JournalLayer _inner = new();
        
        public JournalUiLayer()
        {
            _inner.EventFilter = UIEventFilterMode.Pass;
        }

        public override void Initialize(LogGroupSublayerArgs args)
        {
            var markup = _journal.GetJournalMarkup();
            var innerArgs = new JournalLayer.Args(markup);
            UserInterfaceManager.InitializeLayer<JournalLayer, JournalLayer.Args, UINone>(_inner, innerArgs);
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            _inner.GrabFocus();
        }

        public override void OnQuery()
        {
            _inner.OnQuery();
        }

        public override void GetPreferredBounds(out UIBox2 bounds)
        {
            _inner.GetPreferredBounds(out bounds);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            _inner.SetSize(Width, Height);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            _inner.SetPosition(X, Y);
        }

        public override UiResult<UINone>? GetResult()
        {
            return _inner.GetResult();
        }

        public override void Update(float dt)
        {
            _inner.Update(dt);
        }

        public override void Draw()
        {
            _inner.Draw();
        }

        public override void Dispose()
        {
            _inner.Dispose();
        }
    }
}