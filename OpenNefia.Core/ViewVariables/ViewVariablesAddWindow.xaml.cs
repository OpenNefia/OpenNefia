using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UI.Wisp.CustomControls;
using OpenNefia.Core.UserInterface.XAML;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.ViewVariables
{
    public sealed partial class ViewVariablesAddWindow : DefaultWindow
    {
        protected string? _lastSearch;
        private string[] _entries = Array.Empty<string>();

        public event Action<AddButtonPressedEventArgs>? AddButtonPressed;

        public ViewVariablesAddWindow(IEnumerable<string> entries, LocaleKey title)
        {
            OpenNefiaXamlLoader.Load(this);

            Title = title;

            EntryItemList.OnItemSelected += _ => RefreshAddButton();
            EntryItemList.OnItemDeselected += _ => RefreshAddButton();
            SearchLineEdit.OnTextChanged += OnSearchTextChanged;
            AddButton.OnPressed += OnAddButtonPressed;

            Populate(entries);

            ExactSize = (200, 300);
        }

        private void RefreshAddButton()
        {
            AddButton.Disabled = !EntryItemList.GetSelected().Any();
        }

        public void Populate(IEnumerable<string> entries)
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

            foreach (var entry in _entries)
            {
                if (!string.IsNullOrEmpty(search) && !entry.Contains(search, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                EntryItemList.AddItem(entry);
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
