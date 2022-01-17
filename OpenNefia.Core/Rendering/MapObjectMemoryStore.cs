using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Core.Rendering
{
    [DataDefinition]
    public sealed class MapObjectMemoryStore
    {
        [DataField("allMemory")]
        private Dictionary<int, MapObjectMemory> _allMemory = new();

        public IReadOnlyDictionary<int, MapObjectMemory> AllMemory => _allMemory;

        [DataField("width")]
        private int _width;

        [DataField("height")]
        private int _height;

        [DataField("currentIndex")]
        private int _currentIndex = 0;

        [DataField("positional")]
        private List<MapObjectMemory>?[,] _positional;

        private HashSet<MapObjectMemory> _added = new();
        private Stack<MapObjectMemory> _removed = new();

        private GetMapObjectMemoryEventArgs _event = new(default!);

        public MapObjectMemoryStore() : this(0, 0) { }
        public MapObjectMemoryStore(int width, int height)
        {
            _width = width;
            _height = height;

            _positional = new List<MapObjectMemory>?[width, height];
        }

        public void ForgetObjects(Vector2i coords)
        {
            var at = _positional[coords.X, coords.Y];

            if (at != null)
            {
                foreach (var memory in at)
                {
                    _allMemory.Remove(memory.Index);
                    _removed.Push(memory);
                }

                _positional[coords.X, coords.Y] = null;
            }
        }

        public void HideObjects(Vector2i coords)
        {
            var at = _positional[coords.X, coords.Y];

            if (at != null)
            {
                foreach (var memory in at)
                {
                    if (memory.HideWhenOutOfSight)
                    {
                        _allMemory.Remove(memory.Index);
                        _removed.Push(memory);
                    }
                }
                if (at.Count == 0)
                    _positional[coords.X, coords.Y] = null;
            }
        }

        internal void Flush()
        {
            foreach (var added in _added)
            {
                added.State = MemoryState.InUse;
            }
            _added.Clear();
            _removed.Clear();
        }

        public void RedrawAll()
        {
            _added.Clear();
            _removed.Clear();
            foreach (var memory in _allMemory.Values)
            {
                memory.State = MemoryState.Added;
                _added.Add(memory);
            }
        }

        public void RevealObjects(IMap map, Vector2i pos, IEntityManager entityManager)
        {
            var at = _positional[pos.X, pos.Y];

            if (at != null)
            {
                foreach (var memory in at)
                {
                    _allMemory.Remove(memory.Index);
                    _removed.Push(memory);
                }

                at.Clear();
            }

            var lookup = EntitySystem.Get<IEntityLookup>();

            int i = 0;
            foreach (var spatial in lookup.GetLiveEntitiesAtCoords(map.AtPos(pos)))
            {
                if (at == null)
                {
                    at = new List<MapObjectMemory>();
                    _positional[pos.X, pos.Y] = at;
                }

                var memory = GetOrCreateMemory();

                _event.Memory = memory;
                entityManager.EventBus.RaiseLocalEvent(spatial.Owner, _event);
                memory = _event.Memory;

                if (memory.IsVisible)
                {
                    _allMemory[memory.Index] = memory;
                    _added.Add(memory);
                    memory.ObjectUid = spatial.Owner;
                    memory.Coords = spatial.MapPosition;
                    at.Add(memory);
                }

                i++;
            }
        }

        public MapObjectMemory GetOrCreateMemory()
        {
            MapObjectMemory memory;

            if (_removed.Count > 0)
            {
                // Index is not changed, to support reuse.
                memory = _removed.Pop();
                memory.AtlasIndex = "Default:Default";
            }
            else
            {
                // Allocate a new memory entry and increment the index.
                var index = _currentIndex;
                _currentIndex += 1;

                memory = new MapObjectMemory()
                {
                    Index = index
                };
            }

            memory.State = MemoryState.Added;

            return memory;
        }
    }

    public sealed class GetMapObjectMemoryEventArgs : EntityEventArgs
    {
        public MapObjectMemory Memory;

        public GetMapObjectMemoryEventArgs(MapObjectMemory memory)
        {
            Memory = memory;
        }
    }
}
