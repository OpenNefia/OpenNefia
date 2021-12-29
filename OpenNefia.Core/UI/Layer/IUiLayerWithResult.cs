using OpenNefia.Core.IoC;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.UI.Layer
{
    public interface IUiLayerWithResult<T> : IUiLayer where T: class
    {
        int? DefaultZOrder { get; }

        bool WasFinished { get; }
        bool WasCancelled { get; }
        T? Result { get; }

        void Initialize();
        void Cancel();
        void Finish(T result);
        UiResult<T>? GetResult();
    }
}
