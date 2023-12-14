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
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.PCCs
{
    public sealed class PCCDrawable : IEntityDrawable
    {
        private readonly Love.Quad[] _quads = new Love.Quad[16];

        [DataField]
        public Dictionary<string, PCCPart> Parts { get; } = new();

        [DataField]
        public int Frame { get; set; }

        [DataField]
        public PCCDirection Direction { get; set; }

        [DataField]
        public bool IsFullSize { get; set; }

        private Love.Image? BakedImage = null;

        private const int PartWidth = 32;
        private const int PartHeight = 48;
        private const int SheetWidth = PartWidth * 4;
        private const int SheetHeight = PartHeight * 4;
        private const int MaxFrames = 16;

        public PCCDrawable() {}

        public PCCDrawable(Dictionary<string, PCCPart> parts)
        {
            Parts = parts;
        }

        public void Initialize(IResourceCache cache)
        {
            AllocateQuads();
            RebakeImage(cache);
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
                foreach (var (_, part) in Parts.OrderBy(pair => pair.Value.ZOrder))
                {
                    var image = cache.GetResource<LoveImageResource>(part.ImagePath).Image;
                    Love.Graphics.SetColor(part.Color);
                    Love.Graphics.Draw(image, 0, 0);
                }
            }
        }

        public void Update(float dt)
        {
        }

        public void Draw(float scale, float x, float y, float scaleX = 1f, float scaleY = 1f)
        {
            if (BakedImage == null)
                return;

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
            GraphicsEx.DrawImageRegion(BakedImage!, quad, x + offsetX * scaleX * scale, y + offsetY * scaleY * scale, width * scaleX * scale, height * scaleY * scale, centered: false);
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
