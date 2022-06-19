using OpenNefia.Core.Input;
using Timer = OpenNefia.Core.Timing.Timer;
using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UI.Wisp;
using OpenNefia.Core.UI.Wisp.CustomControls;
using OpenNefia.Core.Utility;
using static OpenNefia.Core.UI.Wisp.Controls.BoxContainer;
using OpenNefia.Core.UI.Element;

namespace OpenNefia.Core.ViewVariables.Instances
{
    internal sealed class ViewVariablesInstanceObject : ViewVariablesInstance
    {
        private TabContainer _tabs = default!;
        private Button _refreshButton = default!;
        private int _tabCount;

        private readonly List<ViewVariablesTrait> _traits = new();

        private CancellationTokenSource _refreshCancelToken = new();

        public object Object { get; private set; } = default!;

        public ViewVariablesInstanceObject(IViewVariablesManagerInternal vvm)
            : base(vvm) { }

        public override void Initialize(DefaultWindow window, object obj)
        {
            Object = obj;
            var type = obj.GetType();

            var title = PrettyPrint.PrintUserFacingWithType(obj, out var subtitle);

            _wrappingInit(window, title, subtitle);
            foreach (var trait in TraitsFor(ViewVariablesManager.TraitIdsFor(type)))
            {
                trait.Initialize(this);
                _traits.Add(trait);
            }
            _refresh();
        }

        private void _wrappingInit(DefaultWindow window, string top, string bottom)
        {
            // Wrapping containers.
            var scrollContainer = new ScrollContainer();
            //scrollContainer.SetAnchorPreset(Control.LayoutPreset.Wide, true);
            window.Contents.AddChild(scrollContainer);
            var vBoxContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                HorizontalExpand = true,
                VerticalExpand = true,
            };
            scrollContainer.AddChild(vBoxContainer);

            // Handle top bar.
            {
                var headBox = new BoxContainer
                {
                    Orientation = LayoutOrientation.Horizontal
                };
                var name = MakeTopBar(top, bottom);
                name.HorizontalExpand = true;
                headBox.AddChild(name);

                _refreshButton = new Button { Text = "Refresh", /* ToolTip = "RMB to toggle auto-refresh." */ };
                _refreshButton.OnPressed += _ => _refresh();
                _refreshButton.OnKeyBindDown += OnButtonKeybindDown;
                headBox.AddChild(_refreshButton);
                vBoxContainer.AddChild(headBox);
            }

            _tabs = new TabContainer();
            vBoxContainer.AddChild(_tabs);
        }

        private void OnButtonKeybindDown(GUIBoundKeyEventArgs eventArgs)
        {
            if (eventArgs.Function == EngineKeyFunctions.UIRightClick)
            {
                _refreshButton.ToggleMode = !_refreshButton.ToggleMode;
                _refreshButton.Pressed = !_refreshButton.Pressed;

                _refreshCancelToken.Cancel();

                if (!_refreshButton.Pressed) return;

                _refreshCancelToken = new CancellationTokenSource();
                Timer.SpawnRepeating(500, _refresh, _refreshCancelToken.Token);

            }
            else if (eventArgs.Function == EngineKeyFunctions.UIClick)
            {
                _refreshButton.ToggleMode = false;
            }
        }

        public override void Close()
        {
            base.Close();

            _refreshCancelToken.Cancel();
        }

        public void AddTab(string title, WispControl control)
        {
            _tabs.AddChild(control);
            _tabs.SetTabTitle(_tabCount++, title);
        }

        private void _refresh()
        {
            // TODO: I'm fully aware the ToString() isn't updated.
            // Eh.
            foreach (var trait in _traits)
            {
                trait.Refresh();
            }
        }

        private List<ViewVariablesTrait> TraitsFor(ICollection<Type> traitData)
        {
            // TODO redundant
            var list = new List<ViewVariablesTrait>(traitData.Count);

            foreach (var traitTy in traitData)
            {
                var trait = (ViewVariablesTrait)Activator.CreateInstance(traitTy)!;
                trait.Initialize(this);
                list.Add(trait);
            }

            return list;
        }
    }
}
