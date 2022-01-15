using OpenNefia.Content.PCCs;
using OpenNefia.Content.Rendering;
using OpenNefia.Content.UI.Element;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Content.UI.Element.UiTopicWindow;

namespace OpenNefia.Content.Charas
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
        }

        public override void GetPreferredSize(out Vector2i size)
        {
            size = new(88, 120);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            WindowFrame.SetSize(Width, Height);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            WindowFrame.SetPosition(X, Y);
        }

        public override void Update(float dt)
        {
            _frame += dt * 50;

            WindowFrame.Update(dt);

            _data.PCCDrawable.Frame = Math.Clamp(((int)_frame / 4) % 4, 0, 3);
            _data.PCCDrawable.Direction = (PCCDirection)Math.Clamp(((int)_frame / 16) % 4, 0, 3);
        }

        public override void Draw()
        {
            WindowFrame.Draw();

            if (ShowPortrait)
            {
                _portraitBatch.Clear();
                _portraitBatch.Add(_data.PortraitProto.Image.AtlasIndex, 4, 4, WindowFrame.Width - 8, WindowFrame.Height - 8);
                _portraitBatch.Draw(WindowFrame.X, WindowFrame.Y);
            }
            else if (_data.UsePCC)
            {
                _data.PCCDrawable.Draw(WindowFrame.X + 46 - 24, WindowFrame.Y + 59 - 12, 2.0f, 2.0f);
            }
            else
            {
                _chipBatch.Clear();
                _chipBatch.Add(_data.ChipProto.Image.AtlasIndex, 46 - 24, 59 - 24, _coords.TileSize.X, _coords.TileSize.Y, _data.ChipColor);
                _chipBatch.Draw(WindowFrame.X, WindowFrame.Y);
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
