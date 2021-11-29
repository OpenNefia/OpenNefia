using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Love;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
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
        public Exception? Exception { get; private set; }
        
        public override sealed void GetPreferredSize(out Vector2i size)
        {
            GetPreferredBounds(out var bounds);
            size = bounds.Size;
        }

        public virtual void GetPreferredBounds(out Box2i bounds)
        {
            bounds = Box2i.FromDimensions(0, 0, Love.Graphics.GetWidth(), Love.Graphics.GetHeight());
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

        public virtual void Error(Exception ex)
        {
            this.Exception = ex;
        }

        public void Initialize()
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
            if (this.Exception != null)
                return new UiResult<T>.Error(this.Exception);

            return null;
        }

        public virtual void OnQuery()
        {
        }

        public virtual void OnQueryFinish()
        {

        }

        public override void Localize(LocaleKey key)
        {
            base.Localize(key);
            IsLocalized = true;
        }
    }
}
