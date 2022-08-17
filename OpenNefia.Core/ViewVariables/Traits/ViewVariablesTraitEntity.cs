using System;
using System.Collections.Generic;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.UI.Wisp;
using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UI.Wisp.CustomControls;
using OpenNefia.Core.Utility;
using OpenNefia.Core.ViewVariables;
using OpenNefia.Core.ViewVariables.Instances;
using static OpenNefia.Core.UI.Wisp.Controls.BoxContainer;
using static OpenNefia.Core.UI.Wisp.Controls.LineEdit;
using static OpenNefia.Core.UI.Wisp.WispControl;

namespace OpenNefia.Core.ViewVariables.Traits
{
    internal sealed class ViewVariablesTraitEntity : ViewVariablesTrait
    {
        [Dependency] private readonly IEntityManager _entityManager = default!;
        [Dependency] private readonly IComponentFactory _componentFactory = default!;

        private EntityUid _entity = default!;

        private ViewVariablesAddComponentWindow? _addComponentWindow;

        private BoxContainer _components = default!;
        private Button _componentsAddButton = default!;
        private LineEdit _componentsSearchBar = default!;

        public ViewVariablesTraitEntity()
        {
            IoCManager.InjectDependencies(this);
        }

        public override void Initialize(ViewVariablesInstanceObject instance)
        {
            base.Initialize(instance);

            _entity = (EntityUid)instance.Object;

            _components = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                SeparationOverride = 0
            };
            instance.AddTab(Loc.GetString("OpenNefia.ViewVariables.Entity.Tab.Components"), _components);

            MakeTopBar(instance);

            PopulateComponents();
        }

        public void MakeTopBar(ViewVariablesInstanceObject instance)
        {
            var headBox = instance.TopBar;
            var stringified = PrettyPrint.PrintUserFacingWithType(_entity, out var typeStringified);
            if (typeStringified != "")
            {
                //var smallFont = new VectorFont(_resourceCache.GetResource<FontResource>("/Fonts/CALIBRI.TTF"), 10);
                // Custom ToString() implementation.
                headBox.AddChild(new Label { Text = stringified, ClipText = true });
            }

            if (_entityManager.TryGetComponent(_entity, out ChipComponent? chip))
            {
                var hBox = new BoxContainer
                {
                    Orientation = LayoutOrientation.Horizontal
                };
                headBox.AddChild(hBox);
                hBox.AddChild(new ChipView { ChipComp = chip });
            }
        }

        public override void Refresh()
        {
            PopulateComponents();
        }

        private void PopulateComponents()
        {
            _components.DisposeAllChildren();

            _components.AddChild(_componentsSearchBar = new LineEdit
            {
                PlaceHolder = Loc.GetString("OpenNefia.ViewVariables.Entity.ComponentsSearchBarPlaceholder"),
                HorizontalExpand = true,
            });

            _components.AddChild(_componentsAddButton = new Button()
            {
                Text = Loc.GetString("OpenNefia.ViewVariables.Entity.AddWindowComponents"),
                HorizontalExpand = true,
            });

            _componentsAddButton.OnPressed += OnComponentsAddButtonPressed;
            _componentsSearchBar.OnTextChanged += OnComponentsSearchBarChanged;

            var componentList = _entityManager.GetComponents(_entity).OrderBy(c => c.GetType().ToString());

            foreach (var component in componentList)
            {
                var button = new Button { Text = TypeAbbreviation.Abbreviate(component.GetType()), TextAlign = Label.AlignMode.Left };
                var removeButton = new TextureButton()
                {
                    StyleClasses = { DefaultWindow.StyleClassWindowCloseButton },
                    HorizontalAlignment = HAlignment.Right
                };
                button.OnPressed += _ => Instance.ViewVariablesManager.OpenVV(component);
                removeButton.OnPressed += _ => RemoveComponent(component);
                button.AddChild(removeButton);
                _components.AddChild(button);
            }
        }

        private void UpdateClientComponentListVisibility(string? searchStr = null)
        {
            if (string.IsNullOrEmpty(searchStr))
            {
                foreach (var child in _components.Children)
                {
                    child.Visible = true;
                }

                return;
            }

            foreach (var child in _components.Children)
            {
                if (child is not Button button || child == _componentsAddButton)
                {
                    continue;
                }

                if (button.Text == null)
                {
                    button.Visible = false;
                    continue;
                }

                if (!button.Text.Contains(searchStr, StringComparison.InvariantCultureIgnoreCase))
                {
                    button.Visible = false;
                    continue;
                }

                button.Visible = true;
            }
        }

        private void OnComponentsSearchBarChanged(LineEditEventArgs args)
        {
            UpdateClientComponentListVisibility(args.Text);
        }

        private void OnComponentsAddButtonPressed(BaseButton.ButtonEventArgs _)
        {
            _addComponentWindow?.Dispose();

            var target = GetValidComponentTargets(_entity);
            _addComponentWindow = new ViewVariablesAddComponentWindow(GetValidComponentsForAdding(), Loc.GetString("OpenNefia.ViewVariables.Entity.AddWindowComponents"), target);
            _addComponentWindow.AddButtonPressed += TryAdd;

            _addComponentWindow.OpenCentered(_components.WispRootLayer!); // TODO
        }

        /// <summary>
        ///     Returns an enumeration of components that can *probably* be added to an entity.
        /// </summary>
        private IEnumerable<VVComponentEntry> GetValidComponentsForAdding()
        {
            foreach (var type in _componentFactory.AllRegisteredTypes)
            {
                if (_entityManager.HasComponent(_entity, type))
                    continue;

                var target = ComponentTarget.Normal;

                if (type.TryGetCustomAttribute<ComponentUsageAttribute>(out var usageAttr))
                    target = usageAttr.Target;

                yield return new(_componentFactory.GetRegistration(type).Name, target);
            }
        }

        private ComponentTarget GetValidComponentTargets(EntityUid entity)
        {
            ComponentTarget target;

            if (_entityManager.HasComponent<MapComponent>(entity))
                target = ComponentTarget.Map;
            else if (_entityManager.HasComponent<AreaComponent>(entity))
                target = ComponentTarget.Area;
            else
                target = ComponentTarget.Normal;

            return target;
        }

        private void TryAdd(ViewVariablesAddComponentWindow.AddButtonPressedEventArgs eventArgs)
        {
            if (!_componentFactory.TryGetRegistration(eventArgs.Entry, out var registration)) return;

            try
            {
                var comp = (Component)_componentFactory.GetComponent(registration.Type);
                comp.Owner = _entity;
                _entityManager.AddComponent(_entity, comp);
            }
            catch (Exception e)
            {
                Logger.WarningS("vv", $"Failed to add component!\n{e}");
            }

            PopulateComponents();

            // Update list of components.
            _addComponentWindow?.Populate(GetValidComponentsForAdding());
        }

        private void RemoveComponent(IComponent component)
        {
            try
            {
                _entityManager.RemoveComponent(_entity, component);
            }
            catch (Exception e)
            {
                Logger.WarningS("vv", $"Couldn't remove component!\n{e}");
            }

            PopulateComponents();
        }
    }
}
