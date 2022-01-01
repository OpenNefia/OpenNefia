using OpenNefia.Core.Maps;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;

namespace OpenNefia.Content.UI.Layer
{
    public interface IFieldLayer : IUiLayerWithResult<UINone, UINone>
    {
        Camera Camera { get; }

        void Startup();
        void SetMap(IMap map);
        void RefreshScreen();
    }
}