using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI.Layer
{
    public interface IUiLayerManager
    {
        public IList<IUiLayer> ActiveLayers { get; }

        public IUiLayer? CurrentLayer { get; }

        void Initialize();
        void Shutdown();

        void DrawLayers();
        bool IsQuerying(IUiLayer layer);
        void PopLayer(IUiLayer layer);
        void PushLayer(IUiLayer layer);
        void UpdateLayers(float dt);
        bool IsInActiveLayerList(IUiLayer layer);
        UiResult<T> Query<T>(IUiLayerWithResult<T> layer) where T : class;
    }
}
