using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.UI.Element
{
    public partial class UiElement : BaseDrawable, IUiDefaultSizeable, ILocalizable, IUiInput
    {
        private readonly List<UiElement> _orderedChildren = new();

        /// <summary>
        ///     Our parent inside the control tree.
        /// </summary>
        public UiElement? Parent { get; private set; }

        public bool IsLocalized { get; protected set; }

        /// <summary>
        ///     Whether or not this control is an (possibly indirect) child of
        ///     <see cref="IUserInterfaceManager.RootControl"/>
        /// </summary>
        public bool IsInsideTree => Root != null;

        public virtual UiLayer? Root { get; internal set; }

        internal IUserInterfaceManagerInternal UserInterfaceManagerInternal { get; }

        /// <summary>
        ///     The UserInterfaceManager we belong to, for convenience.
        /// </summary>
        public IUserInterfaceManager UserInterfaceManager => UserInterfaceManagerInternal;

        /// <summary>
        ///     Gets an ordered enumerable over all the children of this control.
        /// </summary>
        public OrderedChildCollection Children { get; }

        public int ChildCount => _orderedChildren.Count;

        /// <summary>
        ///     The amount of "real" pixels a virtual pixel takes up.
        ///     The higher the number, the bigger the interface.
        ///     I.e. UIScale units are real pixels (rp) / virtual pixels (vp),
        ///     real pixels varies depending on interface, virtual pixels doesn't.
        ///     And vp * UIScale = rp, and rp / UIScale = vp
        /// </summary>
        public virtual float UIScale => 1;

        /// <summary>
        ///     The mode that controls how mouse filtering works. See the enum for how it functions.
        /// </summary>
        public MouseFilterMode MouseFilter { get; set; } = MouseFilterMode.Ignore;

        private bool _canKeyboardFocus;

        /// <summary>
        ///     Whether this control can take keyboard focus.
        ///     Keyboard focus is necessary for the control to receive keyboard events.
        /// </summary>
        /// <seealso cref="KeyboardFocusOnClick"/>
        public bool CanKeyboardFocus
        {
            get => _canKeyboardFocus;
            set
            {
                if (_canKeyboardFocus == value)
                {
                    return;
                }

                _canKeyboardFocus = value;

                if (!value)
                {
                    ReleaseKeyboardFocus();
                }
            }
        }

        /// <summary>
        ///     Whether the control will automatically receive keyboard focus (if possible) when clicked on.
        /// </summary>
        /// <remarks>
        ///     Obviously, <see cref="CanKeyboardFocus"/> must be set to true for this to work.
        /// </remarks>
        public bool KeyboardFocusOnClick { get; set; }

        /// <summary>
        ///     Whether or not this control and its children are visible.
        /// </summary>
        /// <remarks>TODO implement</remarks>
        public bool Visible { get; set; } = true;

        public UiElement()
        {
            UserInterfaceManagerInternal = IoCManager.Resolve<IUserInterfaceManagerInternal>();
            Children = new OrderedChildCollection(this);
        }

        public override void Update(float dt)
        {
        }

        public override void Draw()
        {
        }

        /// <summary>
        ///     Check if this control currently has keyboard focus.
        /// </summary>
        /// <returns></returns>
        public virtual bool HasKeyboardFocus()
        {
            return UserInterfaceManager.KeyboardFocused == this;
        }

        /// <summary>
        ///     Grab keyboard focus if this control doesn't already have it.
        /// </summary>
        /// <remarks>
        ///     <see cref="CanKeyboardFocus"/> must be true for this to work.
        /// </remarks>
        public void GrabKeyboardFocus()
        {
            UserInterfaceManager.GrabKeyboardFocus(this);
        }

        /// <summary>
        ///     Release keyboard focus from this control if it has it.
        ///     If a different control has keyboard focus, nothing happens.
        /// </summary>
        public void ReleaseKeyboardFocus()
        {
            UserInterfaceManager.ReleaseKeyboardFocus(this);
        }

        public virtual void GetPreferredSize(out Vector2i size)
        {
            size = new Vector2i(64, 64);
        }

        public void SetPreferredSize()
        {
            this.GetPreferredSize(out var size);
            this.SetSize(size.X, size.Y);
        }

        public virtual void Localize(LocaleKey key)
        {
            IoCManager.Resolve<ILocalizationManager>().DoLocalize(this, key);
            IsLocalized = true;
        }

        /// <summary>
        ///     Gets the immediate child of this control with the specified index.
        /// </summary>
        /// <param name="index">The index of the child.</param>
        /// <returns>The child.</returns>
        public UiElement GetChild(int index)
        {
            return _orderedChildren[index];
        }

        /// <summary>
        ///     Called when this control receives keyboard focus.
        /// </summary>
        protected internal virtual void KeyboardFocusEntered()
        {
        }

        /// <summary>
        ///     Called when this control loses keyboard focus (corresponds to UserInterfaceManager.KeyboardFocused).
        /// </summary>
        protected internal virtual void KeyboardFocusExited()
        {
        }

        public bool Disposed { get; private set; }

        /// <summary>
        ///     Dispose this control, its own scene control, and all its children.
        ///     Basically the big delete button.
        /// </summary>
        public override void Dispose()
        {
            if (Disposed)
            {
                return;
            }

            Dispose(true);
            Disposed = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            DisposeAllChildren();
            Parent?.RemoveChild(this);

            OnKeyBindDown = null;
        }

        /// <summary>
        ///     Dispose all children, but leave this one intact.
        /// </summary>
        public void DisposeAllChildren()
        {
            // Cache because the children modify the dictionary.
            foreach (var child in Children.ToList())
            {
                child.Dispose();
            }
        }

        /// <summary>
        ///     Remove all the children from this control.
        /// </summary>
        public void RemoveAllChildren()
        {
            foreach (var child in Children.ToArray())
            {
                RemoveChild(child);
            }
        }

        public virtual List<UiKeyHint> MakeKeyHints()
        {
            return new List<UiKeyHint>();
        }

        /// <summary>
        ///     Make the provided control a parent of this control.
        /// </summary>
        /// <param name="child">The control to make a child of this control.</param>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if we already have a component with the same name,
        ///     or the provided component is still parented to a different control.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="child" /> is <c>null</c>.
        /// </exception>
        public void AddChild(UiElement child)
        {
            DebugTools.Assert(!Disposed, "Control has been disposed.");

            if (child == null) throw new ArgumentNullException(nameof(child));
            if (child.Parent != null)
            {
                throw new InvalidOperationException("This component is still parented. Deparent it before adding it.");
            }

            DebugTools.Assert(!child.Disposed, "Child is disposed.");

            if (child == this)
            {
                throw new InvalidOperationException("You can't parent something to itself!");
            }

            // Ensure this control isn't a parent of ours.
            // Doesn't need to happen if the control has no children of course.
            if (child.ChildCount != 0)
            {
                for (var parent = Parent; parent != null; parent = parent.Parent)
                {
                    if (parent == child)
                    {
                        throw new ArgumentException("This control is one of our parents!", nameof(child));
                    }
                }
            }

            child.Parent = this;
            _orderedChildren.Add(child);

            if (Root != null)
            {
                child._propagateEnterTree(Root);
            }
        }

        /// <summary>
        ///     Removes the provided child from this control.
        /// </summary>
        /// <param name="child">The child to remove.</param>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if the provided child is not one of this control's children.
        /// </exception>
        public void RemoveChild(UiElement child)
        {
            DebugTools.Assert(!Disposed, "Control has been disposed.");

            if (child.Parent != this)
            {
                throw new InvalidOperationException("The provided control is not a direct child of this control.");
            }

            _orderedChildren.Remove(child);

            child.Parent = null;

            if (IsInsideTree)
            {
                child._propagateExitTree();
            }
        }

        private void _propagateExitTree()
        {
            Root = null;
            _exitedTree();

            foreach (var child in _orderedChildren)
            {
                child._propagateExitTree();
            }
        }

        /// <summary>
        ///     Called when the control is removed from the root control tree.
        /// </summary>
        /// <seealso cref="EnteredTree"/>
        protected virtual void ExitedTree()
        {
        }

        private void _exitedTree()
        {
            ExitedTree();
            UserInterfaceManagerInternal.ControlRemovedFromTree(this);
        }

        private void _propagateEnterTree(UiLayer root)
        {
            Root = root;
            _enteredTree();

            foreach (var child in _orderedChildren)
            {
                child._propagateEnterTree(root);
            }
        }

        /// <summary>
        ///     Called when the control enters the root control tree.
        /// </summary>
        /// <seealso cref="ExitedTree"/>
        protected virtual void EnteredTree()
        {
        }

        private void _enteredTree()
        {
            EnteredTree();
        }

        public class OrderedChildCollection : ICollection<UiElement>, IReadOnlyCollection<UiElement>
        {
            private readonly UiElement Owner;

            public OrderedChildCollection(UiElement owner)
            {
                Owner = owner;
            }

            public Enumerator GetEnumerator()
            {
                return new(Owner);
            }

            IEnumerator<UiElement> IEnumerable<UiElement>.GetEnumerator() => GetEnumerator();
            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();

            public void Add(UiElement item)
            {
                Owner.AddChild(item);
            }

            public void Clear()
            {
                Owner.RemoveAllChildren();
            }

            public bool Contains(UiElement item)
            {
                return item?.Parent == Owner;
            }

            public void CopyTo(UiElement[] array, int arrayIndex)
            {
                Owner._orderedChildren.CopyTo(array, arrayIndex);
            }

            public bool Remove(UiElement item)
            {
                if (item?.Parent != Owner)
                {
                    return false;
                }

                DebugTools.AssertNotNull(Owner);
                Owner.RemoveChild(item);

                return true;
            }

            int ICollection<UiElement>.Count => Owner.ChildCount;
            int IReadOnlyCollection<UiElement>.Count => Owner.ChildCount;

            public bool IsReadOnly => false;


            public struct Enumerator : IEnumerator<UiElement>
            {
                private List<UiElement>.Enumerator _enumerator;

                internal Enumerator(UiElement control)
                {
                    _enumerator = control._orderedChildren.GetEnumerator();
                }

                public bool MoveNext()
                {
                    return _enumerator.MoveNext();
                }

                public void Reset()
                {
                    ((System.Collections.IEnumerator)_enumerator).Reset();
                }

                public UiElement Current => _enumerator.Current;

                object System.Collections.IEnumerator.Current => Current;

                public void Dispose()
                {
                    _enumerator.Dispose();
                }
            }
        }
    }
}