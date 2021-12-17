using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Core.Rendering
{
    public interface ITileDefinition : IHspIds<Vector2i>
    {
        /// <summary>
        /// The in-code name of this tile definition.
        /// </summary>
        public string ID { get; }

        /// <summary>
        ///     The numeric tile ID used to refer to this tile inside the map datastructure.
        /// </summary>
        int TileIndex { get; }

        /// <summary>
        ///     Assign a new value to <see cref="TileId"/>, used when registering the tile definition.
        /// </summary>
        /// <param name="id">The new tile ID for this tile definition.</param>
        void AssignTileIndex(int id);
    }
}