using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.UserInterface.XAML;
using OpenNefia.Core.Utility;
using static NetVips.Enums;

namespace OpenNefia.Core.UI.Element
{
    public partial class UiElement : BaseDrawable, IUiElement, ILocalizable, IUiInput
    {
        public float MinWidth { get; internal set; }
        public float MinHeight { get; internal set; }
        public float PreferredWidth { get; internal set; } = float.NaN;
        public float PreferredHeight { get; internal set; } = float.NaN;
        public float MaxWidth { get; internal set; } = float.PositiveInfinity;
        public float MaxHeight { get; internal set; } = float.PositiveInfinity;

        // ReSharper disable once ValueParameterNotUsed
        public AccessLevel? Access { set { } }

        /// <summary>
        /// TODO remove after merge with wisp
        /// </summary>
        public virtual Vector2 GlobalPosition => Position;

        /// <summary>
        /// TODO remove after merge with wisp
        /// </summary>
        public virtual Vector2i GlobalPixelPosition => PixelPosition;

        /// <summary>
        /// TODO remove after merge with wisp
        /// </summary>

        /// <summary>
        /// TODO remove after merge with wisp
        /// </summary>
        public UIBox2 GlobalRect => UIBox2.FromDimensions(GlobalPosition, Size);

        /// <summary>
        /// TODO remove after merge with wisp
        /// </summary>
        public UIBox2i GlobalPixelRect => UIBox2i.FromDimensions(GlobalPixelPosition, PixelSize);

        /// <summary>
        /// A settable minimum size for this control.
        /// </summary>
        public virtual Vector2 MinSize
        {
            get => (MinWidth, MinHeight);
            set => (MinWidth, MinHeight) = Vector2.ComponentMax(Vector2.Zero, value);
        }

        /// <summary>
        /// A preferred exact size for this control.
        /// </summary>
        public virtual Vector2 PreferredSize
        {
            get => (PreferredWidth, PreferredHeight);
            set => (PreferredWidth, PreferredHeight) = value;
        }

        /// <summary>
        /// A settable maximum size for this control.
        /// </summary>
        public virtual Vector2 MaxSize
        {
            get => (MaxWidth, MaxHeight);
            set => (MaxWidth, MaxHeight) = Vector2.ComponentMax(Vector2.Zero, value);
        }

        /// <summary>
        ///     The position of the top left corner of the control relative to the parent, in virtual pixels.
        /// </summary>
        /// <seealso cref="RelativePixelPosition"/>
        public virtual Vector2 RelativePosition
        {
            get => Position - (Parent?.Position ?? Vector2.Zero);
            set
            {
                Position = (Parent?.Position ?? Vector2.Zero) + value;
            }
        }

        /// <summary>
        ///     The position of the top left corner of the control relative to the parent, in physical pixels.
        /// </summary>
        /// <seealso cref="Position"/>
        public Vector2i RelativePixelPosition => (Vector2i)(RelativePosition * UIScale);

        /// <inheritdoc/>
        public override float UIScale => Root?.UIScale ?? 1;

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

        public NameScope? NameScope;

        //public void AttachNameScope(Dictionary<string, Control> nameScope)
        //{
        //    _nameScope = nameScope;
        //}

        public NameScope? FindNameScope()
        {
            foreach (var control in this.GetSelfAndLogicalAncestors())
            {
                if (control.NameScope != null) return control.NameScope;
            }

            return null;
        }

        public T FindControl<T>(string name) where T : UiElement
        {
            var nameScope = FindNameScope();
            if (nameScope == null)
            {
                throw new ArgumentException("No Namespace found for Control");
            }

            var value = nameScope.Find(name);
            if (value == null)
            {
                throw new ArgumentException($"No Control with the name {name} found");
            }

            if (value is not T ret)
            {
                throw new ArgumentException($"Control with name {name} had invalid type {value.GetType()}");
            }

            return ret;
        }

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
        ///     The mode that controls how mouse *and* bound key filtering works. See the enum for how it functions.
        /// </summary>
        public UIEventFilterMode EventFilter { get; set; } = UIEventFilterMode.Ignore;

        /// <summary>
        ///     Active filters for determining if this control should receive a <see cref="GUIBoundKeyEventArgs"/>.
        /// </summary>
        public List<IBoundKeyEventFilter> BoundKeyEventFilters { get; } = new();

        private bool _canControlFocus;

        /// <summary>
        ///     Whether this control can take control focus.
        ///     Keyboard focus is necessary for the control to receive key binding events.
        /// </summary>
        public bool CanControlFocus
        {
            get => _canControlFocus;
            set
            {
                if (_canControlFocus == value)
                {
                    return;
                }

                _canControlFocus = value;

                if (!value)
                {
                    ReleaseControlFocus();
                }
            }
        }

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

        private bool _visible = true;

        /// <summary>
        ///     Whether or not this control and its children are visible.
        /// </summary>
        /// <remarks>TODO implement</remarks>
        public bool Visible
        {
            get => _visible && (Parent?.Visible ?? true);
            set => _visible = value;
        }

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
        ///     Check if this control currently has control focus.
        /// </summary>
        public bool HasControlFocus()
        {
            return UserInterfaceManager.ControlFocused == this;
        }

        /// <summary>
        ///     Grab control focus if this control doesn't already have it.
        /// </summary>
        public void GrabControlFocus()
        {
            if (CanControlFocus)
            {
                UserInterfaceManager.ControlFocused = this;
            }
        }

        /// <summary>
        ///     Release control focus from this control if it has it.
        ///     If a different control has control focus, nothing happens.
        /// </summary>
        public void ReleaseControlFocus()
        {
            if (HasControlFocus())
            {
                UserInterfaceManager.ControlFocused = null;
            }
        }

        public virtual void GrabFocus()
        {
            if (CanControlFocus)
            {
                GrabControlFocus();
            }
            if (CanKeyboardFocus)
            {
                GrabKeyboardFocus();
            }
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

        public virtual void GetPreferredSize(out Vector2 size)
        {
            size = PreferredSize;
        }

        public void SetPreferredSize()
        {
            GetPreferredSize(out var size);
            this.SetSize(size.X, size.Y);
        }

        public override void SetSize(float width, float height)
        {
            width = float.IsNaN(width) ? Width : width;
            height = float.IsNaN(height) ? Height : height;

            width = Math.Clamp(width, MinWidth, MaxWidth);
            height = Math.Clamp(height, MinHeight, MaxHeight);

            base.SetSize(width, height);
        }

        public virtual void Localize(LocaleKey key)
        {
            IoCManager.Resolve<ILocalizationManager>().DoLocalize(this, key);
            IsLocalized = true;
            LocalizeChildren();
        }

        protected void LocalizeChildren()
        {
            foreach (var child in Children)
            {
                if (child.GetType().TryGetCustomAttribute<LocalizeAttribute>(out var attr))
                {
                    child.Localize(attr.RootLocaleKey ?? throw new ArgumentNullException($"[Localize] attribute declared on type {child.GetType()} had no locale key declared."));
                }
                child.LocalizeChildren();
            }
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
        ///     Gets the "index" in the parent.
        ///     This index is used for ordering of actions like input and drawing among siblings.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        ///     Thrown if this control has no parent.
        /// </exception>
        public int GetPositionInParent()
        {
            if (Parent == null)
            {
                throw new InvalidOperationException("This control has no parent!");
            }

            return Parent._orderedChildren.IndexOf(this);
        }

        /// <summary>
        ///     Sets the index of this control in the parent.
        ///     This pretty much corresponds to layout and drawing order in relation to its siblings.
        /// </summary>
        /// <param name="position"></param>
        /// <exception cref="InvalidOperationException">This control has no parent.</exception>
        public void SetPositionInParent(int position)
        {
            if (Parent == null)
            {
                throw new InvalidOperationException("No parent to change position in.");
            }

            var posInParent = GetPositionInParent();
            if (posInParent == position)
            {
                return;
            }

            Parent._orderedChildren.RemoveAt(posInParent);
            Parent._orderedChildren.Insert(position, this);
            Parent.ChildMoved(this, posInParent, position);
        }

        /// <summary>
        ///     Makes this the first control among its siblings,
        ///     So that it's first in things such as drawing order.
        /// </summary>
        /// <exception cref="InvalidOperationException">This control has no parent.</exception>
        public void SetPositionFirst()
        {
            SetPositionInParent(0);
        }

        /// <summary>
        ///     Makes this the last control among its siblings,
        ///     So that it's last in things such as drawing order.
        /// </summary>
        /// <exception cref="InvalidOperationException">This control has no parent.</exception>
        public void SetPositionLast()
        {
            if (Parent == null)
            {
                throw new InvalidOperationException("No parent to change position in.");
            }

            SetPositionInParent(Parent.ChildCount - 1);
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

        /// <summary>
        ///     Fired when a control loses control focus for any reason. See <see cref="IUserInterfaceManager.ControlFocused"/>.
        /// </summary>
        /// <remarks>
        ///     Controls which have some sort of drag / drop behavior should usually implement this method (typically by cancelling the drag drop).
        ///     Otherwise, if a user clicks down LMB over one control to initiate a drag, then clicks RMB down
        ///     over a different control while still holding down LMB, the control being dragged will now lose focus
        ///     and will no longer receive the keyup for the LMB, thus won't cancel the drag.
        ///     This should also be considered for controls which have any special KeyBindUp behavior - consider
        ///     what would happen if the control lost focus and never received the KeyBindUp.
        ///
        ///     There is no corresponding ControlFocusEntered - if a control wants to handle that situation they should simply
        ///     handle KeyBindDown as that's the only way a control would gain focus.
        /// </remarks>
        protected internal virtual void ControlFocusExited()
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

            child.Parented(this);
            if (Root != null)
            {
                child._propagateEnterTree(Root);
            }

            ChildAdded(child);
        }

        public event Action<UiElement>? OnChildAdded;

        /// <summary>
        ///     Called after a new child is added to this control.
        /// </summary>
        /// <param name="newChild">The new child.</param>
        protected virtual void ChildAdded(UiElement newChild)
        {
            OnChildAdded?.Invoke(newChild);
        }

        /// <summary>
        ///     Called when this control gets made a child of a different control.
        /// </summary>
        /// <param name="newParent">The new parent component.</param>
        protected virtual void Parented(UiElement newParent)
        {
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

            child.Deparented();
            if (IsInsideTree)
            {
                child._propagateExitTree();
            }

            ChildRemoved(child);
        }

        public event Action<UiElement>? OnChildRemoved;

        /// <summary>
        ///     Called when a child is removed from this child.
        /// </summary>
        /// <param name="child">The former child.</param>
        protected virtual void ChildRemoved(UiElement child)
        {
            OnChildRemoved?.Invoke(child);
        }

        /// <summary>
        ///     Called when this control is removed as child from the former parent.
        /// </summary>
        protected virtual void Deparented()
        {
        }

        public event Action<ControlChildMovedEventArgs>? OnChildMoved;

        /// <summary>
        ///     Called when the order index of a child changes.
        /// </summary>
        /// <param name="child">The child that was changed.</param>
        /// <param name="oldIndex">The previous index of the child.</param>
        /// <param name="newIndex">The new index of the child.</param>
        protected virtual void ChildMoved(UiElement child, int oldIndex, int newIndex)
        {
            OnChildMoved?.Invoke(new ControlChildMovedEventArgs(child, oldIndex, newIndex));
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

    public readonly struct ControlChildMovedEventArgs
    {
        public ControlChildMovedEventArgs(UiElement control, int oldIndex, int newIndex)
        {
            Control = control;
            OldIndex = oldIndex;
            NewIndex = newIndex;
        }

        public readonly UiElement Control;
        public readonly int OldIndex;
        public readonly int NewIndex;
    }
}