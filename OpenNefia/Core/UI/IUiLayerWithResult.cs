using OpenNefia.Core.IoC;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.UI
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

    public static class IUiLayerWithResultExt
    {
        public static UiResult<T> Query<T>(this IUiLayerWithResult<T> layer) where T : class
        {
            return IoCManager.Resolve<IUiLayerManager>().Query(layer);
        }
    }
}
