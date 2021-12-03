using Love;
using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI.Element
{
    public class UiFpsCounter : BaseUiElement
    {
        float Ms = 0f;
        uint Frames = 0;
        float Threshold = 0f;
        float PrevFps = 0f;
        float PrevRam = 0f;
        float PrevRamDiff = 0f;
        DateTime Now;

        private FontSpec FontText = UiFonts.FpsCounter;

        public bool ShowDrawStats { get; set; } = true;
        public IUiText Text { get; }

        public UiFpsCounter()
        {
            Now = DateTime.Now;
            Text = new UiTextOutlined(FontText);
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            this.Text.SetPosition(x, y);
        }

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            this.Text.SetSize(width, height);
        }

        public override void GetPreferredSize(out Vector2i size)
        {
            this.Text.GetPreferredSize(out size);
        }

        public override void Update(float dt)
        {
            var now = DateTime.Now;
            var timeDiff = now - Now;
            Now = now;
            Ms += (float)timeDiff.TotalMilliseconds;
            Frames += 1;

            if (Ms >= Threshold)
            {
                var fps = (float)Frames / (Ms / 1000f);
                var ram = (float)GC.GetTotalMemory(false) / 1024f / 1024f;
                var ramDiff = ram - PrevRam;

                var buff = $"FPS: {fps:n2}\nRAM: {ram:n2}MB\nRMD: {ramDiff:n4}MB";
                Frames = 0;
                Ms = 0;

                if (this.ShowDrawStats)
                {
                    Love.Graphics.GetStats(out var drawCalls,
                        out var canvasSwitches,
                        out var shaderSwitches,
                        out var canvases,
                        out var images,
                        out var fonts,
                        out var textureMemory);

                    buff += $"\nDRW: {drawCalls}\nCNV: {canvasSwitches}\nTXTR: {(textureMemory / 1024f / 1024f):n2}MB\nIMG: {images}\nCNVS: {canvases}\nFNTS: {fonts}";
                }

                Text.Text = buff;

                PrevFps = fps;
                PrevRam = ram;
                PrevRamDiff = ramDiff;
            }
        }

        public override void Draw()
        {
            Text.Draw();
        }

        public override void Dispose()
        {
            Text.Dispose();
        }
    }
}
