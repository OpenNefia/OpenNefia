using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Core.Maps
{
    public class TileDefinitionManager : ITileDefinitionManagerInternal
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

        public void RegisterAll()
        {
            // Tile.Empty relies on the Empty tile prototype being registered first
            // so it gets index 0.
            var emptyDef = _prototypeManager.Index(Tile.EmptyID);

            Register(emptyDef);

            var prototypeList = new List<TilePrototype>();
            foreach (var tileDef in _prototypeManager.EnumeratePrototypes<TilePrototype>())
            {
                if (tileDef.GetStrongID() == Tile.EmptyID)
                    continue;

                prototypeList.Add(tileDef);
            }

            // Ensure deterministic ordering for save files, etc.
            prototypeList.Sort((a, b) => string.Compare(a.ID, b.ID, StringComparison.Ordinal));

            foreach (var tileDef in prototypeList)
            {
                Register(tileDef);
            }
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
