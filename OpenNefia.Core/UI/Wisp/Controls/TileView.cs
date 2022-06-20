using OpenNefia.Core.Maths;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.UI.Wisp;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Core.UI.Wisp.Controls
{
    public class ChipView : WispControl
    {
        [Dependency] private readonly ICoords _coords = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;

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

        public ChipComponent? Chip { get; set; }
        private TileAtlasBatch _chipBatch;

        public ChipView()
        {
            IoCManager.InjectDependencies(this);

            RectClipContent = true;

            _chipBatch = new TileAtlasBatch(AtlasNames.Chip);
        }

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            if (Chip == null || Chip.Deleted)
                return _coords.TileSize * Scale;

            var chipProto = _protos.Index(Chip.ChipID);
            return _chipBatch.GetTileSize(chipProto.Image) * Scale;
        }

        public override void Draw()
        {
            if (Chip == null || Chip.Deleted)
            {
                return;
            }

            var chipProto = _protos.Index(Chip.ChipID);

            _chipBatch.Clear();
            _chipBatch.Add(UIScale, chipProto.Image.AtlasIndex, 0, 0, _coords.TileSize.X, _coords.TileSize.Y, Chip.Color);
            _chipBatch.Draw(UIScale, GlobalPixelX, GlobalPixelY);
        }
    }
}
