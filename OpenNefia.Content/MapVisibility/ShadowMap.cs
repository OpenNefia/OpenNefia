using OpenNefia.Core.Game;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Content.MapVisibility
{
    public sealed class ShadowMap
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly ICoords _coords = default!;

        private IMap _map = default!;
        internal ShadowTile[,] ShadowTiles = new ShadowTile[0, 0];
        public Vector2i ShadowPos { get; internal set; }
        public Vector2i ShadowSize { get; internal set; }
        internal UIBox2i ShadowBounds { get => UIBox2i.FromDimensions(ShadowPos, ShadowSize); }

        public void Initialize(IMap map)
        {
            _map = map;
            ShadowTiles = new ShadowTile[map.Width, map.Height];
        }
    }
}
