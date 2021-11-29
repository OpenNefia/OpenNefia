using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Love;
using OpenNefia.Core.IoC;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.UI
{
    public abstract class BaseUiLayer<T> : BaseInputUiElement, IUiLayerWithResult<T> where T: class
    {
        public virtual int? DefaultZOrder => null;
        public int ZOrder { get; set; }

        public bool WasFinished { get => this.Result != null; }
        public bool WasCancelled { get; private set; }
        public T? Result { get; private set; }
        public bool IsLocalized { get; private set; } = false;
        
        public override sealed void GetPreferredSize(out int width, out int height)
        {
            GetPreferredBounds(out var _, out var _, out width, out height);
        }

        public virtual void GetPreferredBounds(out int x, out int y, out int width, out int height)
        {
            x = 0;
            y = 0;
            width = Love.Graphics.GetWidth();
            height = Love.Graphics.GetHeight();
        }

        public bool IsInActiveLayerList()
        {
            return IoCManager.Resolve<IUiLayerManager>().IsInActiveLayerList(this);
        }

        public bool IsQuerying()
        {
            return IoCManager.Resolve<IUiLayerManager>().IsQuerying(this);
        }

        public virtual void Cancel()
        {
            this.WasCancelled = true;
        }

        public virtual void Finish(T result)
        {
            this.Result = result;
        }

        public virtual void Initialize()
        {
            this.Result = null;
            this.WasCancelled = false;
        }

        public virtual UiResult<T>? GetResult()
        {
            if (this.Result != null)
                return new UiResult<T>.Finished(this.Result);
            if (this.WasCancelled)
                return new UiResult<T>.Cancelled();

            return null;
        }

        public virtual void OnQuery()
        {
        }

        public virtual void OnQueryFinish()
        {

        }

        public void OnLoveKeyPressed(KeyConstant key, bool isRepeat)
        {
            this.ReceiveKeyPressed(key, isRepeat);
        }

        public void OnLoveKeyReleased(KeyConstant key)
        {
            this.ReceiveKeyReleased(key);
        }

        public void OnLoveTextInput(string text)
        {
            this.ReceiveTextInput(text);
        }

        public void OnLoveMouseMoved(float x, float y, float dx, float dy, bool isTouch)
        {
            this.ReceiveMouseMoved(x, y, dx, dy, isTouch);
        }

        public void OnLoveMousePressed(float x, float y, int button, bool isTouch)
        {
            this.ReceiveMousePressed(x, y, button, isTouch);
        }

        public void OnLoveMouseReleased(float x, float y, int button, bool isTouch)
        {
            this.ReceiveMouseReleased(x, y, button, isTouch);
        }

        public override void Localize(LocaleKey key)
        {
            base.Localize(key);
            IsLocalized = true;
        }
    }
}
