using System.Collections;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.UserInterface.XAML;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Utility;
using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.UI.Wisp.CustomControls
{
    /// <summary>
    /// Simple window implementation that can be resized and has a title bar.
    /// </summary>
    /// <remarks>
    /// Warning: ugly.
    /// </remarks>
    // ReSharper disable once InconsistentNaming
    public partial class DefaultWindow : BaseWindow
    {
        public const string StyleClassWindowTitle = "windowTitle";
        public const string StyleClassWindowTitleAlert = "windowTitleAlert";
        public const string StyleClassWindowPanel = "windowPanel";
        public const string StyleClassWindowHeader = "windowHeader";
        public const string StyleClassWindowHeaderAlert = "windowHeaderAlert";
        public const string StyleClassWindowCloseButton = "windowCloseButton";

        private string? _headerClass;
        private string? _titleClass;

        public DefaultWindow()
        {
            OpenNefiaXamlLoader.Load(this);
            EventFilter = UIEventFilterMode.Stop;

            WindowHeader.MinSize = (0, HEADER_SIZE_Y);

            Contents = ContentsContainer;

            CloseButton.OnPressed += CloseButtonPressed;
            XamlChildren = new WispContentCollection(this);
        }

        public string? HeaderClass
        {
            get => _headerClass;
            set
            {
                if (_headerClass == value)
                    return;

                if (_headerClass != null)
                    WindowHeader.RemoveStyleClass(_headerClass);

                if (value != null)
                    WindowHeader.AddStyleClass(value);

                _headerClass = value;
            }
        }

        public string? TitleClass
        {
            get => _titleClass;
            set
            {
                if (_titleClass == value)
                    return;

                if (_titleClass != null)
                    TitleLabel.RemoveStyleClass(_titleClass);

                if (value != null)
                    TitleLabel.AddStyleClass(value);

                _titleClass = value;
            }
        }

        public WispControl Contents { get; private set; }

        private const int DRAG_MARGIN_SIZE = 7;

        // TODO: Un-hard code this header size.
        private const float HEADER_SIZE_Y = 25;
        protected virtual Vector2 ContentsMinimumSize => (50, 50);

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            return Vector2.ComponentMax(
                ContentsMinimumSize,
                base.MeasureOverride(Vector2.ComponentMax(availableSize, ContentsMinimumSize)));
        }

        public string? Title
        {
            get => TitleLabel.Text;
            set => TitleLabel.Text = value;
        }

        // Drag resizing and moving code is mostly taken from Godot's WindowDialog.

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                CloseButton.OnPressed -= CloseButtonPressed;
            }
        }

        private void CloseButtonPressed(BaseButton.ButtonEventArgs args)
        {
            Close();
        }

        // Prevent window headers from getting off screen due to game window resizes.

        public override void Update(float dt)
        {
            var (spaceX, spaceY) = Parent!.Size;
            if (Position.Y > spaceY)
            {
                LayoutContainer.SetPosition(this, (Position.X, spaceY + HEADER_SIZE_Y));
            }

            if (Position.X > spaceX)
            {
                // 50 is arbitrary here. As long as it's bumped back into view.
                LayoutContainer.SetPosition(this, (spaceX - 50, Position.Y));
            }

            if (Position.Y < 0)
            {
                LayoutContainer.SetPosition(this, (Position.X, 0));
            }

            if (Position.X < 0)
            {
                LayoutContainer.SetPosition(this, (0, Position.Y));
            }
        }

        protected override DragMode GetDragModeFor(Vector2 relativeMousePos)
        {
            var mode = DragMode.None;

            if (Resizable)
            {
                if (relativeMousePos.Y < DRAG_MARGIN_SIZE)
                {
                    mode = DragMode.Top;
                }
                else if (relativeMousePos.Y > Size.Y - DRAG_MARGIN_SIZE)
                {
                    mode = DragMode.Bottom;
                }

                if (relativeMousePos.X < DRAG_MARGIN_SIZE)
                {
                    mode |= DragMode.Left;
                }
                else if (relativeMousePos.X > Size.X - DRAG_MARGIN_SIZE)
                {
                    mode |= DragMode.Right;
                }
            }

            if (mode == DragMode.None && relativeMousePos.Y < HEADER_SIZE_Y)
            {
                mode = DragMode.Move;
            }

            return mode;
        }

        public sealed class WispContentCollection : ICollection<UiElement>, IReadOnlyCollection<UiElement>
        {
            private readonly DefaultWindow Owner;

            public WispContentCollection(DefaultWindow owner)
            {
                Owner = owner;
            }

            public Enumerator GetEnumerator()
            {
                return new(Owner);
            }

            IEnumerator<UiElement> IEnumerable<UiElement>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public void Add(UiElement item)
            {
                Owner.Contents.AddChild(item);
            }

            public void Clear()
            {
                Owner.Contents.RemoveAllChildren();
            }

            public bool Contains(UiElement item)
            {
                return item?.Parent == Owner.Contents;
            }

            public void CopyTo(UiElement[] array, int arrayIndex)
            {
                Owner.Contents.Children.CopyTo(array, arrayIndex);
            }

            public bool Remove(UiElement item)
            {
                if (item?.Parent != Owner.Contents)
                {
                    return false;
                }

                DebugTools.AssertNotNull(Owner?.Contents);
                Owner!.Contents.RemoveChild(item);

                return true;
            }

            int ICollection<UiElement>.Count => Owner.Contents.ChildCount;
            int IReadOnlyCollection<UiElement>.Count => Owner.Contents.ChildCount;

            public bool IsReadOnly => false;


            public struct Enumerator : IEnumerator<UiElement>
            {
                private OrderedChildCollection.Enumerator _enumerator;

                internal Enumerator(DefaultWindow DefaultWindow)
                {
                    _enumerator = DefaultWindow.Contents.Children.GetEnumerator();
                }

                public bool MoveNext()
                {
                    return _enumerator.MoveNext();
                }

                public void Reset()
                {
                    _enumerator.Reset();
                }

                public UiElement Current => _enumerator.Current;

                object IEnumerator.Current => Current;

                public void Dispose()
                {
                    _enumerator.Dispose();
                }
            }
        }
    }
}
