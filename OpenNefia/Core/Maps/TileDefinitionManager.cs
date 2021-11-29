using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Maps
{
    public class TileDefinitionManager : ITileDefinitionManager
    {
        protected readonly List<TilePrototype> TileDefs = new();
        private readonly HashSet<PrototypeId<TilePrototype>> _tileIndices = new();
        private readonly Dictionary<ushort, TilePrototype> _tileIds = new();

        public virtual void Initialize()
        {
        }

        public virtual void Register(TilePrototype tileDef)
        {
            var id = tileDef.GetStrongID();
            if (_tileIndices.Contains(id))
            {
                throw new ArgumentException("Another tile definition with the same name has already been registered.", nameof(tileDef));
            }

            var index = checked((ushort)TileDefs.Count);
            tileDef.AssignTileIndex(index);
            TileDefs.Add(tileDef);
            _tileIds[index] = tileDef;
            _tileIndices.Add(id);
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
