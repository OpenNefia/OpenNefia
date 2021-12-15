using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Core.Maps
{
    public class TileDefinitionManager : ITileDefinitionManager
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

        protected readonly List<TilePrototype> TileDefs = new();
        private readonly Dictionary<ushort, TilePrototype> _tileDefs = new();
        private readonly Dictionary<PrototypeId<TilePrototype>, ushort> _tileIndices = new();

        public virtual void Initialize()
        {
            _prototypeManager.PrototypesReloaded += OnPrototypesReloaded;
        }

        private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
        {
            if (args.ByType.TryGetValue(typeof(TilePrototype), out var tiles))
            {
                foreach (var tileDef in tiles.Modified.Values.Cast<TilePrototype>())
                {
                    if (_tileIndices.TryGetValue(tileDef.GetStrongID(), out var index))
                    {
                        tileDef.AssignTileIndex(index);
                    }
                    else
                    {
                        Register(tileDef);
                    }
                }
            }
        }

        public virtual void Register(TilePrototype tileDef)
        {
            var id = tileDef.GetStrongID();
            if (_tileIndices.ContainsKey(id))
            {
                throw new ArgumentException("Another tile definition with the same name has already been registered.", nameof(tileDef));
            }

            var index = checked((ushort)TileDefs.Count);
            tileDef.AssignTileIndex(index);
            TileDefs.Add(tileDef);
            _tileDefs[index] = tileDef;
            _tileIndices[id] = index;
        }

        public TilePrototype this[int index] => TileDefs[index];

        public int Count => TileDefs.Count;

        public IEnumerator<TilePrototype> GetEnumerator()
        {
            return TileDefs.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
