using System;
using System.Collections.Generic;
using System.Text;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.UI.Wisp.Controls
{
    /// <summary>
    ///     A label is a GUI control that displays simple text.
    /// </summary>
    public class Label : WispControl
    {
        public const string StylePropertyFontColor = "fontColor";
        public const string StylePropertyFont = "font";
        public const string StylePropertyAlignMode = "alignMode";

        private int _cachedTextHeight;
        private readonly List<int> _cachedTextWidths = new();
        private bool _textDimensionCacheValid;
        private string? _text;
        private bool _clipText;
        private AlignMode _align;

        public Label()
        {
            VerticalAlignment = VAlignment.Center;
        }

        private List<Love.Text> _splitText = new();

        /// <summary>
        ///     The text to display.
        /// </summary>
        public string? Text
        {
            get => _text;
            set
            {
                if (_text == value)
                {
                    return;
                }
                _text = value;

                _textDimensionCacheValid = false;
                InvalidateMeasure();
            }
        }

        public bool ClipText
        {
            get => _clipText;
            set
            {
                _clipText = value;
                RectClipContent = value;
                InvalidateMeasure();
            }
        }

        public AlignMode Align
        {
            get
            {
                if (TryGetStyleProperty<AlignMode>(StylePropertyAlignMode, out var alignMode))
                {
                    return alignMode;
                }

                return _align;
            }
            set => _align = value;
        }

        public VAlignMode VAlign { get; set; }

        private FontSpec? _fontOverride;
        public FontSpec? FontOverride
        {
            get => _fontOverride;
            set
            {
                _fontOverride = value;
                RebakeText();
            }
        }

        private FontSpec ActualFont
        {
            get
            {
                if (FontOverride != null)
                {
                    return FontOverride;
                }

                if (TryGetStyleProperty<FontSpec>(StylePropertyFont, out var font))
                {
                    return font;
                }

                return WispManager.GetStyleFallback<FontSpec>();
            }
        }

        public Color? FontColorShadowOverride { get; set; }

        private Color ActualFontColor
        {
            get
            {
                if (FontColorOverride.HasValue)
                {
                    return FontColorOverride.Value;
                }

                if (TryGetStyleProperty<Color>(StylePropertyFontColor, out var color))
                {
                    return color;
                }

                return WispManager.GetStyleFallback<Color>();
            }
        }

        public Color? FontColorOverride { get; set; }

        public int? ShadowOffsetXOverride { get; set; }

        public int? ShadowOffsetYOverride { get; set; }

        /// <summary>
        /// Horizontal alignment mode for label text.
        /// </summary>
        /// <remarks>
        /// NOTE: Separate from <see cref="WispControl.HAlignment"/>!
        /// </remarks>
        public enum AlignMode : byte
        {
            Left = 0,
            Center = 1,
            Right = 2,
            Fill = 3
        }

        /// <summary>
        /// Vertical alignment mode for label text.
        /// </summary>
        /// <remarks>
        /// NOTE: Separate from <see cref="WispControl.VAlignment"/>!
        /// </remarks>
        public enum VAlignMode : byte
        {
            Top = 0,
            Center = 1,
            Bottom = 2,
            Fill = 3
        }

        private void RebakeText()
        {
            _splitText.Clear();

            if (_text != null)
            {
                _splitText = _text.Split("\n").Select(s =>
                {
                    var text = Love.Graphics.NewText(ActualFont.LoveFont, s);
                    return text;
                }).ToList();
            }
        }

        public override void Draw()
        {
            if (_text == null)
            {
                return;
            }

            if (!_textDimensionCacheValid)
            {
                RebakeText();
                _calculateTextDimension();
                DebugTools.Assert(_textDimensionCacheValid);
            }

            int vOffset;
            switch (VAlign)
            {
                case VAlignMode.Top:
                    vOffset = 0;
                    break;
                case VAlignMode.Fill:
                case VAlignMode.Center:
                    vOffset = (PixelSize.Y - _cachedTextHeight) / 2;
                    break;
                case VAlignMode.Bottom:
                    vOffset = PixelSize.Y - _cachedTextHeight;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var newlines = 0;
            var font = ActualFont;
            var actualFontColor = ActualFontColor;

            Vector2 CalcBaseline()
            {
                DebugTools.Assert(_textDimensionCacheValid);

                int hOffset;
                switch (Align)
                {
                    case AlignMode.Left:
                        hOffset = 0;
                        break;
                    case AlignMode.Center:
                    case AlignMode.Fill:
                        hOffset = (PixelSize.X - _cachedTextWidths[newlines]) / 2;
                        break;
                    case AlignMode.Right:
                        hOffset = PixelSize.X - _cachedTextWidths[newlines];
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                return (hOffset, font.LoveFont.GetHeight() * newlines + vOffset);
            }

            var baseLine = CalcBaseline();

            GraphicsS.SetColorTinted(this, actualFontColor);

            // TODO: need better clipping management (global stack used by WispManager)
            if (ClipText)
                WispRootLayer!.PushScissor(GlobalPixelRect);

            foreach (var line in _splitText)
            {
                Love.Graphics.Draw(line, GlobalPixelPosition.X + baseLine.X, GlobalPixelPosition.Y + baseLine.Y);
                newlines += 1;
                baseLine = CalcBaseline();
            }

            if (ClipText)
                WispRootLayer!.PopScissor();
        }

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            if (!_textDimensionCacheValid)
            {
                RebakeText();
                _calculateTextDimension();
                DebugTools.Assert(_textDimensionCacheValid);
            }

            if (ClipText)
            {
                return (0, _cachedTextHeight / UIScale);
            }

            var totalWidth = 0;
            foreach (var width in _cachedTextWidths)
            {
                totalWidth = Math.Max(totalWidth, width);
            }

            return (totalWidth / UIScale, _cachedTextHeight / UIScale);
        }

        protected internal override void UIScaleChanged(GUIScaleChangedEventArgs args)
        {
            _textDimensionCacheValid = false;

            base.UIScaleChanged(args);
        }

        private void _calculateTextDimension()
        {
            _cachedTextWidths.Clear();

            if (_text == null)
            {
                _cachedTextHeight = 0;
                _textDimensionCacheValid = true;
                return;
            }

            var font = ActualFont;
            _cachedTextWidths.AddRange(_splitText.Select(line => line.GetWidth()));
            _cachedTextHeight = (font.LoveFont.GetHeight() * _splitText.Count);

            _cachedTextWidths.Add(0);
            _textDimensionCacheValid = true;
        }

        protected override void StylePropertiesChanged()
        {
            _textDimensionCacheValid = false;

            base.StylePropertiesChanged();
        }
    }
}
