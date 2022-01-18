using OpenNefia.Core.Graphics;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UserInterface;

namespace OpenNefia.Core.UI.Layer
{
    public class UiLayer : UiElement, IUiLayer
    {
        private readonly IInputManager _inputManager = default!;

        public virtual int? DefaultZOrder => null;
        public int ZOrder { get; set; }

        public override UiLayer? Root { get; internal set; }

        public UiLayer()
        {
            _inputManager = IoCManager.Resolve<IInputManager>();

            Root = this;
            EventFilter = UIEventFilterMode.Stop;
        }

        public virtual void GetPreferredBounds(out UIBox2 bounds)
        {
            var graphics = IoCManager.Resolve<IGraphics>();
            bounds = UIBox2.FromDimensions(Vector2.Zero, graphics.WindowSize);
        }

        public override sealed void GetPreferredSize(out Vector2 size)
        {
            GetPreferredBounds(out var bounds);
            size = bounds.Size;
        }

        public void GetPreferredPosition(out Vector2 pos)
        {
            GetPreferredBounds(out var bounds);
            pos = bounds.TopLeft;
        }

        public void SetPreferredPosition()
        {
            GetPreferredPosition(out var pos);
            SetPosition(pos.X, pos.Y);
        }

        public bool IsInActiveLayerList()
        {
            return UserInterfaceManager.IsInActiveLayerList(this);
        }

        public bool IsQuerying()
        {
            return UserInterfaceManager.IsQuerying(this);
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            _inputManager.Contexts.SetActiveContext(InputContextContainer.DefaultContextName);
        }

        public virtual void OnQuery()
        {
        }

        public virtual void OnQueryFinish()
        {
        }

        protected internal override void KeyBindDown(GUIBoundKeyEventArgs args)
        {
            base.KeyBindDown(args);

            if (args.Handled)
                return;

            _inputManager.ViewportKeyEvent(this, args);
        }

        protected internal override void KeyBindUp(GUIBoundKeyEventArgs args)
        {
            base.KeyBindUp(args);

            if (args.Handled)
                return;

            _inputManager.ViewportKeyEvent(this, args);
        }

        public void Localize()
        {
            Localize(GetType().GetBaseLocaleKey());
        }
    }
}
