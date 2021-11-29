
using OpenNefia.Core.GameController;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;

namespace OpenNefia.Core.UI
{
    public class UiLayerManager : IUiLayerManager
    {
        [Dependency] private readonly IGraphics _graphics = default!;
        [Dependency] private readonly IGameController _gameController = default!;

        internal List<IUiLayer> Layers { get; private set; } = new();
        private List<IUiLayer> _layersByZOrder = new List<IUiLayer>();

        public void Initialize()
        {
            _graphics.OnWindowResized += HandleWindowResized;

            _graphics.OnKeyPressed += HandleKeyPressed;
            _graphics.OnKeyReleased += HandleKeyReleased;
            _graphics.OnTextInput += HandleTextInput;
            _graphics.OnMouseMoved += HandleMouseMoved;
            _graphics.OnMousePressed += HandleMousePressed;
            _graphics.OnMouseReleased += HandleMouseReleased;
        }

        public void Shutdown()
        {
            _graphics.OnWindowResized -= HandleWindowResized;

            _graphics.OnKeyPressed -= HandleKeyPressed;
            _graphics.OnKeyReleased -= HandleKeyReleased;
            _graphics.OnTextInput -= HandleTextInput;
            _graphics.OnMouseMoved -= HandleMouseMoved;
            _graphics.OnMousePressed -= HandleMousePressed;
            _graphics.OnMouseReleased -= HandleMouseReleased;
        }

        #region Graphics Events

        private void HandleWindowResized(WindowResizedEventArgs args)
        {
            foreach (var layer in this.Layers)
            {
                layer.GetPreferredBounds(out var bounds);
                layer.SetSize(bounds.Size);
                layer.SetPosition(bounds.TopLeft);
            }
        }

        public void HandleKeyPressed(KeyPressedEventArgs args)
        {
            CurrentLayer?.ReceiveKeyPressed(args);
        }

        public void HandleKeyReleased(KeyPressedEventArgs args)
        {
            CurrentLayer?.ReceiveKeyReleased(args);
        }

        public void HandleTextInput(TextInputEventArgs args)
        {
            CurrentLayer?.ReceiveTextInput(args);
        }

        public void HandleMouseMoved(MouseMovedEventArgs args)
        {
            CurrentLayer?.ReceiveMouseMoved(args);
        }

        public void HandleMousePressed(MousePressedEventArgs args)
        {
            CurrentLayer?.ReceiveMousePressed(args);
        }

        public void HandleMouseReleased(MousePressedEventArgs args)
        {
            CurrentLayer?.ReceiveMouseReleased(args);
        }

        #endregion

        public IUiLayer? CurrentLayer 
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

        public IList<IUiLayer> ActiveLayers => Layers;

        public bool IsInActiveLayerList(IUiLayer layer) => this.Layers.Contains(layer);

        public void UpdateLayers(float dt)
        {
            for (int i = 0; i < this.Layers.Count; i++)
            {
                this.Layers[i].Update(dt);
            }
        }

        public void DrawLayers()
        {
            for (int i = 0; i < _layersByZOrder.Count; i++)
            {
                _layersByZOrder[i].Draw();
            }
        }

        public void PushLayer(IUiLayer layer)
        {
            layer.GetPreferredBounds(out var bounds);
            layer.SetSize(bounds.Size);
            layer.SetPosition(bounds.TopLeft);
            Layers.Add(layer);
            SortLayers();
        }

        public void PopLayer(IUiLayer layer)
        {
            Layers.Remove(layer);
            SortLayers();
        }

        public bool IsQuerying(IUiLayer layer)
        {
            return Layers.Count() != 0 && CurrentLayer == layer;
        }

        private void SortLayers()
        {
            _layersByZOrder = this.Layers.OrderBy(x => x.ZOrder).ToList();
        }

        public UiResult<T> Query<T>(IUiLayerWithResult<T> layer) where T : class
        {
            if (layer.DefaultZOrder != null)
            {
                layer.ZOrder = layer.DefaultZOrder.Value;
            }
            else
            {
                layer.ZOrder = (CurrentLayer?.ZOrder ?? 0) + 1000;
            }

            CurrentLayer?.HaltInput();

            if (!layer.IsLocalized)
            {
                layer.Localize(this.GetType().GetBaseLocaleKey());
            }

            PushLayer(layer);

            UiResult<T>? result;

            try
            {
                // Global REPL hotkey
                layer.Keybinds[Keys.Backquote] += (_) =>
                {
                    //var repl = Current.Game.Repl.Value;
                    //if (!repl.IsInActiveLayerList())
                    //    repl.Query();
                };

                layer.ApplyTheme();
                layer.OnQuery();

                while (true)
                {
                    var dt = Love.Timer.GetDelta();
                    layer.RunKeyActions(dt);
                    _gameController.Update(dt);
                    result = layer.GetResult();
                    if (result != null)
                    {
                        break;
                    }

                    _gameController.Draw();
                    _gameController.SystemStep();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, $"Error during {this.GetType().Name}.Query()", ex);
                result = new UiResult<T>.Error(ex);
            }
            finally
            {
                PopLayer(layer);
            }

            layer.HaltInput();
            layer.OnQueryFinish();

            return result;
        }
    }
}
