using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Wisp.CustomControls;
using OpenNefia.Core.Input;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UserInterface;
using static OpenNefia.Core.UI.Wisp.WispControl;
using OpenNefia.Core.UI.Wisp;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.ViewVariables;

namespace OpenNefia.Content.DebugView
{
    public interface IDebugViewLayer : IUiLayerWithResult<UINone, UINone>, IWispLayer
    {
    }

    public sealed class DebugViewLayer : WispLayerWithResult<UINone, UINone>, IDebugViewLayer
    {
        // The dependency on IFieldLayer is why this lives in content instead of core.
        [Dependency] private readonly IFieldLayer _field = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;
        [Dependency] private readonly IViewVariablesManager _viewVariables = default!;

        private bool _initialized = false;

        public DebugViewLayer()
        {
            OnKeyBindDown += HandleKeyBindDown;
        }

        public void Initialize()
        {
            if (_initialized)
                return;

            for (int i = 0; i < 5; i++)
            {
                var win = new DefaultWindow()
                {
                    TitleClass = DefaultWindow.StyleClassWindowTitleAlert,
                    HeaderClass = DefaultWindow.StyleClassWindowHeaderAlert,
                    Title = "Asdfg!",
                    ExactSize = (400, 100),
                    EventFilter = UIEventFilterMode.Pass,
                };

                // win.SetValue(LayoutContainer.DebugProperty, true);

                var box = new BoxContainer()
                {
                    Orientation = BoxContainer.LayoutOrientation.Vertical
                };

                for (int j = 0; j < 3; j++)
                {
                    box.AddChild(new Label()
                    {
                        Text = $"テスト{j}でござる。",
                        HorizontalAlignment = HAlignment.Left,
                        HorizontalExpand = true
                    });
                }

                win.Contents.AddChild(box);
                this.OpenWindowCentered(win);
            }

            var controlTestWindow = new ControlTestMainWindow();
            this.OpenWindowToLeft(controlTestWindow);

            var controlDebugWindow = new ControlDebugWindow();
            this.OpenWindowCentered(controlDebugWindow);

            _initialized = true;
        }

        public override void OnQuery()
        {
            base.OnQuery();
            Initialize();
        }

        private void HandleKeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel)
            {
                Cancel();
            }
            else if (args.Function == EngineKeyFunctions.UIRightClick)
            {
                if (_mapManager.ActiveMap != null)
                {
                    var pos = _field.Camera.VisibleScreenToTile(args.PointerLocation.Position * UIScale);
                    Logger.InfoS("debugview", $"{pos}");
                    OnRightClick(_mapManager.ActiveMap, pos);
                }
            }
        }

        private void OnRightClick(IMap map, Vector2i pos)
        {
            if (!map.IsInBounds(pos))
                return;

            var coords = map.AtPos(pos);

            foreach (var entity in _lookup.GetLiveEntitiesAtCoords(coords))
            {
                _viewVariables.OpenVV(entity, this);
            }
        }
    }
}