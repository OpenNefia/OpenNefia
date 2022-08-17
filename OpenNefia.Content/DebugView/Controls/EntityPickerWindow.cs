using OpenNefia.Content.Charas;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Wisp;
using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UI.Wisp.CustomControls;
using OpenNefia.Core.UserInterface.XAML;
using static OpenNefia.Core.UI.Wisp.Controls.BoxContainer;

namespace OpenNefia.Content.DebugView
{
    public partial class EntityPickerWindow : DefaultWindow
    {
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly ICharaSystem _charas = default!;

        public event Action<EntityButtonPressedEventArgs>? OnEntityButtonPressed;

        public const string StyleClassEntityButton = "entityButton";

        public EntityPickerWindow()
        {
            EntitySystem.InjectDependencies(this);
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
            EntityGrid.RemoveAllChildren();

            foreach (var entityProto in _protos.EnumeratePrototypes<EntityPrototype>())
            {
                if (!string.IsNullOrEmpty(search))
                {
                    var name = Loc.GetPrototypeString(entityProto, "MetaData.Name");
                    if (!entityProto.ID.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                        && !name.Contains(search, StringComparison.InvariantCultureIgnoreCase))
                    {
                        continue;
                    }
                }

                var chipID = Protos.Chip.Default;
                var color = Color.White;
                if (entityProto.Components.TryGetComponent<ChipComponent>(out var chip))
                {
                    chipID = chip.ChipID;
                    color = chip.Color;
                }
                // TODO generalize
                if (entityProto.Components.TryGetComponent<CharaComponent>(out var chara))
                {
                    chipID = _charas.GetDefaultCharaChip(chara.Race, Gender.Male);
                }

                var box = new BoxContainer() { Orientation = LayoutOrientation.Vertical };

                var chipView = new ChipView() { ChipID = chipID, Color = color, Name = entityProto.ID };
                var button = new ContainerButton() { };
                button.AddStyleClass(StyleClassEntityButton);
                button.OnPressed += OnButtonPressed;
                button.OnMouseEntered += OnMouseEntered;
                button.OnMouseExited += OnMouseExited;
                chipView.AddChild(button);
                box.AddChild(chipView);

                EntityGrid.AddChild(box);
            }
        }

        private void OnButtonPressed(BaseButton.ButtonEventArgs obj)
        {
            var chipView = (ChipView)obj.Button.Parent!;
            OnEntityButtonPressed?.Invoke(new(_protos.Index<EntityPrototype>(new(chipView.Name!)), _protos.Index(chipView.ChipID)));
        }

        private void OnMouseEntered(GUIMouseHoverEventArgs obj)
        {
            SelectedName.Text = ((ChipView)((ContainerButton)obj.SourceControl).Parent!).Name;
        }

        private void OnMouseExited(GUIMouseHoverEventArgs obj)
        {
            SelectedName.Text = string.Empty;
        }

        public sealed class EntityButtonPressedEventArgs : EventArgs
        {
            public EntityPrototype EntityPrototype { get; }
            public ChipPrototype ChipPrototype { get; }

            public EntityButtonPressedEventArgs(EntityPrototype entry, ChipPrototype chipPrototype)
            {
                EntityPrototype = entry;
                ChipPrototype = chipPrototype;
            }
        }
    }
}
