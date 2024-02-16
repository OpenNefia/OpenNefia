using OpenNefia.Core.Maths;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Maps;

namespace OpenNefia.Core.UI.Wisp.Controls
{
    public class TileView : WispControl
    {
        [Dependency] private readonly ICoords _coords = default!;

        private Vector2 _scale = (1, 1);

        public Vector2 Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                InvalidateMeasure();
            }
        }

        public TilePrototype? Tile { get; set; }
        private TileAtlasBatch _tileBatch;

        public TileView()
        {
            IoCManager.InjectDependencies(this);

            RectClipContent = true;

            _tileBatch = new TileAtlasBatch(AtlasNames.Tile);
        }

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            return _coords.TileSize * Scale;
        }

        public override void Draw()
        {
            if (Tile == null)
            {
                return;
            }

            _tileBatch.Clear();
            _tileBatch.Add(UIScale, Tile.Image.AtlasIndex, 0, 0, _coords.TileSize.X, _coords.TileSize.Y, Color.White);
            _tileBatch.Draw(UIScale, GlobalX, GlobalY);
        }

        public override void Dispose()
        {
            base.Dispose();
            _tileBatch.Dispose();
        }
    }
}
