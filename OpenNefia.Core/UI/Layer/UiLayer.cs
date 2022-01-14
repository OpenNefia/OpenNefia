using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Love;
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

        public override UiLayer? Root { get => this; internal set => throw new InvalidOperationException(); }

        public UiLayer()
        {
            _inputManager = IoCManager.Resolve<IInputManager>();

            EventFilter = UIEventFilterMode.Stop;
        }

        public virtual void GetPreferredBounds(out UIBox2i bounds)
        {
            var graphics = IoCManager.Resolve<IGraphics>();
            bounds = UIBox2i.FromDimensions(Vector2i.Zero, graphics.WindowSize);
        }

        public override sealed void GetPreferredSize(out Vector2i size)
        {
            GetPreferredBounds(out var bounds);
            size = bounds.Size;
        }

        public bool IsInActiveLayerList()
        {
            return IoCManager.Resolve<IUserInterfaceManager>().IsInActiveLayerList(this);
        }

        public bool IsQuerying()
        {
            return IoCManager.Resolve<IUserInterfaceManager>().IsQuerying(this);
        }

        public virtual void OnFocused()
        {
            GrabFocus();
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

    public class UiLayerWithResult<TArgs, TResult> : UiLayer, IUiLayerWithResult<TArgs, TResult> 
        where TResult : class
    {
        public bool WasCancelled { get; set; }
        public bool WasFinished => Result != null;
        public TResult? Result { get; set; }
        public Exception? Exception { get; private set; }

        private LocaleScope LocaleScope = default!;

        public virtual void Cancel()
        {
            WasCancelled = true;
        }

        public virtual void Finish(TResult result)
        {
            Result = result;
        }

        public virtual void Error(Exception ex)
        {
            Exception = ex;
        }

        public virtual void Initialize(TArgs args)
        {
        }

        public virtual UiResult<TResult>? GetResult()
        {
            if (Result != null)
                return new UiResult<TResult>.Finished(Result);
            if (WasCancelled)
                return new UiResult<TResult>.Cancelled();
            if (Exception != null)
                return new UiResult<TResult>.Error(Exception);

            return null;
        }

        public override void Localize(LocaleKey key)
        {
            var manager = IoCManager.Resolve<ILocalizationManager>();
            LocaleScope = new LocaleScope(manager, key);

            manager.DoLocalize(this, key);

            IsLocalized = true;
        }
    }
}
