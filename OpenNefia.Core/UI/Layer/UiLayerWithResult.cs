using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;

namespace OpenNefia.Core.UI.Layer
{
    public class UiLayerWithResult<TArgs, TResult> : UiLayer, IUiLayerWithResult<TArgs, TResult> 
        where TResult : class
    {
        public bool WasCancelled { get; set; }
        public bool WasFinished => Result != null;
        public bool HasResult => WasCancelled || WasFinished || Exception != null;
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

            base.Localize(key);
        }
    }
}
