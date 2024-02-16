using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;

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

        public ChipComponent? ChipComp { get; set; }
        private PrototypeId<ChipPrototype> _chipId = new("Default");
        public PrototypeId<ChipPrototype> ChipID { get => ChipComp?.ChipID ?? _chipId; set => _chipId = value; }
        private Color _color = Maths.Color.White;
        public Color Color { get => ChipComp?.Color ?? _color; set => _color = value; }
        private TileAtlasBatch _chipBatch;

        public ChipView()
        {
            IoCManager.InjectDependencies(this);

            RectClipContent = true;

            _chipBatch = new TileAtlasBatch(AtlasNames.Chip);
        }

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            var chipProto = _protos.Index(ChipID);
            return _chipBatch.GetTileSize(chipProto.Image) * Scale;
        }

        public override void Draw()
        {
            var chipProto = _protos.Index(ChipID);
            var chipSize = _chipBatch.GetTileSize(chipProto.Image);

            _chipBatch.Clear();
            _chipBatch.Add(UIScale, chipProto.Image.AtlasIndex, 0, 0, chipSize.X, chipSize.Y, Color);
            _chipBatch.Draw(UIScale, GlobalX, GlobalY);
        }

        public override void Dispose()
        {
            base.Dispose();
            _chipBatch?.Dispose();
        }
    }
}
