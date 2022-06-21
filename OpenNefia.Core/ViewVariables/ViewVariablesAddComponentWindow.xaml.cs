using OpenNefia.Core.GameObjects;
using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UI.Wisp.CustomControls;
using OpenNefia.Core.UserInterface.XAML;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.ViewVariables
{
    public sealed record VVComponentEntry(string Name, ComponentTarget Target) : IComparable<VVComponentEntry>
    {
        public int CompareTo(VVComponentEntry? other) => Name.CompareTo(other?.Name);
    }

    public sealed partial class ViewVariablesAddComponentWindow : DefaultWindow
    {
        protected string? _lastSearch;
        private VVComponentEntry[] _entries = Array.Empty<VVComponentEntry>();
        private readonly ComponentTarget _componentTarget;

        public event Action<AddButtonPressedEventArgs>? AddButtonPressed;

        public ViewVariablesAddComponentWindow(IEnumerable<VVComponentEntry> entries, LocaleKey title, ComponentTarget componentTarget)
        {
            OpenNefiaXamlLoader.Load(this);

            _componentTarget = componentTarget;

            Title = title;

            EntryItemList.OnItemSelected += _ => RefreshAddButton();
            EntryItemList.OnItemDeselected += _ => RefreshAddButton();
            SearchLineEdit.OnTextChanged += OnSearchTextChanged;
            AddButton.OnPressed += OnAddButtonPressed;
            ShowAllCheckBox.OnPressed += _ => PopulateWithLastSearch();

            Populate(entries);

            ExactSize = (200, 300);
        }

        private void RefreshAddButton()
        {
            AddButton.Disabled = !EntryItemList.GetSelected().Any();
        }

        private void PopulateWithLastSearch()
        {
            Populate(_lastSearch);
        }

        public void Populate(IEnumerable<VVComponentEntry> entries)
        {
            _entries = entries.ToArray();
            Array.Sort(_entries);
            Populate(_lastSearch);
        }

        protected void Populate(string? search = null)
        {
            _lastSearch = search;
            EntryItemList.ClearSelected();
            EntryItemList.Clear();
            AddButton.Disabled = true;

            foreach (var (name, target) in _entries)
            {
                if (!string.IsNullOrEmpty(search) && !name.Contains(search, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                if (!ShowAllCheckBox.Pressed && _componentTarget != target)
                    continue;

                EntryItemList.AddItem(name);
            }
        }

        private void OnSearchTextChanged(LineEdit.LineEditEventArgs obj)
        {
            Populate(obj.Text);
        }

        private void OnAddButtonPressed(BaseButton.ButtonEventArgs obj)
        {
            var selected = EntryItemList.GetSelected().ToArray();

            // Nothing to do here!
            if (selected.Length == 0)
                return;

            var comp = selected[0];

            // This shouldn't really happen.
            if (comp.Text == null)
                return;

            AddButtonPressed?.Invoke(new AddButtonPressedEventArgs(comp.Text));
        }

        public sealed class AddButtonPressedEventArgs : EventArgs
        {
            public string Entry { get; }

            public AddButtonPressedEventArgs(string entry)
            {
                Entry = entry;
            }
        }
    }
}
