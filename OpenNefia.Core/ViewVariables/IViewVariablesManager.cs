using OpenNefia.Core.UI.Wisp;

namespace OpenNefia.Core.ViewVariables
{
    public interface IViewVariablesManager
    {
        /// <summary>
        ///     Open a VV window for a locally existing object.
        /// </summary>
        /// <param name="obj">The object to VV.</param>
        /// <param name="layer">Layer to open VV in. Defaults to the current layer if it's an <see cref="IWispLayer"/>.</param>
        void OpenVV(object obj, IWispLayer? layer = null);
    }
}
