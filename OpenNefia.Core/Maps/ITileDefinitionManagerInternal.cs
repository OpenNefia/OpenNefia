namespace OpenNefia.Core.Maps
{
    internal interface ITileDefinitionManagerInternal : ITileDefinitionManager
    {
        /// <summary>
        /// Registers all tiles loaded in the <see cref="IPrototypeManager"/>
        /// </summary>
        void RegisterAll();
    }
}