using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UI.Wisp.CustomControls;
using OpenNefia.Core.UserInterface.XAML;
using static Love.NativeLibraryUtil;
using System.Xml.Linq;
using static OpenNefia.Core.UI.Wisp.Controls.BoxContainer;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Wisp;

namespace OpenNefia.Content.DebugView
{
    public partial class TilePickerWindow : DefaultWindow
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;

        public event Action<TileButtonPressedEventArgs>? OnTileButtonPressed;

        public const string StyleClassTileButton = "tileButton";

        public TilePickerWindow()
        {
            IoCManager.InjectDependencies(this);
            OpenNefiaXamlLoader.Load(this);

            SearchLineEdit.OnTextChanged += OnSearchTextChanged;

            Refresh();
        }

        private void OnSearchTextChanged(LineEdit.LineEditEventArgs obj)
        {
            Refresh(obj.Text);
        }

        public void Refresh(string? search = null)
        {
            TileGrid.RemoveAllChildren();

            foreach (var tileProto in _protos.EnumeratePrototypes<TilePrototype>())
            {
                if (!string.IsNullOrEmpty(search) && !tileProto.ID.Contains(search, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                var box = new BoxContainer() { Orientation = LayoutOrientation.Vertical };

                var tileView = new TileView() { Tile = tileProto, Name = tileProto.ID };
                var button = new ContainerButton() {};
                button.AddStyleClass(StyleClassTileButton);
                button.OnPressed += OnButtonPressed;
                button.OnMouseEntered += OnMouseEntered;
                button.OnMouseExited += OnMouseExited;
                tileView.AddChild(button);
                box.AddChild(tileView);

                TileGrid.AddChild(box);
            }
        }

        private void OnButtonPressed(BaseButton.ButtonEventArgs obj)
        {
            var tileView = (TileView)obj.Button.Parent!;
            OnTileButtonPressed?.Invoke(new(tileView.Tile!));
        }

        private void OnMouseEntered(GUIMouseHoverEventArgs obj)
        {
            SelectedName.Text = ((TileView)((ContainerButton)obj.SourceControl).Parent!).Name;
        }

        private void OnMouseExited(GUIMouseHoverEventArgs obj)
        {
            SelectedName.Text = string.Empty;
        }

        public sealed class TileButtonPressedEventArgs : EventArgs
        {
            public TilePrototype TilePrototype { get; }

            public TileButtonPressedEventArgs(TilePrototype entry)
            {
                TilePrototype = entry;
            }
        }
    }
}
