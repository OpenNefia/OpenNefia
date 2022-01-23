using OpenNefia.Content.GameObjects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Content.Inventory
{
    public class EntitySpriteBatch : UiElement
    {
        [Dependency] private readonly IEntityMemorySystem _entityMemory = default!;

        private class Entry 
        {
            public MapObjectMemory Memory { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public int? Width { get; set; }
            public int? Height { get; set; }
            public Color Color { get; set; }
            public bool Centered { get; set; }
            public float Rotation { get; set; }

            public Entry(MapObjectMemory memory, int x, int y, int? width, int? height, 
                Color color, bool centered, float rotation)
            {
                Memory = memory;
                X = x;
                Y = y;
                Width = width;
                Height = height;
                Color = color;
                Centered = centered;
                Rotation = rotation;
            }

        }

        private TileAtlasBatch _atlasBatch;

        private readonly List<Entry> _entries = new();

        public EntitySpriteBatch()
        {
            EntitySystem.InjectDependencies(this);

            _atlasBatch = new TileAtlasBatch(AtlasNames.Chip);
        }

        public void Add(EntityUid entity, int x, int y, int? width = null, int? height = null, Color? color = null, bool centered = false, float rotation = 0f)
        {
            color ??= Color.White;

            var memory = _entityMemory.GetEntityMemory(entity);

            var entry = new Entry(memory, x, y, width, height, color.Value, centered, rotation);
            _entries.Add(entry);
        }

        public void Clear()
        {
            _entries.Clear();
        }

        public override void Update(float dt)
        {
        }

        public override void Draw()
        {
            _atlasBatch.Clear();

            foreach (var entry in _entries)
            {
                _atlasBatch.Add(1, entry.Memory.AtlasIndex, entry.X, entry.Y, entry.Width, entry.Height, 
                    entry.Color, entry.Centered, entry.Rotation);
            }

            _atlasBatch.Flush();
            _atlasBatch.Draw(UIScale, X, Y, Width, Height);
        }
    }
}