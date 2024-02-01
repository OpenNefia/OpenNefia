using OpenNefia.Core.EngineVariables;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.HotReload;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
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

        [EngineVariable("Core.DebugUILayers")]
        public bool DebugUILayers { get; set; } = false;

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
                var layer = _layersByZOrder[i];
                layer.Draw();

                if (DebugUILayers)
                    DrawDebug(layer);
            }

            if (DebugUILayers)
            {
                var uiScale = _config.GetCVar(CVars.DisplayUIScale);
                Love.Graphics.SetColor(Color.Black.WithAlphaB(192));
                GraphicsS.RectangleS(uiScale, Love.DrawMode.Fill, 0, 0, 400, 60);
                Love.Graphics.SetColor(Color.White);
                GraphicsS.PrintS(uiScale, $"{nameof(CurrentlyHovered)}: {CurrentlyHovered?.GetType()}", 4, 4);
                GraphicsS.PrintS(uiScale, $"{nameof(ControlFocused)}: {ControlFocused?.GetType()}", 4, 24);
                GraphicsS.PrintS(uiScale, $"{nameof(KeyboardFocused)}: {KeyboardFocused?.GetType()}", 4, 44);
            }
        }

        private void DrawDebug(UiLayer layer)
        {
            var queue = new Queue<UiElement>();
            queue.Enqueue(layer);

            while (queue.Count > 0)
            {
                var element = queue.Dequeue();

                if (!element.Visible)
                    continue;

                foreach (var child in element.Children)
                    queue.Enqueue(child);

                var lineWidth = 1;
                var color = Color.Red;
                if (ControlFocused == element)
                {
                    lineWidth = 3;
                    color = Color.Cyan;
                }
                else if (KeyboardFocused == element)
                {
                    lineWidth = 3;
                    color = Color.Orange;
                }
                if (CurrentlyHovered == element)
                {
                    lineWidth = 3;
                    color = Color.Yellow;
                }
                if (element is UiLayer && CurrentLayer == element)
                    color = Color.Purple;

                Love.Graphics.SetLineWidth(lineWidth);
                Love.Graphics.SetColor(color);
                Love.Graphics.Rectangle(Love.DrawMode.Line, element.PixelRect);
            }
        }

        public void PushLayer(UiLayer layer, bool noHaltInput = false)
        {
            layer.LayerUIScale = _graphics.WindowScale;
            layer.LayerTileScale = _config.GetCVar(CVars.DisplayTileScale);
            AddChildrenResizeAndLayoutLayer(layer);
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
                AddChildrenResizeAndLayoutLayer(layer);
            }
        }

        private void HandleUpdateApplication(HotReloadEventArgs args)
        {
            foreach (var layer in this.Layers)
            {
                AddChildrenResizeAndLayoutLayer(layer);
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

        private static void AddChildrenResizeAndLayoutLayer(UiLayer layer)
        {
            UiHelpers.AddChildrenFromAttributesRecursive(layer);
            ResizeAndLayoutLayer(layer);
        }

        private static void ResizeAndLayoutLayer(UiLayer layer)
        {
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

                ResizeAndLayoutLayer(layer);
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

        private void HandleTileScaleChanged(float scale)
        {
            foreach (var layer in this.Layers)
            {
                layer.LayerTileScale = scale;

                NotifyTileScaleChanged(layer, scale);

                ResizeAndLayoutLayer(layer);
            }
        }

        private void NotifyTileScaleChanged(UiElement elem, float scale)
        {
            foreach (var child in elem.Children)
            {
                NotifyTileScaleChanged(child, scale);
            }

            // elem.TileScaleChanged(ev);
        }

        public TLayer CreateAndInitializeLayer<TLayer, TArgs, TResult>(TArgs args)
            where TLayer : IUiLayerWithResult<TArgs, TResult>, new()
            where TResult : class
        {
            var layer = Activator.CreateInstance<TLayer>()!;
            InitializeLayer<TLayer, TArgs, TResult>(layer, args);
            return layer;
        }

        public TLayer CreateAndInitializeLayer<TLayer, TResult>()
            where TLayer : IUiLayerWithResult<UINone, TResult>, new()
            where TResult : class
        {
            return CreateAndInitializeLayer<TLayer, UINone, TResult>(new());
        }

        public TLayer CreateAndInitializeLayer<TLayer, TArgs>(TArgs args)
            where TLayer : IUiLayerWithResult<TArgs, UINone>, new()
        {
            return CreateAndInitializeLayer<TLayer, TArgs, UINone>(args);
        }

        public TLayer CreateAndInitializeLayer<TLayer>()
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
            using (var layer = CreateAndInitializeLayer<TLayer, TArgs, TResult>(args))
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

            layer.WasCancelled = false;

            var baseLayer = (UiLayer)layer;

            PushLayer(baseLayer, queryArgs?.NoHaltInput ?? false);

            UiResult<TResult>? result = layer.GetResult();

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

            layer.Result = null;

            return result;
        }
    }
}