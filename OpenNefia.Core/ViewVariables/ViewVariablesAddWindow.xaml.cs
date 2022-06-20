using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Locale;
using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UI.Wisp.CustomControls;
using OpenNefia.Core.UserInterface.XAML;

namespace OpenNefia.Core.ViewVariables
{
    public sealed record VVComponentEntry(string Name, ComponentTarget Target);

    public sealed class ViewVariablesAddComponentWindow : ViewVariablesAddWindow<VVComponentEntry>
    {
        private readonly ComponentTarget _componentTarget;

        public ViewVariablesAddComponentWindow(IEnumerable<VVComponentEntry> entries, LocaleKey title, ComponentTarget componentTarget)
            : base(entries, title)
        {
            _componentTarget = componentTarget;
        }
    }

    public sealed class ViewVariablesAddPrototypeWindow : ViewVariablesAddWindow<string>
    {
        public ViewVariablesAddPrototypeWindow(IEnumerable<string> entries, LocaleKey title) 
            : base(entries, title)
        {
        }
    }

    public abstract partial class ViewVariablesAddWindow<T> : DefaultWindow
    {
        private string? _lastSearch;
        private T[] _entries = Array.Empty<T>();

        public event Action<AddButtonPressedEventArgs>? AddButtonPressed;

        public ViewVariablesAddWindow(IEnumerable<T> entries, LocaleKey title)
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

        public void Populate(IEnumerable<T> entries)
        {
            _entries = entries.ToArray();
            Array.Sort(_entries);
            Populate(_lastSearch);
        }

        private void Populate(string? search = null, bool showAll = false)
        {
            _lastSearch = search;
            EntryItemList.ClearSelected();
            EntryItemList.Clear();
            AddButton.Disabled = true;

            foreach (var entry in _entries)
            {
                // if (!string.IsNullOrEmpty(search) && !entry.Contains(search, StringComparison.InvariantCultureIgnoreCase))
                //    continue;

                EntryItemList.AddItem("");
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
