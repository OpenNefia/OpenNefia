using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Wisp;
using OpenNefia.Core.UI.Wisp.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.ControlDesigner
{
    public sealed class MeasurementPointer : WispControl
    {
        public const string StylePropertyRulerColor = "rulerColor";
        public const string StylePropertyRulerThickness = "rulerThickness";
        public const string StyleClassPointerText = "pointerText";

        private Label _label;

        public MeasurementPointer()
        {
            MaxSize = ExactSize = (25, 25);
            _label = new Label()
            {
                Class = StyleClassPointerText,
                Margin = new Thickness(5, 0)
            };
            AddChild(_label);
        }

        /// <summary>
        /// What control this pointer is measuring against.
        /// </summary>
        public WispControl? Target { get; set; }

        public Color? RulerColorOverride { get; set; }
        private Color RulerColor
        {
            get
            {
                if (RulerColorOverride.HasValue)
                {
                    return RulerColorOverride.Value;
                }
                
                if (TryGetStyleProperty<Color>(StylePropertyRulerColor, out var color))
                {
                    return color;
                }

                return WispManager.GetStyleFallback<Color>();
            }
        }

        public int? RulerThicknessOverride { get; set; }
        public int RulerThickness
        {
            get
            {
                if (RulerThicknessOverride.HasValue)
                {
                    return RulerThicknessOverride.Value;
                }

                if (TryGetStyleProperty<int>(StylePropertyRulerThickness, out var color))
                {
                    return color;
                }

                return 1;
            }
        }

        public override void Draw()
        {
            base.Draw();

            GraphicsS.SetColorTinted(this, RulerColor);
            Love.Graphics.SetLineWidth(RulerThickness);

            Love.Graphics.Line(GlobalPixelX, 0, GlobalPixelX, WispRootLayer!.WispRoot.PixelHeight);
            Love.Graphics.Line(0, GlobalPixelY + PixelHeight, WispRootLayer!.WispRoot.PixelWidth, GlobalPixelY + PixelHeight);

            Love.Graphics.SetLineWidth(1); // TODO remove
        }

        public override void Update(float dt)
        {
            base.Update(dt);

            var (x, y) = GlobalPixelPosition;
            string? text = null;

            if (Target != null)
            {
                var (tx, ty) = Target.GlobalPixelPosition;
                text = $"({x - tx}, {y - ty})";
            }

            _label.Text = text;
        }
    }
}
