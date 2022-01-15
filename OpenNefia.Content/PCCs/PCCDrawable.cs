using OpenNefia.Core.Prototypes;
using OpenNefia.Core.UI.Element;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Utility;
using OpenNefia.Core.ResourceManagement;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Rendering;

namespace OpenNefia.Content.PCCs
{
    public sealed class PCCDrawable : IEntityDrawable
    {
        private const int DefaultPCCPartZOrder = 100000;

        private readonly Dictionary<string, PCCPart> _parts;
        private readonly Love.Quad[] _quads = new Love.Quad[16];

        public IReadOnlyDictionary<string, PCCPart> Parts => _parts;

        public int Frame { get; set; }
        public PCCDirection Direction { get; set; }
        public bool IsFullSize { get; set; }

        private Love.Image? BakedImage = null;

        private const int PartWidth = 32;
        private const int PartHeight = 48;
        private const int SheetWidth = PartWidth * 4;
        private const int SheetHeight = PartHeight * 4;
        private const int MaxFrames = 16;

        public PCCDrawable(Dictionary<string, PCCPart> parts)
        {
            _parts = parts;

            foreach (var (_, part) in _parts)
            {
                if (part.ZOrder == null)
                {
                    part.ZOrder = PCCConstants.DefaultPartZOrders.GetValueOr(part.Type, DefaultPCCPartZOrder);
                }
            }

            AllocateQuads();
        }

        private void AllocateQuads()
        {
            for (int i = 0; i < 16; i++)
            {
                var x = i / 4;
                var y = i % 4;

                _quads[i]?.Dispose();
                _quads[i] = Love.Graphics.NewQuad(x * PartWidth, y * PartHeight, 
                    PartWidth, PartHeight, 
                    SheetWidth, SheetHeight);
            }
        }

        public void RebakeImage(IResourceCache cache)
        {
            BakedImage?.Dispose();

            var canvas = Love.Graphics.NewCanvas(SheetWidth, SheetHeight);
            canvas.SetFilter(Love.FilterMode.Nearest, Love.FilterMode.Nearest, 1);

            GraphicsEx.WithCanvas(canvas, DoRebake);

            BakedImage = Love.Graphics.NewImage(canvas.NewImageData());
            BakedImage.SetFilter(Love.FilterMode.Nearest, Love.FilterMode.Nearest, 1);

            canvas.Dispose();

            void DoRebake()
            {
                foreach (var part in _parts.OrderBy(pair => pair.Value.ZOrder ?? DefaultPCCPartZOrder))
                {
                    var image = cache.GetResource<LoveImageResource>(part.ImagePath).Image;
                    Love.Graphics.SetColor(part.Color);
                    Love.Graphics.Draw(image, 0, 0);
                }

            }
        }

        public void NextFrame()
        {
            Frame = (Frame + 1) % MaxFrames;
        }

        public void Update(float dt)
        {
        }

        public void Draw(float x, float y, float scaleX, float scaleY)
        {
            int width, height, offsetX, offsetY;

            if (IsFullSize)
            {
                width = PartWidth;
                height = PartHeight;
                offsetX = 8;
                offsetY = -4;
            }
            else
            {
                width = 24;
                height = 40;
                offsetX = 12;
                offsetY = 4;
            }

            var index = Math.Clamp((int)Direction + Frame * 4, 0, MaxFrames - 1);
            var quad = _quads[index];

            Love.Graphics.SetColor(Love.Color.White);
            GraphicsEx.DrawImageRegion(BakedImage!, quad, x + offsetX * scaleX, y + offsetY * scaleY, width * scaleX, height * scaleY, centered: true);
        }

        public void Dispose()
        {
            BakedImage?.Dispose();
            foreach (var quad in _quads)
            {
                quad?.Dispose();
            }
        }
    }
}
