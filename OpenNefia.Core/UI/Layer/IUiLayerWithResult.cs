using OpenNefia.Core.IoC;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.UI.Layer
{
    public interface IUiLayerWithResult<TArgs, TResult> : IUiLayer where TResult: class
    {
        int? DefaultZOrder { get; }

        bool WasCancelled { get; set; }
        TResult? Result { get; set; }

        void Initialize(TArgs args);
        void Cancel();
        void Finish(TResult result);
        UiResult<TResult>? GetResult();
    }
}
