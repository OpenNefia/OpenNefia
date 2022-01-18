using OpenNefia.Content.PCCs;
using OpenNefia.Content.Rendering;
using OpenNefia.Content.UI.Element;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using static OpenNefia.Content.UI.Element.UiTopicWindow;

namespace OpenNefia.Content.CharaAppearance
{
    public sealed class CharaAppearancePreviewPanel : UiElement
    {
        [Dependency] private readonly ICoords _coords = default!;

        public bool ShowPortrait { get; set; }

        private UiTopicWindow WindowFrame = new(FrameStyleKind.One, WindowStyleKind.One);
        private TileAtlasBatch _chipBatch;
        private TileAtlasBatch _portraitBatch;

        private CharaAppearanceData _data = default!;
        private float _frame;
        private float _pccFrame;

        public CharaAppearancePreviewPanel()
        {
            _chipBatch = new TileAtlasBatch(AtlasNames.Chip);
            _portraitBatch = new TileAtlasBatch(ContentAtlasNames.Portrait);
        }

        public void Initialize(CharaAppearanceData data)
        {
            IoCManager.InjectDependencies(this);

            _data = data;
            _frame = 0f;
            _pccFrame = 0f;
        }

        public override void GetPreferredSize(out Vector2 size)
        {
            size = new(88, 120);
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            WindowFrame.SetSize(Width, Height);
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            WindowFrame.SetPosition(X, Y);
        }

        public override void Update(float dt)
        {
            var delta = dt * 50;
            _frame += delta;

            if (_frame % 100 < 45)
            {
                _pccFrame = _frame % 16;
            }
            else
            {
                _pccFrame += delta;
            }

            _data.PCCDrawable.Frame = Math.Clamp((int)_pccFrame / 4 % 4, 0, 3);
            _data.PCCDrawable.Direction = (PCCDirection)Math.Clamp((int)_pccFrame / 16 % 4, 0, 3);

            WindowFrame.Update(dt);
        }

        public override void Draw()
        {
            WindowFrame.Draw();

            if (ShowPortrait)
            {
                _portraitBatch.Clear();
                _portraitBatch.Add(UIScale, _data.PortraitProto.Image.AtlasIndex, 4, 4, WindowFrame.PixelWidth - 8, WindowFrame.PixelHeight - 8);
                _portraitBatch.Draw(UIScale, WindowFrame.X, WindowFrame.Y);
            }
            else if (_data.UsePCC)
            {
                _data.PCCDrawable.Draw(WindowFrame.PixelX + 44 - 24, WindowFrame.PixelY + 59 - 12, 2.0f, 2.0f);
            }
            else
            {
                _chipBatch.Clear();
                _chipBatch.Add(UIScale, _data.ChipProto.Image.AtlasIndex, 46 - 24, 59 - 24, _coords.TileSize.X, _coords.TileSize.Y, _data.ChipColor);
                _chipBatch.Draw(UIScale, WindowFrame.X, WindowFrame.Y);
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            _chipBatch.Dispose();
            _portraitBatch.Dispose();
        }
    }
}
