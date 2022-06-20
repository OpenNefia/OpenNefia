using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.Input;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Wisp.Drawing;
using OpenNefia.Core.UserInterface;
using Timer = OpenNefia.Core.Timing.Timer;

namespace OpenNefia.Core.UI.Wisp.Controls
{
    public class ItemList : WispControl, IList<ItemList.Item>
    {
        private bool _isAtBottom = true;
        private int _totalContentHeight;

        private VScrollBar _scrollBar;
        private readonly List<Item> _itemList = new();
        public event Action<ItemListSelectedEventArgs>? OnItemSelected;
        public event Action<ItemListDeselectedEventArgs>? OnItemDeselected;
        public event Action<ItemListHoverEventArgs>? OnItemHover;

        public const string StylePropertyBackground = "itemlistBackground";
        public const string StylePropertyItemBackground = "itemBackground";
        public const string StylePropertySelectedItemBackground = "selectedItemBackground";
        public const string StylePropertyDisabledItemBackground = "disabledItemBackground";

        public int Count => _itemList.Count;
        public bool IsReadOnly => false;

        public bool ScrollFollowing { get; set; } = false;
        public int ButtonDeselectDelay { get; set; } = 100;

        public ItemListSelectMode SelectMode { get; set; } = ItemListSelectMode.Single;

        public ItemList()
        {
            EventFilter = UIEventFilterMode.Pass;

            RectClipContent = true;

            _scrollBar = new VScrollBar
            {
                Name = "_v_scroll",

                HorizontalAlignment = HAlignment.Right
            };
            AddChild(_scrollBar);
            _scrollBar.OnValueChanged += _ => _isAtBottom = _scrollBar.IsAtEnd;
        }

        private void RecalculateContentHeight()
        {
            _totalContentHeight = 0;
            foreach (var item in _itemList)
            {
                var itemHeight = 0f;
                if (item.Icon != null)
                {
                    itemHeight = item.IconSize.Y;
                }

                itemHeight = Math.Max(itemHeight, ActualFont.LoveFont.GetHeight());
                itemHeight += ActualItemBackground.MinimumSize.Y;

                _totalContentHeight += (int)Math.Ceiling(itemHeight);
            }

            _scrollBar.MaxValue = Math.Max(_scrollBar.Page, _totalContentHeight);
            _updateScrollbarVisibility();
        }

        public void Add(Item item)
        {
            if (item == null) return;
            if (item.Owner != this) throw new ArgumentException("Item is owned by another ItemList!");

            _itemList.Add(item);

            item.OnSelected += Select;
            item.OnDeselected += Deselect;

            RecalculateContentHeight();
            if (_isAtBottom && ScrollFollowing)
                _scrollBar.MoveToEnd();
        }

        public Item AddItem(string text, IAssetInstance? icon = null, bool selectable = true)
        {
            var item = new Item(this) { Text = text, Icon = icon, Selectable = selectable };
            Add(item);
            return item;
        }

        public void Clear()
        {
            foreach (var item in _itemList.ToArray())
            {
                Remove(item);
            }

            _totalContentHeight = 0;
        }

        public bool Contains(Item item)
        {
            return _itemList.Contains(item);
        }

        public void CopyTo(Item[] array, int arrayIndex)
        {
            _itemList.CopyTo(array, arrayIndex);
        }

        public bool Remove(Item item)
        {
            if (item == null) return false;

            var value = _itemList.Remove(item);

            item.OnSelected -= Select;
            item.OnDeselected -= Deselect;

            RecalculateContentHeight();
            if (_isAtBottom && ScrollFollowing)
                _scrollBar.MoveToEnd();

            return value;
        }

        public void RemoveAt(int index)
        {
            Remove(this[index]);
        }

        public IEnumerator<Item> GetEnumerator()
        {
            return _itemList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(Item item)
        {
            return _itemList.IndexOf(item);
        }

        public void Insert(int index, Item item)
        {
            if (item == null) return;
            if (item.Owner != this) throw new ArgumentException("Item is owned by another ItemList!");

            _itemList.Insert(index, item);

            item.OnSelected += Select;
            item.OnDeselected += Deselect;

            RecalculateContentHeight();
            if (_isAtBottom && ScrollFollowing)
                _scrollBar.MoveToEnd();
        }

        // Without this attribute, this would compile into a property called "Item", causing problems with the Item class.
        [System.Runtime.CompilerServices.IndexerName("IndexItem")]
        public Item this[int index]
        {
            get => _itemList[index];
            set => _itemList[index] = value;
        }

        public IEnumerable<Item> GetSelected()
        {
            var list = new List<Item>();

            for (var i = 0; i < _itemList.Count; i++)
            {
                var item = _itemList[i];
                if (item.Selected) list.Add(item);
            }

            return list;
        }

        private void Select(int idx)
        {
            if (SelectMode != ItemListSelectMode.Multiple)
                ClearSelected(idx);
            OnItemSelected?.Invoke(new ItemListSelectedEventArgs(idx, this));
        }

        private void Select(Item item)
        {
            var idx = IndexOf(item);
            if (idx != -1)
                Select(idx);
        }

        private void Deselect(int idx)
        {
            OnItemDeselected?.Invoke(new ItemListDeselectedEventArgs(idx, this));
        }

        private void Deselect(Item item)
        {
            var idx = IndexOf(item);
            if (idx == -1) return;
            Deselect(idx);
        }

        public void ClearSelected(int? except = null)
        {
            foreach (var item in GetSelected())
            {
                if (IndexOf(item) == except) continue;
                item.Selected = false;
            }
        }

        public void SortItemsByText() => Sort((p, q) => string.Compare(p.Text, q.Text, StringComparison.Ordinal));

        public void Sort(Comparison<Item> comparison) => _itemList.Sort(comparison);


        public void EnsureCurrentIsVisible()
        {
            // TODO: Implement this.
        }

        public int GetItemAtPosition(Vector2 position, bool exact = false)
        {
            throw new NotImplementedException();
        }

        public FontSpec ActualFont
        {
            get
            {
                if (TryGetStyleProperty<FontSpec>(Label.StylePropertyFont, out var font))
                {
                    return font;
                }

                return WispManager.GetStyleFallback<FontSpec>();
            }
        }

        public Color ActualFontColor
        {
            get
            {
                if (TryGetStyleProperty(Label.StylePropertyFontColor, out Color fontColor))
                {
                    return fontColor;
                }

                return Color.White;
            }
        }

        public StyleBox ActualBackground
        {
            get
            {
                if (TryGetStyleProperty<StyleBox>(StylePropertyBackground, out var bg))
                {
                    return bg;
                }

                return new StyleBoxFlat();
            }
        }
        public StyleBox ActualItemBackground
        {
            get
            {
                if (TryGetStyleProperty<StyleBox>(StylePropertyItemBackground, out var bg))
                {
                    return bg;
                }

                return new StyleBoxFlat();
            }
        }

        public StyleBox ActualSelectedItemBackground
        {
            get
            {
                if (TryGetStyleProperty<StyleBox>(StylePropertySelectedItemBackground, out var bg))
                {
                    return bg;
                }

                return new StyleBoxFlat();
            }
        }

        public StyleBox ActualDisabledItemBackground
        {
            get
            {
                if (TryGetStyleProperty<StyleBox>(StylePropertyDisabledItemBackground, out var bg))
                {
                    return bg;
                }

                return new StyleBoxFlat();
            }
        }

        public void ScrollToBottom()
        {
            _scrollBar.MoveToEnd();
            _isAtBottom = true;
        }

        public override void Draw()
        {
            base.Draw();

            var sizeBox = PixelSizeBox;

            var font = ActualFont;
            var listBg = ActualBackground;
            var iconBg = ActualItemBackground;
            var iconSelectedBg = ActualSelectedItemBackground;
            var iconDisabledBg = ActualDisabledItemBackground;

            var offset = -_scrollBar.Value;

            listBg.Draw(GlobalPixelRect, WispRootLayer!.GlobalTint);

            foreach (var item in _itemList)
            {
                var bg = iconBg;

                if (item.Disabled)
                    bg = iconDisabledBg;

                if (item.Selected)
                {
                    bg = iconSelectedBg;
                }

                var itemHeight = 0f;
                if (item.Icon != null)
                {
                    itemHeight = item.IconSize.Y;
                }

                itemHeight = Math.Max(itemHeight, font.LoveFont.GetHeight());
                itemHeight += ActualItemBackground.MinimumSize.Y;

                var region = UIBox2.FromDimensions(PixelX, PixelY + offset, PixelWidth, itemHeight);
                item.Region = region;

                if (region.Intersects(sizeBox))
                {
                    bg.Draw(item.Region.Value.Translated(GlobalPixelPosition), WispRootLayer!.GlobalTint);

                    var contentBox = bg.GetContentBox(item.Region.Value);
                    var drawOffset = contentBox.TopLeft;
                    if (item.Icon != null)
                    {
                        GraphicsS.SetColorTinted(this, item.IconModulate);
                        var drawLocation = UIBox2.FromDimensions(GlobalPixelPosition + drawOffset, item.Icon.PixelSize);
                        if (item.IconRegion.Size == Vector2.Zero)
                        {
                            item.Icon.DrawUnscaled(drawLocation);
                        }
                        else
                        {
                            item.Icon.DrawRegionUnscaled(item.IconRegion, drawLocation);
                        }
                    }

                    if (item.Text != null)
                    {
                        if (item.BakedText == null)
                            item.BakedText = Love.Graphics.NewText(ActualFont.LoveFont, item.Text);

                        var textBox = new UIBox2(contentBox.Left + item.IconSize.X, contentBox.Top, contentBox.Right,
                            contentBox.Bottom);
                        DrawTextInternal(item.BakedText, textBox);
                    }
                }

                offset += itemHeight;
            }
        }

        protected void DrawTextInternal(Love.Text text, UIBox2 box)
        {
            var font = ActualFont;

            var color = ActualFontColor;
            var offsetY = (int)(box.Height - font.LoveFont.GetHeight()) / 2 - 1;
            var baseLine = GlobalPixelPosition + new Vector2i(0, offsetY) + box.TopLeft;

            GraphicsS.SetColorTinted(this, color);
            WispRootLayer!.PushScissor(box.Translated(GlobalPixelPosition), ignoreParents: true);
            Love.Graphics.Draw(text, baseLine.X, baseLine.Y);
            WispRootLayer.PopScissor();
        }

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            var size = Vector2.Zero;
            if (ActualBackground != null)
            {
                size += ActualBackground.MinimumSize / UIScale;
            }

            return size;
        }

        protected internal override void KeyBindDown(GUIBoundKeyEventArgs args)
        {
            base.KeyBindDown(args);

            if (SelectMode == ItemListSelectMode.None || args.Function != EngineKeyFunctions.UIClick)
            {
                return;
            }

            foreach (var item in _itemList)
            {
                if (item.Region == null)
                    continue;

                if (!item.Region.Value.Contains(args.RelativePixelPosition))
                    continue;

                if (item.Selectable && !item.Disabled)
                {
                    if (item.Selected && SelectMode != ItemListSelectMode.Button)
                    {
                        ClearSelected();
                        item.Selected = false;
                        return;
                    }

                    item.Selected = true;
                    if (SelectMode == ItemListSelectMode.Button)
                        Timer.Spawn(ButtonDeselectDelay, () => { item.Selected = false; });
                }
                break;
            }
        }

        protected internal override void MouseMove(GUIMouseMoveEventArgs args)
        {
            base.MouseMove(args);

            for (var idx = 0; idx < _itemList.Count; idx++)
            {
                var item = _itemList[idx];
                if (item.Region == null) continue;
                if (!item.Region.Value.Contains(args.RelativePosition)) continue;
                OnItemHover?.Invoke(new ItemListHoverEventArgs(idx, this));
                break;
            }
        }

        protected internal override void MouseWheel(GUIMouseWheelEventArgs args)
        {
            base.MouseWheel(args);

            if (MathHelper.CloseToPercent(0, args.Delta.Y))
            {
                return;
            }

            _scrollBar.ValueTarget -= _getScrollSpeed() * args.Delta.Y;
            _isAtBottom = _scrollBar.IsAtEnd;

            args.Handle();
        }

        [Pure]
        private int _getScrollSpeed()
        {
            var font = ActualFont;
            return font.LoveFont.GetHeight() * 2;
        }

        [Pure]
        private UIBox2 _getContentBox()
        {
            var style = ActualBackground;
            return style?.GetContentBox(SizeBox) ?? SizeBox;
        }

        protected override void Resized()
        {
            base.Resized();

            var styleBoxSize = ActualBackground?.MinimumSize.Y ?? 0;

            _scrollBar.Page = PixelSize.Y - styleBoxSize;
            RecalculateContentHeight();
        }

        protected internal override void UIScaleChanged(GUIScaleChangedEventArgs args)
        {
            RecalculateContentHeight();

            base.UIScaleChanged(args);
        }

        private void _updateScrollbarVisibility()
        {
            _scrollBar.Visible = _totalContentHeight + ActualBackground.MinimumSize.Y > PixelHeight;
        }

        public abstract class ItemListEventArgs : EventArgs
        {
            /// <summary>
            ///     The ItemList this event originated from.
            /// </summary>
            public ItemList ItemList { get; }

            protected ItemListEventArgs(ItemList list)
            {
                ItemList = list;
            }
        }

        public sealed class ItemListSelectedEventArgs : ItemListEventArgs
        {
            /// <summary>
            ///     The index of the item that was selected.
            /// </summary>
            public int ItemIndex;

            public ItemListSelectedEventArgs(int itemIndex, ItemList list) : base(list)
            {
                ItemIndex = itemIndex;
            }
        }

        public sealed class ItemListDeselectedEventArgs : ItemListEventArgs
        {
            /// <summary>
            ///     The index of the item that was selected.
            /// </summary>
            public int ItemIndex;

            public ItemListDeselectedEventArgs(int itemIndex, ItemList list) : base(list)
            {
                ItemIndex = itemIndex;
            }
        }

        public sealed class ItemListHoverEventArgs : ItemListEventArgs
        {
            /// <summary>
            ///     The index of the item that was selected.
            /// </summary>
            public int ItemIndex;

            public ItemListHoverEventArgs(int itemIndex, ItemList list) : base(list)
            {
                ItemIndex = itemIndex;
            }
        }

        public enum ItemListSelectMode : byte
        {
            None,
            Single,
            Multiple,
            Button,
        }

        public sealed class Item
        {
            public event Action<Item>? OnSelected;
            public event Action<Item>? OnDeselected;

            private bool _selected = false;
            private bool _disabled = false;

            public ItemList Owner { get; }
            private string? _text;
            public string? Text
            {
                get => _text;
                set
                {
                    _text = value;
                    BakedText?.Dispose();
                    BakedText = null;
                }
            }
            public Love.Text? BakedText { get; set; }
            public string? TooltipText { get; set; }
            public IAssetInstance? Icon { get; set; }
            public UIBox2 IconRegion { get; set; }
            public Color IconModulate { get; set; } = Color.White;
            public bool Selectable { get; set; } = true;
            public bool TooltipEnabled { get; set; } = true;
            public UIBox2? Region { get; set; }
            public object? Metadata { get; set; }

            public bool Disabled
            {
                get => _disabled;
                set
                {
                    _disabled = value;
                    if (Selected) Selected = false;
                }
            }
            public bool Selected
            {
                get => _selected;
                set
                {
                    if (!Selectable) return;
                    _selected = value;
                    if (_selected) OnSelected?.Invoke(this);
                    else OnDeselected?.Invoke(this);
                }
            }

            public Vector2 IconSize
            {
                get
                {
                    if (Icon == null)
                        return Vector2.Zero;
                    return IconRegion.Size != Vector2.Zero ? IconRegion.Size : Icon.PixelSize;
                }
            }

            public Item(ItemList owner)
            {
                Owner = owner;
            }
        }
    }
}
