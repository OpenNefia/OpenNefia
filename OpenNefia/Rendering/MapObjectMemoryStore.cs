using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.Rendering
{
    [Serializable]
    public sealed class MapObjectMemoryStore : IEnumerable<MapObjectMemory>
    {
        internal IMap Map;
        internal int CurrentIndex;
        internal Dictionary<int, MapObjectMemory> AllMemory;
        internal List<MapObjectMemory>?[,] Positional;
        internal HashSet<MapObjectMemory> Added;
        internal Stack<MapObjectMemory> Removed;

        private GetMapObjectMemoryEventArgs _event = new(default!);

        public MapObjectMemoryStore(IMap map)
        {
            this.Map = map;
            CurrentIndex = 0;
            this.AllMemory = new Dictionary<int, MapObjectMemory>();
            this.Positional = new List<MapObjectMemory>?[map.Width, map.Height];
            this.Added = new HashSet<MapObjectMemory>();
            this.Removed = new Stack<MapObjectMemory>();
        }

        public IEnumerator<MapObjectMemory> GetEnumerator() => AllMemory.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => AllMemory.Values.GetEnumerator();

        public void ForgetObjects(Vector2i coords)
        {
            var at = Positional[coords.X, coords.Y];

            if (at != null)
            {
                foreach (var memory in at)
                {
                    AllMemory.Remove(memory.Index);
                    Removed.Push(memory);
                }

                Positional[coords.X, coords.Y] = null;
            }
        }

        public void HideObjects(Vector2i coords)
        {
            var at = Positional[coords.X, coords.Y];

            if (at != null)
            {
                foreach (var memory in at)
                {
                    if (memory.HideWhenOutOfSight)
                    {
                        AllMemory.Remove(memory.Index);
                        Removed.Push(memory);
                    }
                }
                if (at.Count == 0)
                    Positional[coords.X, coords.Y] = null;
            }
        }

        internal void Flush()
        {
            foreach (var added in this.Added)
            {
                added.State = MemoryState.InUse;
            }
            this.Map.MapObjectMemory.Added.Clear();
            this.Map.MapObjectMemory.Removed.Clear();
        }

        public void RedrawAll()
        {
            this.Map.MapObjectMemory.Added.Clear();
            this.Map.MapObjectMemory.Removed.Clear();
            foreach (var memory in this.AllMemory.Values)
            {
                memory.State = MemoryState.Added;
                this.Added.Add(memory);
            }
        }

        public void RevealObjects(Vector2i coords)
        {
            var at = Positional[coords.X, coords.Y];

            if (at != null)
            {
                foreach (var memory in at)
                {
                    AllMemory.Remove(memory.Index);
                    Removed.Push(memory);
                }

                at.Clear();
            }

            int i = 0;
            foreach (var obj in Map.AtPos(coords).GetEntities())
            {
                if (at == null)
                {
                    at = new List<MapObjectMemory>();
                    Positional[coords.X, coords.Y] = at;
                }

                var memory = GetOrCreateMemory();

                _event.Memory = memory;
                obj.EntityManager.EventBus.RaiseLocalEvent(obj.Uid, _event);
                memory = _event.Memory;

                if (memory.IsVisible)
                {
                    this.AllMemory[memory.Index] = memory;
                    this.Added.Add(memory);
                    memory.ObjectUid = obj.Uid;
                    memory.Coords = obj.Spatial.Coords;
                    memory.ZOrder = i;
                    at.Add(memory);
                }

                i++;
            }
        }

        public MapObjectMemory GetOrCreateMemory()
        {
            MapObjectMemory memory;

            if (this.Removed.Count > 0)
            {
                // Index is not changed, to support reuse.
                memory = this.Removed.Pop();
            }
            else
            {
                // Allocate a new memory entry and increment the index.
                var index = CurrentIndex;
                CurrentIndex += 1;

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
