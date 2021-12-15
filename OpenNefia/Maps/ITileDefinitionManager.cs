using OpenNefia.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Core.Maps
{
    /// <summary>
    ///     This manages tile definitions for grid tiles.
    /// </summary>
    public interface ITileDefinitionManager : IEnumerable<TilePrototype>
    {
        /// <summary>
        ///     Indexer to retrieve a tile definition by internal ID.
        /// </summary>
        /// <param name="id">The ID of the tile definition.</param>
        /// <returns>The tile definition.</returns>
        TilePrototype this[int id] { get; }

        /// <summary>
        ///     The number of tile definitions contained inside of this manager.
        /// </summary>
        int Count { get; }

        void Initialize();

        /// <summary>
        ///     Register a definition with this manager.
        /// </summary>
        /// <param name="tileDef">THe definition to register.</param>
        void Register(TilePrototype tileDef);
    }
}
