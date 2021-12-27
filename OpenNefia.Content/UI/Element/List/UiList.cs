using Love;
using OpenNefia.Core;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Content.Prototypes;
using System.Collections;
using MouseButton = OpenNefia.Core.UI.MouseButton;
using OpenNefia.Core.Utility;

namespace OpenNefia.Content.UI.Element.List
{
    public class UiList<T> : BaseInputUiElement, IUiList<T>
    {
        public const int DEFAULT_ITEM_HEIGHT = 19;

        protected IList<IUiListCell<T>> Cells { get; }
        public int ItemHeight { get; }
        public int ItemOffsetX { get; }

        public bool HighlightSelected { get; set; }
        public bool SelectOnActivate { get; set; }

        private int _SelectedIndex;
        public int SelectedIndex
        {
            get => _SelectedIndex;
            set
            {
                if (value < 0 || value >= Cells.Count)
                    throw new ArgumentException($"Index {value} is out of bounds (count: {Cells.Count})");
                _SelectedIndex = value;
            }
        }
        public IUiListCell<T> SelectedCell { get => Cells[SelectedIndex]!; }

        protected Dictionary<int, UiListChoiceKey> ChoiceKeys;

        public event UiListEventHandler<T>? EventOnSelect;
        public event UiListEventHandler<T>? EventOnActivate;

        public UiList(IEnumerable<IUiListCell<T>>? cells = null, int itemOffsetX = 0)
        {
            if (cells == null)
                cells = new List<IUiListCell<T>>();

            ItemHeight = DEFAULT_ITEM_HEIGHT;
            ItemOffsetX = 0;
            HighlightSelected = true;
            SelectOnActivate = true;

            Cells = cells.ToList();

            ChoiceKeys = new Dictionary<int, UiListChoiceKey>();
            for (var i = 0; i < Cells.Count; i++)
            {
                var cell = Cells[i];
                cell.IndexInList = i;
                if (cell.Key == null)
                {
                    cell.Key = UiListChoiceKey.MakeDefault(i);
                }
                ChoiceKeys[i] = cell.Key;
            }

            BindKeys();
        }

        public UiList(IEnumerable<T> items, int itemOffsetX = 0)
            : this(MakeDefaultList(items), itemOffsetX)
        {
        }

        public override void Localize(LocaleKey key)
        {
            for (int i = 0; i < Cells.Count; i++)
            {
                var cell = Cells[i];
                cell.Localize(key.With(cell.LocalizeKey ?? i.ToString()));
            }
        }

        private static IEnumerable<IUiListCell<TItem>> MakeDefaultList<TItem>(IEnumerable<TItem> items)
        {
            IUiListCell<TItem> MakeListCell(TItem item, int index)
            {
                if (item is IUiListItem)
                {
                    var listItem = (IUiListItem)item;
                    return new UiListCell<TItem>(item, listItem.GetChoiceText(index), listItem.GetChoiceKey(index));
                }
                else
                {
                    return new UiListCell<TItem>(item, $"{item}", UiListChoiceKey.MakeDefault(index));
                }
            }
            return items.Select(MakeListCell);
        }

        protected virtual void BindKeys()
        {
            for (int i = 0; i < ChoiceKeys.Count; i++)
            {
                var choiceKey = ChoiceKeys[i];
                IKeybind keybind;
                if (choiceKey.UseKeybind)
                {
                    if (CoreKeybinds.SelectionKeys.ContainsKey(choiceKey.Key))
                    {
                        keybind = CoreKeybinds.SelectionKeys[choiceKey.Key];
                    }
                    else
                    {
                        keybind = new RawKey(choiceKey.Key);
                    }
                }
                else
                {
                    keybind = new RawKey(choiceKey.Key);
                }

                // C# doesn't capture locals in closures like Lua does with upvalues.
                var indexCopy = i;
                Keybinds[keybind] += (_) => Activate(indexCopy);
            }

            Keybinds[CoreKeybinds.UIUp] += (_) =>
            {
                Sounds.Play(Protos.Sound.Cursor1);
                IncrementIndex(-1);
            };
            Keybinds[CoreKeybinds.UIDown] += (_) =>
            {
                Sounds.Play(Protos.Sound.Cursor1);
                IncrementIndex(1);
            };
            Keybinds[CoreKeybinds.Enter] += (_) => Activate(SelectedIndex);

            MouseMoved.Callback += (evt) =>
            {
                for (var index = 0; index < Cells.Count; index++)
                {
                    if (Cells[index].ContainsPoint(evt.Pos))
                    {
                        if (SelectedIndex != index)
                        {
                            Sounds.Play(Protos.Sound.Cursor1);
                            Select(index);
                        }
                        break;
                    }
                }
            };

            this.MouseButtons[MouseButton.Mouse1] += (evt) =>
            {
                if (this.SelectedCell.ContainsPoint(evt.Pos))
                {
                    this.Activate(this.SelectedIndex);
                }
            };
        }

        #region Data Creation

        public override List<UiKeyHint> MakeKeyHints()
        {
            return new List<UiKeyHint>();
        }

        #endregion

        #region List Handling

        protected virtual void OnSelect(UiListEventArgs<T> e)
        {
            UiListEventHandler<T>? handler = EventOnSelect;
            handler?.Invoke(this, e);
        }

        protected virtual void OnActivate(UiListEventArgs<T> e)
        {
            UiListEventHandler<T>? handler = EventOnActivate;
            handler?.Invoke(this, e);
        }

        public virtual bool CanSelect(int index)
        {
            return index >= 0 && index < Cells.Count;
        }

        public void IncrementIndex(int delta)
        {
            var newIndex = SelectedIndex + delta;
            var sign = Math.Sign(delta);

            while (!CanSelect(newIndex) && newIndex != SelectedIndex)
            {
                newIndex += sign;
                if (newIndex < 0)
                    newIndex = Count - 1;
                else if (newIndex >= Count)
                    newIndex = 0;
            }
            Select(newIndex);
        }

        public void Select(int index)
        {
            if (!CanSelect(index))
            {
                return;
            }

            SelectedIndex = index;
            OnSelect(new UiListEventArgs<T>(this[index], index));
        }

        public virtual bool CanActivate(int index)
        {
            return index >= 0 && index < Cells.Count;
        }

        public void Activate(int index)
        {
            if (!CanActivate(index))
            {
                return;
            }

            if (SelectOnActivate)
                Select(index);

            OnActivate(new UiListEventArgs<T>(this[index], index));
        }

        #endregion

        #region UI Handling

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);

            var iy = Y;

            for (int index = 0; index < Count; index++)
            {
                var cell = Cells[index];
                cell.XOffset = ItemOffsetX;
                cell.SetPosition(X, iy);

                iy += cell.Height;
            }
        }

        public override void GetPreferredSize(out Vector2i size)
        {
            size = Vector2i.Zero;

            for (int index = 0; index < Count; index++)
            {
                var cell = Cells[index];
                cell.GetPreferredSize(out var cellSize);
                size.X = Math.Max(size.X, cellSize.X);
                size.Y += Math.Max(cellSize.Y, ItemHeight);
            }
        }

        public override void SetSize(int width, int height)
        {
            for (int index = 0; index < Count; index++)
            {
                var cell = Cells[index];
                cell.GetPreferredSize(out var cellSize);
                cell.SetSize(width, Math.Max(cellSize.Y, ItemHeight));
                width = Math.Max(width, cell.Width);
            }

            base.SetSize(width, height);
        }

        public override void Update(float dt)
        {
            foreach (var cell in Cells)
                cell.Update(dt);
        }

        public override void Draw()
        {
            for (int index = 0; index < Count; index++)
            {
                var cell = Cells[index];
                cell.Draw();

                if (HighlightSelected && index == SelectedIndex)
                {
                    cell.DrawHighlight();
                }
            }
        }

        public override void Dispose()
        {
            foreach (var cell in Cells)
            {
                cell.Dispose();
            }
        }

        #endregion

        #region IList implementation

        public int Count => Cells.Count;
        public bool IsReadOnly => Cells.IsReadOnly;

        public IUiListCell<T> this[int index] { get => Cells[index]; set => Cells[index] = value; }
        public int IndexOf(IUiListCell<T> item) => Cells.IndexOf(item);
        public void Insert(int index, IUiListCell<T> item) => Cells.Insert(index, item);
        public void RemoveAt(int index) => Cells.RemoveAt(index);
        public void Add(IUiListCell<T> item) => Cells.Add(item);
        public void Clear() => Cells.Clear();
        public void AddRange(IEnumerable<IUiListCell<T>> items) => Cells.AddRange(items);
        public bool Contains(IUiListCell<T> item) => Cells.Contains(item);
        public void CopyTo(IUiListCell<T>[] array, int arrayIndex) => Cells.CopyTo(array, arrayIndex);
        public bool Remove(IUiListCell<T> item) => Cells.Remove(item);

        public bool IsFixedSize => false;
        public bool IsSynchronized => false;
        public object SyncRoot => this;

        public IEnumerator<IUiListCell<T>> GetEnumerator() => Cells.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Cells.GetEnumerator();

        #endregion
    }
}
