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
using OpenNefia.Core.Log;

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

        public PCCDrawable() : this(false) { }

        public PCCDrawable(bool isFullSize)
        {
            IsFullSize = isFullSize;
        }

        public PCCDrawable(Dictionary<string, PCCPart> parts, bool isFullSize = false)
        {
            Parts = parts;
            IsFullSize = isFullSize;
        }

        public void Initialize(IResourceCache cache)
        {
            AllocateQuads();
            RebakeImage(cache);
        }

        private void AllocateQuads()
        {
            if (_quads[0] != null)
                return;

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
                    if (cache.TryGetResource<LoveImageResource>(part.ImagePath, out var resource))
                    {
                        Love.Graphics.SetColor(part.Color);
                        Love.Graphics.Draw(resource.Image, 0, 0);
                    }
                    else
                    {
                        Logger.WarningS("pcc", $"PCC part at path {part.ImagePath} was missing.");
                    }
                }
            }
        }

        public void Update(float dt)
        {
        }

        public void Draw(float scale, float x, float y, bool centered = false)
        {
            if (BakedImage == null)
                return;

            int width, height;
            int offsetX = 0;
            int offsetY = 0;

            if (IsFullSize)
            {
                width = PartWidth;
                height = PartHeight;
                if (!centered)
                {
                    offsetX = 8;
                    offsetY = -4;
                }
            }
            else
            {
                width = 24;
                height = 40;
                if (!centered)
                {
                    offsetX = 12;
                    offsetY = 4;
                }
            }

            var index = Math.Clamp((int)Direction + Frame * 4, 0, MaxFrames - 1);
            var quad = _quads[index];

            Love.Graphics.SetColor(Love.Color.White);
            GraphicsEx.DrawImageRegion(BakedImage!, quad, x + offsetX * scale, y + offsetY * scale, width * scale, height * scale, centered: centered);
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
