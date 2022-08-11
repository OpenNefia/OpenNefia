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
        [Dependency] private readonly ICoords _coords = default!;

        private class Entry 
        {
            public MapObjectMemory Memory { get; set; }
            public float X { get; set; }
            public float Y { get; set; }
            public float? Width { get; set; }
            public float? Height { get; set; }
            public Color Color { get; set; }
            public bool Centered { get; set; }
            public float Rotation { get; set; }

            public Entry(MapObjectMemory memory, float x, float y, float? width, float? height, 
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

        public enum Centering
        {
            None,
            Centered,
            AlignBottom
        }

        public void Add(EntityUid entity, float x, float y, float? width = null, float? height = null, Color? color = null, Centering centering = Centering.None, float rotation = 0f)
        {
            color ??= Color.White;

            var memory = _entityMemory.GetEntityMemory(entity);

            var centered = centering == Centering.Centered;
            if (centering == Centering.AlignBottom)
            {
                var size = GetTileSize(memory.AtlasIndex);
                y -= size.Y - _coords.TileSize.Y;
            }

            var entry = new Entry(memory, x, y, width, height, color.Value, centered, rotation);
            _entries.Add(entry);
        }

        public Vector2i GetTileSize(TileSpecifier spec) => _atlasBatch.GetTileSize(spec);
        public Vector2i GetTileSize(string tileId) => _atlasBatch.GetTileSize(tileId);

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
                _atlasBatch.Add(UIScale, entry.Memory.AtlasIndex, entry.X, entry.Y, entry.Width, entry.Height, 
                    entry.Color, entry.Centered, entry.Rotation);
            }

            _atlasBatch.Flush();
            _atlasBatch.Draw(UIScale, X, Y /*, TODO Width, Height */);
        }
    }
}