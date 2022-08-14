using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UI.Wisp.CustomControls;
using OpenNefia.Core.UserInterface.XAML;
using static OpenNefia.Core.UI.Wisp.Controls.BoxContainer;

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

            Refresh();
        }

        public void Refresh()
        {
            foreach (var tileProto in _protos.EnumeratePrototypes<TilePrototype>())
            {
                var box = new BoxContainer() { Orientation = LayoutOrientation.Vertical };

                var tileView = new TileView() { Tile = tileProto, Name = nameof(TileView) };
                var button = new ContainerButton() { };
                button.AddStyleClass(StyleClassTileButton);
                button.OnPressed += OnButtonPressed;
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
