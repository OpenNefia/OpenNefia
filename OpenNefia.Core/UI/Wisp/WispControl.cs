using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.UserInterface.XAML;
using OpenNefia.Core.Utility;
using OpenNefia.Core.ViewVariables;
using static NetVips.Enums;

namespace OpenNefia.Core.UI.Wisp
{
    /// <summary>
    /// <para>
    /// "Wisp" is the codename for the subset of OpenNefia's UI that supports
    /// automatic layouting (and in the future, XAML layouting and styling). It is so
    /// named because of its intended usage in floating layout managers/debug contexts,
    /// in the style of ImGui.
    /// </para>
    /// <para>
    /// It implements the rest of Robust's <c>Control</c> class that <see cref="Element.UiElement"/>
    /// does not implement yet. Historically, this is because OpenNefia's original UI
    /// was ported directly from its previous prototype, so several changes to Robust's
    /// <c>Control</c> were made for compatibility.
    /// </para>
    /// <para>
    /// In the future, <see cref="WispControl"/> and <see cref="Element.UiElement"/> should be merged 
    /// into one single <c>Control</c> class, and every existing UI control/layer that uses 
    /// <see cref="Element.UiElement"/> should be ported over to use this class, along with XAML layouting.
    /// </para>
    /// </summary>
    public partial class WispControl : UiElement
    {
        internal Vector2? PreviousMeasure;
        internal UIBox2? PreviousArrange;

        private float _sizeFlagsStretchRatio = 1f;

        private bool _horizontalExpand;
        private bool _verticalExpand;
        private HAlignment _horizontalAlignment = HAlignment.Stretch;
        private VAlignment _verticalAlignment = VAlignment.Stretch;
        private Thickness _margin;
        private bool _measuring;

        protected IWispManager WispManager { get; }

        public WispControl() : base()
        {
            CanControlFocus = true;
            WispManager = IoCManager.Resolve<IWispManager>();     
            StyleClasses = new StyleClassCollection(this);
            XamlChildren = Children;
        }

        /// <summary>
        ///     The name of this control.
        ///     Names must be unique between the siblings of the control.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The desired minimum size this control needs for layout to avoid cutting off content or such.
        /// </summary>
        /// <remarks>
        /// This is calculated by calling <see cref="Measure"/>.
        /// </remarks>
        public Vector2 DesiredSize { get; private set; }
        public Vector2i DesiredPixelSize => (Vector2i)(DesiredSize * UIScale);

        public override Vector2 MinSize
        {
            get => base.MinSize;
            set
            {
                base.MinSize = value;
                InvalidateMeasure();
            }
        }

        public override Vector2 ExactSize
        {
            get => base.ExactSize;
            set
            {
                base.ExactSize = value;
                InvalidateMeasure();
            }
        }

        public override Vector2 MaxSize
        {
            get => base.MaxSize; 
            set
            {
                base.MaxSize = value;
                InvalidateMeasure();
            }
        }

        public bool IsMeasureValid { get; private set; }
        public bool IsArrangeValid { get; private set; }

        /// <summary>
        /// Margin of this control.
        /// </summary>
        public Thickness Margin
        {
            get => _margin;
            set
            {
                _margin = value;
                InvalidateMeasure();
            }
        }

        public IWispLayer? WispRootLayer => Root as IWispLayer;

        public WispControl? WispParent => Parent as WispControl;

        public IEnumerable<WispControl> WispChildren => Children.WhereAssignable<UiElement, WispControl>();
        public int WispChildCount => WispChildren.Count();

        // TODO remove
        public WispControl GetWispChild(int index)
        {
            return (WispControl)GetChild(index);
        }

        [Content]
        public virtual ICollection<UiElement> XamlChildren { get; protected set; }

        /// <summary>
        /// Full name of the class to bind this control to. Set by XAML.
        /// If this is non-null, the class is expected to be declared with
        /// <c>partial</c>, and a corresponding .xaml file sharing the same
        /// basename should exist in the same directory as the class's file.
        /// </summary>
        public string? Class { get; set; }

        /// <summary>
        ///     Gets whether this control is at all visible.
        ///     This means the control is part of the tree of the root control, and all of its parents are visible.
        /// </summary>
        /// <seealso cref="Visible"/>
        [ViewVariables]
        public bool VisibleInTree
        {
            get
            {
                for (var parent = this; parent != null; parent = parent.WispParent)
                {
                    if (!parent.Visible)
                    {
                        return false;
                    }

                    if (parent is WispRoot)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public event Action<WispControl>? OnVisibilityChanged;

        /// <summary>
        ///     Whether or not this control and its children are visible.
        /// </summary>
        public override bool Visible
        {
            get => base.Visible;
            set
            {
                if (base.Visible == value)
                    return;

                base.Visible = value;

                _propagateVisibilityChanged(value);
                // TODO: unhardcode this.
                // Many containers ignore children if they're invisible, so that's why we're replicating that ehre.
                WispParent?.InvalidateMeasure();
                InvalidateMeasure();
            }
        }

        private void _propagateVisibilityChanged(bool newVisible)
        {
            OnVisibilityChanged?.Invoke(this);
            if (!VisibleInTree)
            {
                UserInterfaceManagerInternal.ControlHidden(this);
            }

            foreach (var child in WispChildren)
            {
                if (newVisible || child.Visible)
                {
                    child._propagateVisibilityChanged(newVisible);
                }
            }
        }

        // TODO cache instead of recalculating; LOVE2D forces us to use this
        // coordinate system. Or implement a screen-relative drawing handle.
        public override Vector2 GlobalPosition
        {
            get
            {
                var offset = Position;
                var parent = Parent;
                while (parent != null)
                {
                    offset += parent.Position;
                    parent = parent.Parent;
                }

                return offset;
            }
        }

        public float GlobalX => GlobalPosition.X;
        public float GlobalY => GlobalPosition.Y;

        public override Vector2i GlobalPixelPosition
        {
            get
            {
                var offset = PixelPosition;
                var parent = Parent;
                while (parent != null)
                {
                    offset += parent.PixelPosition;
                    parent = parent.Parent;
                }

                return offset;
            }
        }

        public int GlobalPixelX => GlobalPixelPosition.X;
        public int GlobalPixelY => GlobalPixelPosition.Y;

        /// <summary>
        ///     Called to test whether this control has a certain point,
        ///     for the purposes of finding controls under the cursor.
        /// </summary>
        /// <param name="point">The relative point, in virtual pixels.</param>
        /// <returns>True if this control does have the point and should be counted as a hit.</returns>
        public override bool ContainsPoint(Vector2 point)
        {
            var size = Size;
            return point.X >= 0 && point.X <= size.X && point.Y >= 0 && point.Y <= size.Y;
        }

        /// <summary>
        /// Horizontal alignment mode.
        /// This determines how the control should be laid out horizontally
        /// if it gets more available space than its <see cref="DesiredSize"/>.
        /// </summary>
        public HAlignment HorizontalAlignment
        {
            get => _horizontalAlignment;
            set
            {
                _horizontalAlignment = value;
                InvalidateArrange();
            }
        }

        /// <summary>
        /// Vertical alignment mode.
        /// This determines how the control should be laid out vertically
        /// if it gets more available space than its <see cref="DesiredSize"/>.
        /// </summary>
        public VAlignment VerticalAlignment
        {
            get => _verticalAlignment;
            set
            {
                _verticalAlignment = value;
                InvalidateArrange();
            }
        }

        /// <summary>
        /// Whether to horizontally expand and push other controls in layout controls that support this.
        /// This does nothing unless the parent is a control like <see cref="BoxContainer"/> which supports this behavior.
        /// </summary>
        /// <remarks>
        /// If I was redesigning the UI system from scratch today, this would be an attached property instead.
        /// </remarks>
        public bool HorizontalExpand
        {
            get => _horizontalExpand;
            set
            {
                _horizontalExpand = value;
                if (Parent is WispControl parentCtrl)
                    parentCtrl.InvalidateMeasure();
            }
        }

        /// <summary>
        /// Whether to vertically expand and push other controls in layout controls that support this.
        /// This does nothing unless the parent is a control like <see cref="BoxContainer"/> which supports this behavior.
        /// </summary>
        /// <remarks>
        /// If I was redesigning the UI system from scratch today, this would be an attached property instead.
        /// </remarks>
        public bool VerticalExpand
        {
            get => _verticalExpand;
            set
            {
                _verticalExpand = value;
                if (Parent is WispControl parentCtrl)
                    parentCtrl.InvalidateMeasure();
            }
        }

        /// <summary>
        ///     Stretch ratio used to give shared of the available space in case multiple siblings are set to expand
        ///     in a container
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if the value is less than or equal to 0.
        /// </exception>
        public float SizeFlagsStretchRatio
        {
            get => _sizeFlagsStretchRatio;
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be greater than zero.");
                }

                _sizeFlagsStretchRatio = value;

                if (Parent is WispControl wispParent)
                    wispParent.InvalidateArrange();
            }
        }

        /// <summary>
        ///     Whether to clip drawing of this control and its children to its rectangle.
        /// </summary>
        /// <remarks>
        ///     By default, controls (and their children) can render outside their rectangle.
        ///     If this is set, rendering is hard clipped to it.
        /// </remarks>
        /// <seealso cref="RectDrawClipMargin"/>
        public bool RectClipContent { get; set; }

        // TODO: remove GetPreferredSize
        public sealed override void GetPreferredSize(out Vector2 size)
        {
            size = DesiredSize;
        }

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
        }

        /// <inheritdoc/>
        protected internal override void UIScaleChanged(GUIScaleChangedEventArgs args)
        {
            base.UIScaleChanged(args);
            InvalidateMeasure();
        }

        protected override void ChildAdded(UiElement newChild)
        {
            base.ChildAdded(newChild);
            InvalidateMeasure();
        }

        protected override void Parented(UiElement newParent)
        {
            base.Parented(newParent);
            StylesheetUpdateRecursive();
            InvalidateMeasure();
        }

        protected override void ChildRemoved(UiElement child)
        {
            base.ChildRemoved(child);
            InvalidateMeasure();
        }

        /// <summary>
        /// Notify the layout system that this control's <see cref="Measure"/> result may have changed
        /// and must be recalculated.
        /// </summary>
        public void InvalidateMeasure()
        {
            if (!IsMeasureValid)
                return;

            IsMeasureValid = false;
            IsArrangeValid = false;

            WispManager.QueueMeasureUpdate(this);
        }

        /// <summary>
        /// Notify the layout system that this control's <see cref="Arrange"/> result may have changed
        /// and must be recalculated.
        /// </summary>
        public void InvalidateArrange()
        {
            if (!IsArrangeValid)
            {
                // Already queued for a layout update, don't bother.
                return;
            }

            IsArrangeValid = false;
            WispManager.QueueArrangeUpdate(this);
        }

        /// <summary>
        /// Measure the desired size of this control, if given a specific available space.
        /// The result of this measure is stored in <see cref="DesiredSize"/>.
        /// </summary>
        /// <remarks>
        /// Available size is given to this method so that controls can handle special cases such as text layout,
        /// where word wrapping can cause the vertical size to change based on available horizontal size.
        /// </remarks>
        /// <param name="availableSize">The space available to this control, that it should measure for.</param>
        public void Measure(Vector2 availableSize)
        {
            if (!IsMeasureValid || PreviousMeasure != availableSize)
            {
                IsMeasureValid = true;
                var desired = MeasureCore(availableSize);

                if (desired.X < 0 || desired.Y < 0 || !float.IsFinite(desired.X) || !float.IsFinite(desired.Y))
                    throw new InvalidOperationException("Invalid size returned from Measure()");

                var prev = DesiredSize;
                DesiredSize = desired;
                PreviousMeasure = availableSize;

                if (prev != desired && Parent is WispControl parentCtrl && !parentCtrl._measuring)
                    parentCtrl.InvalidateMeasure();
            }
        }

        /// <summary>
        /// Core logic implementation of <see cref="Measure"/>,
        /// implementing stuff such as margins and <see cref="MinSize"/>.
        /// In almost all cases, you want to override <see cref="MeasureOverride"/> instead, which is called by this.
        /// </summary>
        /// <returns>The actual measured desired size of the control.</returns>
        protected virtual Vector2 MeasureCore(Vector2 availableSize)
        {
            if (!Visible)
                return default;
            if (_stylingDirty)
               ForceRunStyleUpdate();

            var withoutMargin = _margin.Deflate(availableSize);

            var constrained = ApplySizeConstraints(this, withoutMargin);

            Vector2 measured;
            try
            {
                _measuring = true;
                measured = MeasureOverride(constrained);
            }
            finally
            {
                _measuring = false;
            }

            if (!float.IsNaN(PreferredWidth))
            {
                measured.X = PreferredWidth;
            }

            measured.X = Math.Clamp(measured.X, MinWidth, MaxWidth);

            if (!float.IsNaN(PreferredHeight))
            {
                measured.Y = PreferredHeight;
            }

            measured.Y = Math.Clamp(measured.Y, MinHeight, MaxHeight);

            measured = _margin.Inflate(measured);
            measured = Vector2.ComponentMin(measured, availableSize);
            measured = Vector2.ComponentMax(measured, Vector2.Zero);
            return measured;
        }

        /// <summary>
        /// Calculates the actual desired size for the contents of this control, based on available size.
        /// </summary>
        protected virtual Vector2 MeasureOverride(Vector2 availableSize)
        {
            var min = Vector2.Zero;

            foreach (var child in WispChildren)
            {
                child.Measure(availableSize);
                min = Vector2.ComponentMax(min, child.DesiredSize);
            }

            return min;
        }

        /// <summary>
        /// Lay out this control in the given space of its parent, by pixel coordinates.
        /// </summary>
        public void ArrangePixel(UIBox2i finalRect)
        {
            var topLeft = finalRect.TopLeft / UIScale;
            var bottomRight = finalRect.BottomRight / UIScale;

            Arrange(new UIBox2(topLeft, bottomRight));
        }

        /// <summary>
        /// Lay out this control in the given space of its parent.
        /// This sets <see cref="RelativePosition"/> and <see cref="Size"/> and also arranges any child controls.
        /// </summary>
        public void Arrange(UIBox2 finalRect)
        {
            if (!IsMeasureValid)
                Measure(PreviousMeasure ?? finalRect.Size);

            if (!IsArrangeValid || PreviousArrange != finalRect)
            {
                IsArrangeValid = true;
                ArrangeCore(finalRect);
                PreviousArrange = finalRect;
            }
        }

        /// <summary>
        /// Core logic implementation of <see cref="Arrange"/>,
        /// implementing stuff such as margins and <see cref="MinSize"/>.
        /// In almost all cases, you want to override <see cref="ArrangeOverride"/> instead, which is called by this.
        /// </summary>
        protected virtual void ArrangeCore(UIBox2 finalRect)
        {
            if (!Visible)
                return;

            var withoutMargins = _margin.Deflate(finalRect);
            var availWithoutMargins = withoutMargins.Size;
            var size = availWithoutMargins;
            var origin = withoutMargins.TopLeft;

            if (_horizontalAlignment != HAlignment.Stretch)
                size.X = Math.Min(size.X, DesiredSize.X - _margin.SumHorizontal);

            if (_verticalAlignment != VAlignment.Stretch)
                size.Y = Math.Min(size.Y, DesiredSize.Y - _margin.SumVertical);


            size = ApplySizeConstraints(this, size);

            var arranged = ArrangeOverride(size);

            size = Vector2.ComponentMin(arranged, size);

            switch (HorizontalAlignment)
            {
                case HAlignment.Stretch:
                case HAlignment.Center:
                    origin.X += (availWithoutMargins.X - size.X) / 2;
                    break;
                case HAlignment.Right:
                    origin.X += availWithoutMargins.X - size.X;
                    break;
            }

            switch (VerticalAlignment)
            {
                case VAlignment.Stretch:
                case VAlignment.Center:
                    origin.Y += (availWithoutMargins.Y - size.Y) / 2;
                    break;
                case VAlignment.Bottom:
                    origin.Y += availWithoutMargins.Y - size.Y;
                    break;
            }

            Position = origin;
            Size = size;
        }

        /// <summary>
        /// Lay out this control and its children for the specified final size.
        /// </summary>
        /// <param name="finalSize">
        /// The final size for this control,
        /// after calculation of things like margins and alignment.
        /// </param>
        /// <returns>The actual space used by this control.</returns>
        protected virtual Vector2 ArrangeOverride(Vector2 finalSize)
        {
            foreach (var child in Children.WhereAssignable<UiElement, WispControl>())
            {
                child.Arrange(UIBox2.FromDimensions(Vector2.Zero, finalSize));
            }

            return finalSize;
        }

        private static Vector2 ApplySizeConstraints(WispControl control, Vector2 avail)
        {
            var minW = control.MinWidth;
            var setW = control.PreferredWidth;
            var maxW = control.MaxWidth;

            var maxConstraint = float.IsNaN(setW) ? float.PositiveInfinity : setW;
            maxW = MathHelper.Clamp(maxConstraint, minW, maxW);

            var minConstraint = float.IsNaN(setW) ? 0 : setW;
            minW = MathHelper.Clamp(maxW, minConstraint, minW);

            var minH = control.MinHeight;
            var setH = control.PreferredHeight;
            var maxH = control.MaxHeight;

            maxConstraint = float.IsNaN(setH) ? float.PositiveInfinity : setH;
            maxH = MathHelper.Clamp(maxConstraint, minH, maxH);

            minConstraint = float.IsNaN(setH) ? 0 : setH;
            minH = MathHelper.Clamp(maxH, minConstraint, minH);

            return (
                Math.Clamp(avail.X, minW, maxW),
                Math.Clamp(avail.Y, minH, maxH));
        }

        /// <summary>
        /// Specifies horizontal alignment modes.
        /// </summary>
        /// <seealso cref="HorizontalAlignment"/>
        public enum HAlignment
        {
            /// <summary>
            /// The control should take up all available horizontal space.
            /// </summary>
            Stretch,

            /// <summary>
            /// The control should take up minimal (<see cref="DesiredSize"/>) space and align to the left of its given space.
            /// </summary>
            Left,

            /// <summary>
            /// The control should take up minimal (<see cref="DesiredSize"/>) space and align in the center of its given space.
            /// </summary>
            Center,

            /// <summary>
            /// The control should take up minimal (<see cref="DesiredSize"/>) space and align to the right of its given space.
            /// </summary>
            Right
        }

        /// <summary>
        /// Specifies vertical alignment modes.
        /// </summary>
        /// <seealso cref="VerticalAlignment"/>
        public enum VAlignment
        {
            /// <summary>
            /// The control should take up all available vertical space.
            /// </summary>
            Stretch,

            /// <summary>
            /// The control should take up minimal (<see cref="DesiredSize"/>) space and align to the top of its given space.
            /// </summary>
            Top,

            /// <summary>
            /// The control should take up minimal (<see cref="DesiredSize"/>) space and align in the center of its given space.
            /// </summary>
            Center,

            /// <summary>
            /// The control should take up minimal (<see cref="DesiredSize"/>) space and align to the bottom of its given space.
            /// </summary>
            Bottom
        }
    }
}
