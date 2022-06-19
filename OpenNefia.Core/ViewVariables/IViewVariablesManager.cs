namespace OpenNefia.Core.ViewVariables
{
    public interface IViewVariablesManager
    {
        /// <summary>
        ///     Open a VV window for a locally existing object.
        /// </summary>
        /// <param name="obj">The object to VV.</param>
        void OpenVV(object obj);
    }
}
