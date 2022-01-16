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

        /// <summary>
        /// Returns true if the simulation is active (map is being displayed, etc.). This
        /// will also return true if there is an active modal being displayed over the map.
        /// </summary>
        bool IsInGame();
    }
}