using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.HotReload;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Timing;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Layer;

namespace OpenNefia.Core.UserInterface
{
    public sealed partial class UserInterfaceManager
    {
        [Dependency] private readonly IDynamicTypeFactory _dynTypeFactory = default!;

        internal List<UiLayer> Layers { get; private set; } = new();
        private List<UiLayer> _layersByZOrder = new List<UiLayer>();

        public UiLayer? CurrentLayer
        {
            get
            {
                if (ActiveLayers.Count == 0)
                {
                    return null;
                }
                return ActiveLayers.Last();
            }
        }

        public IList<UiLayer> ActiveLayers => Layers;

        public bool IsInActiveLayerList(UiLayer layer) => Layers.Contains(layer);

        public void FrameUpdate(FrameEventArgs frame)
        {
            for (int i = 0; i < Layers.Count; i++)
            {
                Layers[i].Update(frame.DeltaSeconds);
            }
        }

        public void DrawLayers()
        {
            for (int i = 0; i < _layersByZOrder.Count; i++)
            {
                _layersByZOrder[i].Draw();
            }
        }

        public void PushLayer(UiLayer layer, bool noHaltInput = false)
        {
            layer.LayerUIScale = _graphics.WindowScale;
            ResizeAndLayoutLayer(layer);
            Layers.Add(layer);
            SortLayers();

            if (!noHaltInput)
                _inputManager.HaltInput();

            ControlFocused = null;
            KeyboardFocused = null;
            CurrentlyHovered = null;

            CurrentLayer?.GrabFocus();
        }

        public void PopLayer(UiLayer layer, bool noHaltInput = false)
        {
            Layers.Remove(layer);
            SortLayers();

            if (!noHaltInput)
                _inputManager.HaltInput();

            ControlFocused = null;
            KeyboardFocused = null;
            CurrentlyHovered = null;

            CurrentLayer?.GrabFocus();
        }

        public bool IsQuerying(UiLayer layer)
        {
            return Layers.Count() != 0 && CurrentLayer == layer;
        }

        private void SortLayers()
        {
            _layersByZOrder = this.Layers.OrderBy(x => x.ZOrder).ToList();
        }

        private void HandleWindowResized(WindowResizedEventArgs args)
        {
            foreach (var layer in this.Layers)
            {
                ResizeAndLayoutLayer(layer);
            }
        }

        private void HandleUpdateApplication(HotReloadEventArgs args)
        {
            foreach (var layer in this.Layers)
            {
                ResizeAndLayoutLayer(layer);
            }

            if (args.UpdatedTypes != null)
            {
                foreach (var type in args.UpdatedTypes)
                {
                    if (typeof(IUiLayer).IsAssignableFrom(type))
                    {
                        Logger.InfoS("ui", $"Reinjecting dependencies for hotloaded layer {type}.");
                        EntitySystem.InjectDependencies(type);
                    }
                }
            }
        }

        private static void ResizeAndLayoutLayer(UiLayer layer)
        {
            UiHelpers.AddChildrenFromAttributesRecursive(layer);
            layer.GetPreferredBounds(out var bounds);
            layer.SetSize(bounds.Width, bounds.Height);
            layer.SetPosition(bounds.Left, bounds.Top);
        }

        private void HandleWindowScaleChanged(WindowScaleChangedEventArgs evt)
        {
            var ev = new GUIScaleChangedEventArgs(globalUIScale: evt.UIScale);

            foreach (var layer in this.Layers)
            {
                layer.LayerUIScale = evt.UIScale;

                NotifyUIScaleChanged(layer, ev);

                layer.GetPreferredBounds(out var bounds);
                layer.SetSize(bounds.Width, bounds.Height);
                layer.SetPosition(bounds.Left, bounds.Top);
            }
        }

        private void NotifyUIScaleChanged(UiElement elem, GUIScaleChangedEventArgs ev)
        {
            foreach (var child in elem.Children)
            {
                NotifyUIScaleChanged(child, ev);
            }

            elem.UIScaleChanged(ev);
        }

        public TLayer CreateLayer<TLayer, TArgs, TResult>(TArgs args)
            where TLayer : IUiLayerWithResult<TArgs, TResult>, new()
            where TResult : class
        {
            var layer = Activator.CreateInstance<TLayer>()!;
            InitializeLayer<TLayer, TArgs, TResult>(layer, args);
            return layer;
        }

        public TLayer CreateLayer<TLayer, TResult>()
            where TLayer : IUiLayerWithResult<UINone, TResult>, new()
            where TResult : class
        {
            return CreateLayer<TLayer, UINone, TResult>(new());
        }

        public TLayer CreateLayer<TLayer, TArgs>(TArgs args)
            where TLayer : IUiLayerWithResult<TArgs, UINone>, new()
        {
            return CreateLayer<TLayer, TArgs, UINone>(args);
        }

        public TLayer CreateLayer<TLayer>()
            where TLayer : IUiLayer, new()
        {
            var layer = Activator.CreateInstance<TLayer>()!;
            InitializeLayer(layer);
            return layer;
        }

        public void InitializeLayer<TLayer, TArgs, TResult>(TLayer layer, TArgs args)
            where TLayer : IUiLayerWithResult<TArgs, TResult>
            where TResult : class
        {
            InitializeLayer(layer);
            layer.Initialize(args);
        }

        public void InitializeLayer<TLayer>(TLayer layer)
            where TLayer : IUiLayer
        {
            if (layer is UiElement elem)
                UiHelpers.AddChildrenFromAttributesRecursive(elem);

            EntitySystem.InjectDependencies(layer);
            if (!layer.IsLocalized)
                layer.Localize();
        }

        public UiResult<TResult> Query<TLayer, TResult>(QueryLayerArgs? queryArgs = null)
            where TLayer : IUiLayerWithResult<UINone, TResult>, new()
            where TResult : class
        {
            return Query<TLayer, UINone, TResult>(new UINone(), queryArgs);
        }

        public UiResult<UINone> Query<TLayer, TArgs>(TArgs args, QueryLayerArgs? queryArgs = null)
            where TLayer : IUiLayerWithResult<TArgs, UINone>, new()
        {
            return Query<TLayer, TArgs, UINone>(args, queryArgs);
        }

        public UiResult<UINone> Query<TLayer>(QueryLayerArgs? queryArgs = null)
            where TLayer : IUiLayerWithResult<UINone, UINone>, new()
        {
            return Query<TLayer, UINone, UINone>(new UINone(), queryArgs);
        }

        public UiResult<TResult> Query<TLayer, TArgs, TResult>(TArgs args, QueryLayerArgs? queryArgs = null)
            where TLayer : IUiLayerWithResult<TArgs, TResult>, new()
            where TResult : class
        {
            using (var layer = CreateLayer<TLayer, TArgs, TResult>(args))
            {
                return Query(layer, queryArgs);
            }
        }

        public UiResult<TResult> Query<TResult, TLayer, TArgs>(TLayer layer, TArgs args, QueryLayerArgs? queryArgs = null)
            where TLayer : IUiLayerWithResult<TArgs, TResult>
            where TResult : class
        {
            InitializeLayer<TLayer, TArgs, TResult>(layer, args);
            return Query(layer, queryArgs);
        }

        public UiResult<TResult> Query<TArgs, TResult>(IUiLayerWithResult<TArgs, TResult> layer, QueryLayerArgs? queryArgs = null)
            where TResult : class
        {
            if (layer.DefaultZOrder != null)
            {
                layer.ZOrder = layer.DefaultZOrder.Value;
            }
            else
            {
                layer.ZOrder = (CurrentLayer?.ZOrder ?? 0) + 1000;
            }

            layer.Result = null;
            layer.WasCancelled = false;

            var baseLayer = (UiLayer)layer;

            PushLayer(baseLayer, queryArgs?.NoHaltInput ?? false);

            UiResult<TResult>? result = null;

            layer.OnQuery();

            while (result == null)
            {
                try
                {
                    var dt = Love.Timer.GetDelta();
                    var frameArgs = new FrameEventArgs(dt);
                    _gameController.Update(frameArgs);
                    result = layer.GetResult();
                    if (result != null)
                    {
                        break;
                    }

                    _gameController.Draw();
                    _gameController.SystemStep();
                }
                catch (Exception ex)
                {
                    Logger.ErrorS("ui.layer", ex, $"Error during {this.GetType().Name}.Query()");

                    if (layer.ExceptionTolerance)
                    {
                        var errorResult = Query<ErrorHandlerLayer, ErrorHandlerLayer.Args, ErrorHandlerLayer.Result>(new(ex));

                        var action = ErrorHandlerAction.PopLayer;
                        if (errorResult.HasValue)
                            action = errorResult.Value.Action;

                        switch (action)
                        {
                            case ErrorHandlerAction.Continue:
                                break;
                            case ErrorHandlerAction.PopLayer:
                            default:
                                result = new UiResult<TResult>.Error(ex);
                                break;
                        }
                    }
                    else
                    {
                        result = new UiResult<TResult>.Error(ex);
                    }
                }
            }

            PopLayer((UiLayer)layer, queryArgs?.NoHaltInput ?? false);

            // layer.HaltInput();
            layer.OnQueryFinish();

            return result;
        }
    }
}