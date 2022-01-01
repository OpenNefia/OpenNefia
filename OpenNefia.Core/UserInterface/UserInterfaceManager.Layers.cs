using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Log;
using OpenNefia.Core.Timing;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public bool IsInActiveLayerList(UiLayer layer) => this.Layers.Contains(layer);

        public void UpdateLayers(FrameEventArgs frame)
        {
            for (int i = 0; i < this.Layers.Count; i++)
            {
                this.Layers[i].Update(frame.DeltaSeconds);
            }
        }

        public void DrawLayers()
        {
            for (int i = 0; i < _layersByZOrder.Count; i++)
            {
                _layersByZOrder[i].Draw();
            }
        }

        public void PushLayer(UiLayer layer)
        {
            layer.GetPreferredBounds(out var bounds);
            layer.SetSize(bounds.Width, bounds.Height);
            layer.SetPosition(bounds.Left, bounds.Top);
            Layers.Add(layer);
            SortLayers();

            _inputManager.HaltInput();
            ControlFocused = null;
            KeyboardFocused = null;
            CurrentlyHovered = null;

            CurrentLayer?.OnFocused();
        }

        public void PopLayer(UiLayer layer)
        {
            Layers.Remove(layer);
            SortLayers();

            _inputManager.HaltInput();
            ControlFocused = null;
            KeyboardFocused = null;
            CurrentlyHovered = null;

            CurrentLayer?.OnFocused();
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
                layer.GetPreferredBounds(out var bounds);
                layer.SetSize(bounds.Width, bounds.Height);
                layer.SetPosition(bounds.Left, bounds.Top);
            }
        }

        public UiResult<TResult> Query<TLayer, TResult>()
            where TLayer : IUiLayerWithResult<UINone, TResult>, new()
            where TResult : class
        {
            return Query<TLayer, UINone, TResult>(new UINone());
        }

        public UiResult<UINone> Query<TLayer, TArgs>(TArgs args) 
            where TLayer : IUiLayerWithResult<TArgs, UINone>, new()
        {
            return Query<TLayer, TArgs, UINone>(args);
        }

        public UiResult<UINone> Query<TLayer>() 
            where TLayer : IUiLayerWithResult<UINone, UINone>, new()
        {
            return Query<TLayer, UINone, UINone>(new UINone());
        }

        public UiResult<TResult> Query<TLayer, TArgs, TResult>(TArgs args)
            where TLayer: IUiLayerWithResult<TArgs, TResult>, new()
            where TResult : class
        {
            using (var layer = Activator.CreateInstance<TLayer>()!)
            {
                EntitySystem.InjectDependencies(layer);
                layer.Initialize(args);
                return Query(layer);
            }
        }

        public UiResult<TResult> Query<TArgs, TResult>(IUiLayerWithResult<TArgs, TResult> layer) 
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

            if (!layer.IsLocalized)
            {
                layer.Localize(layer.GetType().GetBaseLocaleKey());
            }

            layer.Result = null;
            layer.WasCancelled = false;

            var baseLayer = (UiLayer)layer;

            PushLayer(baseLayer);

            UiResult<TResult>? result;

            try
            {
                layer.OnQuery();

                while (true)
                {
                    var dt = Love.Timer.GetDelta();
                    var frameArgs = new FrameEventArgs(dt);
                    // layer.RunKeyActions(frameArgs);
                    _gameController.Update(frameArgs);
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
                Logger.ErrorS("ui.layer", ex, $"Error during {this.GetType().Name}.Query()");
                result = new UiResult<TResult>.Error(ex);
            }
            finally
            {
                PopLayer((UiLayer)layer);
            }

            // layer.HaltInput();
            layer.OnQueryFinish();

            return result;
        }
    }
}
