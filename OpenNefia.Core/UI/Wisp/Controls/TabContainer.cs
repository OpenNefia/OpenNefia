using System;
using OpenNefia.Core.Input;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Wisp.Drawing;
using OpenNefia.Core.UserInterface;

namespace OpenNefia.Core.UI.Wisp.Controls
{
    public class TabContainer : Container
    {
        public static readonly AttachedProperty<bool> TabVisibleProperty = AttachedProperty<bool>.Create("TabVisible", typeof(TabContainer), true);
        public static readonly AttachedProperty<string?> TabTitleProperty = AttachedProperty<string?>.CreateNull("TabTitle", typeof(TabContainer));

        public const string StylePropertyTabStyleBox = "tabStyleBox";
        public const string StylePropertyTabStyleBoxInactive = "tabStyleBoxInactive";
        public const string stylePropertyTabFontColor = "tabFontColor";
        public const string StylePropertyTabFontColorInactive = "tabFontColorInactive";
        public const string StylePropertyPanelStyleBox = "panelStyleBox";
        public const string StylePropertyFont = "font";

        private int _currentTab;
        private bool _tabsVisible = true;

        public int CurrentTab
        {
            get => _currentTab;
            set
            {
                if (_currentTab < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Current tab must be positive.");
                }

                if (_currentTab >= ChildCount)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value,
                        "Current tab must less than the amount of tabs.");
                }

                if (_currentTab == value)
                {
                    return;
                }

                var old = _currentTab;
                _currentTab = value;

                GetChild(old).Visible = false;
                var newSelected = GetChild(value);
                newSelected.Visible = true;
                InvalidateMeasure();

                OnTabChanged?.Invoke(value);
            }
        }

        public bool TabsVisible
        {
            get => _tabsVisible;
            set
            {
                _tabsVisible = value;
                InvalidateMeasure();
            }
        }

        public event Action<int>? OnTabChanged;

        public TabContainer()
        {
            EventFilter = UIEventFilterMode.Pass;
        }

        public string GetActualTabTitle(int tab)
        {
            var control = GetWispChild(tab);
            var title = control.GetValue(TabTitleProperty);

            return title ?? control.Name ?? Loc.GetString("OpenNefia.UI.TabContainer.NoTabTitleProvided");
        }

        public static string? GetTabTitle(WispControl control)
        {
            return control.GetValue(TabTitleProperty);
        }

        public bool GetTabVisible(int tab)
        {
            var control = GetWispChild(tab);
            return GetTabVisible(control);
        }

        public static bool GetTabVisible(WispControl control)
        {
            return control.GetValue(TabVisibleProperty);
        }

        public void SetTabTitle(int tab, string title)
        {
            var control = GetWispChild(tab);
            SetTabTitle(control, title);
        }

        public static void SetTabTitle(WispControl control, string title)
        {
            control.SetValue(TabTitleProperty, title);
        }

        public void SetTabVisible(int tab, bool visible)
        {
            var control = GetWispChild(tab);
            SetTabVisible(control, visible);
        }

        public static void SetTabVisible(WispControl control, bool visible)
        {
            control.SetValue(TabVisibleProperty, visible);
        }

        protected override void ChildAdded(UiElement newChild)
        {
            base.ChildAdded(newChild);

            if (ChildCount == 1)
            {
                // This is our first child so it must always be visible.
                newChild.Visible = true;
            }
            else
            {
                // If not this can't be the currently selected tab so just make it invisible immediately.
                newChild.Visible = false;
            }
        }

        public override void Draw()
        {
            base.Draw();

            // First, draw panel.
            var headerSize = _getHeaderSize();
            var panel = _getPanel();
            var panelBox = new UIBox2(0, headerSize, PixelWidth, PixelHeight);

            panel?.Draw(panelBox.Translated(GlobalPixelPosition), WispRootLayer!.GlobalTint);

            var font = _getFont();
            var boxActive = _getTabBoxActive();
            var boxInactive = _getTabBoxInactive();
            var fontColorActive = _getTabFontColorActive();
            var fontColorInactive = _getTabFontColorInactive();

            var headerOffset = 0f;

            // Then, draw the tabs.
            for (var i = 0; i < ChildCount; i++)
            {
                if (!GetTabVisible(i))
                {
                    continue;
                }

                var title = GetActualTabTitle(i);

                var titleLength = font.LoveFont.GetWidth(title);
                //// Get string length.
                //foreach (var rune in title.EnumerateRunes())
                //{
                //    if (!font.TryGetCharMetrics(rune, UIScale, out var metrics))
                //    {
                //        continue;
                //    }
                //
                //    titleLength += metrics.Advance;
                //}

                var active = _currentTab == i;
                var box = active ? boxActive : boxInactive;
                var fontColor = active ? fontColorActive : fontColorInactive;

                UIBox2 contentBox;
                var topLeft = new Vector2(headerOffset, 0);
                var size = new Vector2(titleLength, font.LoveFont.GetHeightV(UIScale));
                float boxAdvance;

                if (box != null)
                {
                    var drawBox = box.GetEnvelopBox(GlobalPixelPosition + topLeft, size);
                    boxAdvance = drawBox.Width;
                    box.Draw(drawBox, WispRootLayer!.GlobalTint);
                    contentBox = box.GetContentBox(drawBox);
                }
                else
                {
                    boxAdvance = size.X;
                    contentBox = UIBox2.FromDimensions(GlobalPosition + topLeft, size);
                }

                var baseLine = new Vector2(0, 0) + contentBox.TopLeft;

                GraphicsS.SetColorTinted(this, fontColor);
                Love.Graphics.SetFont(font.LoveFont);
                Love.Graphics.Print(title, baseLine.X, baseLine.Y);
                //foreach (var rune in title.EnumerateRunes())
                //{
                //    if (!font.TryGetCharMetrics(rune, UIScale, out var metrics))
                //    {
                //        continue;
                //    }

                //    font.DrawChar(handle, rune, baseLine, UIScale, active ? fontColorActive : fontColorInactive);
                //    baseLine += new Vector2(metrics.Advance, 0);
                //}

                headerOffset += boxAdvance;
            }
        }

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            var headerSize = Vector2.Zero;

            if (TabsVisible)
            {
                headerSize = (0, _getHeaderSize() / UIScale);
            }

            var panel = _getPanel();
            var panelSize = (panel?.MinimumSize ?? Vector2.Zero) / UIScale;

            var contentsSize = availableSize - headerSize - panelSize;

            var total = Vector2.Zero;
            foreach (var child in WispChildren)
            {
                if (child.Visible)
                {
                    child.Measure(contentsSize);
                    total = Vector2.ComponentMax(child.DesiredSize, total);
                }
            }

            return total + headerSize + panelSize;
        }

        protected override Vector2 ArrangeOverride(Vector2 finalSize)
        {
            if (ChildCount == 0 || _currentTab >= ChildCount)
            {
                return finalSize;
            }

            var headerSize = _getHeaderSize();
            var panel = _getPanel();
            var contentBox = new UIBox2i(0, headerSize, (int)(finalSize.X * UIScale), (int)(finalSize.Y * UIScale));
            if (panel != null)
            {
                contentBox = (UIBox2i)panel.GetContentBox(contentBox);
            }

            var control = GetWispChild(_currentTab);
            control.Visible = true;
            control.ArrangePixel(contentBox);
            return finalSize;
        }

        protected internal override void KeyBindDown(GUIBoundKeyEventArgs args)
        {
            base.KeyBindDown(args);

            if (!TabsVisible || args.Function != EngineKeyFunctions.UIClick)
            {
                return;
            }

            // Outside of header size, ignore.
            if (args.RelativePixelPosition.Y < 0 || args.RelativePixelPosition.Y > _getHeaderSize())
            {
                return;
            }

            args.Handle();

            var relX = args.RelativePixelPosition.X;

            var font = _getFont();
            var boxActive = _getTabBoxActive();
            var boxInactive = _getTabBoxInactive();

            var headerOffset = 0f;

            for (var i = 0; i < ChildCount; i++)
            {
                if (!GetTabVisible(i))
                {
                    continue;
                }

                var title = GetActualTabTitle(i);

                var titleLength = font.LoveFont.GetWidth(title);
                //// Get string length.
                //foreach (var rune in title.EnumerateRunes())
                //{
                //    if (!font.TryGetCharMetrics(rune, UIScale, out var metrics))
                //    {
                //        continue;
                //    }

                //    titleLength += metrics.Advance;
                //}

                var active = _currentTab == i;
                var box = active ? boxActive : boxInactive;
                var boxAdvance = titleLength + box?.MinimumSize.X ?? 0;

                if (headerOffset < relX && headerOffset + boxAdvance > relX)
                {
                    // Got em.
                    CurrentTab = i;
                    return;
                }

                headerOffset += boxAdvance;
            }
        }

        [System.Diagnostics.Contracts.Pure]
        private int _getHeaderSize()
        {
            var headerSize = 0;

            if (TabsVisible)
            {
                var active = _getTabBoxActive();
                var inactive = _getTabBoxInactive();
                var font = _getFont();

                var activeSize = active?.MinimumSize ?? Vector2.Zero;
                var inactiveSize = inactive?.MinimumSize ?? Vector2.Zero;

                headerSize = (int)MathF.Max(activeSize.Y, inactiveSize.Y);
                headerSize += (int)font.LoveFont.GetHeightV(UIScale);
            }

            return headerSize;
        }

        [System.Diagnostics.Contracts.Pure]
        private StyleBox? _getTabBoxActive()
        {
            TryGetStyleProperty<StyleBox>(StylePropertyTabStyleBox, out var box);
            return box;
        }

        [System.Diagnostics.Contracts.Pure]
        private StyleBox? _getTabBoxInactive()
        {
            TryGetStyleProperty<StyleBox>(StylePropertyTabStyleBoxInactive, out var box);
            return box;
        }

        [System.Diagnostics.Contracts.Pure]
        private Color _getTabFontColorActive()
        {
            if (TryGetStyleProperty(stylePropertyTabFontColor, out Color color))
            {
                return color;
            }
            return Color.White;
        }

        [System.Diagnostics.Contracts.Pure]
        private Color _getTabFontColorInactive()
        {
            if (TryGetStyleProperty(StylePropertyTabFontColorInactive, out Color color))
            {
                return color;
            }
            return Color.Gray;
        }

        [System.Diagnostics.Contracts.Pure]
        private StyleBox? _getPanel()
        {
            TryGetStyleProperty<StyleBox>(StylePropertyPanelStyleBox, out var box);
            return box;
        }

        [System.Diagnostics.Contracts.Pure]
        private FontSpec _getFont()
        {
            if (TryGetStyleProperty<FontSpec>(StylePropertyFont, out var font))
            {
                return font;
            }

            return WispManager.GetStyleFallback<FontSpec>();
        }
    }
}
