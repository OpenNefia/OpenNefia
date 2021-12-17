using OpenNefia.Core.Maps;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;

namespace OpenNefia.Content.UI.Layer
{
    public interface IFieldLayer : IUiLayerWithResult<UiNoResult>
    {
        Camera Camera { get; }

        void Startup();
        void SetMap(IMap map);
        void RefreshScreen();
    }
}