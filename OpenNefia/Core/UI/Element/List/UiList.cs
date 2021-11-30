using Love;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Data.Types;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI.Element.List
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
        public int SelectedIndex { 
            get => _SelectedIndex; 
            set
            {
                if (value < 0 || value >= this.Cells.Count)
                    throw new ArgumentException($"Index {value} is out of bounds (count: {this.Cells.Count})");
                this._SelectedIndex = value;
            }
        }
        public IUiListCell<T> SelectedCell { get => this.Cells[this.SelectedIndex]!; }

        protected Dictionary<int, UiListChoiceKey> ChoiceKeys;

        public event UiListEventHandler<T>? EventOnSelect;
        public event UiListEventHandler<T>? EventOnActivate;

        public UiList(IEnumerable<IUiListCell<T>>? cells = null, int itemOffsetX = 0)
        {
            if (cells == null)
                cells = new List<IUiListCell<T>>();

            this.ItemHeight = DEFAULT_ITEM_HEIGHT;
            this.ItemOffsetX = 0;
            this.HighlightSelected = true;
            this.SelectOnActivate = true;

            this.Cells = cells.ToList();

            this.ChoiceKeys = new Dictionary<int, UiListChoiceKey>();
            for (var i = 0; i < this.Cells.Count; i++)
            {
                var cell = this.Cells[i];
                if (cell.Key == null)
                {
                    cell.Key = UiListChoiceKey.MakeDefault(i);
                }
                this.ChoiceKeys[i] = cell.Key;
            }

            this.BindKeys();
        }

        public UiList(IEnumerable<T> items, int itemOffsetX = 0)
            : this(MakeDefaultList(items), itemOffsetX)
        {
        }

        public override void Localize(LocaleKey key)
        {
            for (int i = 0; i < this.Cells.Count; i++)
            {
                var cell = this.Cells[i];
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
            for (int i = 0; i < this.ChoiceKeys.Count; i++)
            {
                var choiceKey = this.ChoiceKeys[i];
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
                this.Keybinds[keybind] += (_) => this.Activate(indexCopy);
            }

            this.Keybinds[CoreKeybinds.UIUp] += (_) =>
            {
                Sounds.Play(SoundPrototypeOf.Cursor1);
                this.IncrementIndex(-1);
            };
            this.Keybinds[CoreKeybinds.UIDown] += (_) =>
            {
                Sounds.Play(SoundPrototypeOf.Cursor1);
                this.IncrementIndex(1);
            };
            this.Keybinds[CoreKeybinds.Enter] += (_) => this.Activate(this.SelectedIndex);

            this.MouseMoved.Callback += (evt) =>
            {
                for (var index = 0; index < this.Cells.Count; index++)
                {
                    if (this.Cells[index].ContainsPoint(evt.Pos))
                    {
                        if (this.SelectedIndex != index)
                        {
                            Sounds.Play(SoundPrototypeOf.Cursor1);
                            this.Select(index);
                        }
                        break;
                    }
                }
            };

            this.MouseButtons[UI.MouseButtons.Mouse1] += (evt) =>
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
            var newIndex = this.SelectedIndex + delta;
            var sign = Math.Sign(delta);

            while (!this.CanSelect(newIndex) && newIndex != SelectedIndex)
            {
                newIndex += sign;
                if (newIndex < 0)
                    newIndex = this.Count - 1;
                else if (newIndex >= this.Count)
                    newIndex = 0;
            }
            this.Select(newIndex);
        }

        public void Select(int index)
        {
            if (!this.CanSelect(index))
            {
                return;
            }

            this.SelectedIndex = index;
            this.OnSelect(new UiListEventArgs<T>(this[index], index));
        }

        public virtual bool CanActivate(int index)
        {
            return index >= 0 && index < Cells.Count;
        }

        public void Activate(int index)
        {
            if (!this.CanActivate(index))
            {
                return;
            }

            if (this.SelectOnActivate)
                this.Select(index);

            this.OnActivate(new UiListEventArgs<T>(this[index], index));
        }

        #endregion

        #region UI Handling

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);

            var iy = this.Y;

            for (int index = 0; index < this.Count; index++)
            {
                var cell = this.Cells[index];
                cell.XOffset = this.ItemOffsetX;
                cell.SetPosition(this.X, iy);

                iy += cell.Height;
            }
        }

        public override void GetPreferredSize(out Vector2i size)
        {
            size = Vector2i.Zero;

            for (int index = 0; index < this.Count; index++)
            {
                var cell = this.Cells[index];
                cell.GetPreferredSize(out var cellSize);
                size.X = Math.Max(size.X, cellSize.X);
                size.Y += Math.Max(cellSize.Y, this.ItemHeight);
            }
        }

        public override void SetSize(int width, int height)
        {
            for (int index = 0; index < this.Count; index++)
            {
                var cell = this.Cells[index];
                cell.GetPreferredSize(out var cellSize);
                cell.SetSize(width, Math.Max(cellSize.Y, this.ItemHeight));
                width = Math.Max(width, cell.Width);
            }

            base.SetSize(width, height);
        }
        
        public override void Update(float dt)
        {
            foreach (var cell in this.Cells)
                cell.Update(dt);
        }

        public override void Draw()
        {
            for (int index = 0; index < this.Count; index++)
            {
                var cell = this.Cells[index];
                cell.Draw();

                if (this.HighlightSelected && index == this.SelectedIndex)
                {
                    cell.DrawHighlight();
                }
            }
        }

        public override void Dispose()
        {
            foreach (var cell in this.Cells)
            {
                cell.Dispose();
            }
        }

        #endregion

        #region IList implementation

        public int Count => this.Cells.Count;
        public bool IsReadOnly => this.Cells.IsReadOnly;

        public IUiListCell<T> this[int index] { get => this.Cells[index]; set => this.Cells[index] = value; }
        public int IndexOf(IUiListCell<T> item) => this.Cells.IndexOf(item);
        public void Insert(int index, IUiListCell<T> item) => this.Cells.Insert(index, item);
        public void RemoveAt(int index) => this.Cells.RemoveAt(index);
        public void Add(IUiListCell<T> item) => this.Cells.Add(item);
        public void Clear() => this.Cells.Clear();
        public bool Contains(IUiListCell<T> item) => this.Cells.Contains(item);
        public void CopyTo(IUiListCell<T>[] array, int arrayIndex) => this.Cells.CopyTo(array, arrayIndex);
        public bool Remove(IUiListCell<T> item) => this.Cells.Remove(item);

        public bool IsFixedSize => false;
        public bool IsSynchronized => false;
        public object SyncRoot => this;

        public IEnumerator<IUiListCell<T>> GetEnumerator() => this.Cells.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.Cells.GetEnumerator();

        #endregion
    }
}
