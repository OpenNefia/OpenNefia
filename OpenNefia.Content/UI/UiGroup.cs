using OpenNefia.Content.Inventory;
using OpenNefia.Content.UI.Element;
using OpenNefia.Content.UI.Element.Containers;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Layer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Maths;
using OpenNefia.Content.UI.Hud;
using OpenNefia.Core.Locale;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.Audio;

namespace OpenNefia.Content.UI
{
    public class UiGroupArgs<TLayer, TArgs>
        where TArgs : notnull
    {
        public Dictionary<TArgs, TLayer> Layers = new();
        public TArgs SelectedArgs = default!;
    }

    public class GroupableUiLayer<TArgs, TResult> : UiLayerWithResult<TArgs, TResult>
        where TResult : class
    {
        public GroupableUiLayer()
        {

        }
    }

    public class UiGroup<TLayer, TArgs, TLayerArgs, TResult> : UiLayerWithResult<TArgs, TResult>
        where TArgs : UiGroupArgs<TLayer, TLayerArgs>
        where TLayerArgs : notnull
        where TResult : class
        where TLayer : GroupableUiLayer<TLayerArgs, TResult>, new()
    {
        private Dictionary<TLayerArgs, TLayer> Layers = new();
        private Dictionary<TLayerArgs, IUiText> Texts = new();
        private Dictionary<TLayerArgs, (IAssetDrawable Elem, Vector2i Offset)> Icons = new();
        private UiTopicWindow TabWindow = default!;
        private IAssetInstance TabDeco;

        private TLayerArgs SelectedArgs = default!;
        private TLayer SelectedLayer = default!;

        private const int ItemWidth = 50;
        private const int ExtraTabWindowWidth = 30;

        public UiGroup()
        {
            TabWindow = new UiTopicWindow(UiTopicWindow.FrameStyleKind.Three, UiTopicWindow.WindowStyleKind.Four);
            OnKeyBindDown += OnKeyDown;
            TabDeco = Assets.Get(Protos.Asset.RadarDeco);
        }

        public override void Initialize(TArgs args)
        {
            base.Initialize(args);
            SelectedArgs = args.SelectedArgs;
            Layers = args.Layers;
            Texts = Layers.ToDictionary(x => x.Key, y => (IUiText)new UiTextOutlined(UiFonts.HUDTabText, GetText(y.Key)));
            Icons = Layers.ToDictionary(x => x.Key, y => GetIcon(y.Key));
            ShowSelectedLayer();
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            SelectedLayer.GrabFocus();
        }

        private void OnKeyDown(Core.UI.Element.GUIBoundKeyEventArgs args)
        {
            var queryArgs = Layers.Keys.ToList();
            var index = queryArgs.IndexOf(SelectedArgs);
            if (args.Function == EngineKeyFunctions.UINextTab)
            {
                index++;
                SelectedArgs = queryArgs[index >= queryArgs.Count ? 0 : index];
                ShowSelectedLayer();
                args.Handle();
            }
            else if (args.Function == EngineKeyFunctions.UIPreviousTab)
            {
                index--;
                SelectedArgs = queryArgs[index < 0 ? queryArgs.Count - 1 : index];
                ShowSelectedLayer();
                args.Handle();
            }
        }

        protected virtual void ShowSelectedLayer()
        {
            if (SelectedLayer != null)
            {
                RemoveChild(SelectedLayer);
                SelectedLayer.OnKeyBindDown -= OnKeyDown;
                SelectedLayer.OnQueryFinish();
            }

            if (Layers.TryGetValue(SelectedArgs, out var layer))
            {
                SelectedLayer = layer;
                AddChild(SelectedLayer);
                EntitySystem.InjectDependencies(SelectedLayer);
                layer.Initialize(SelectedArgs);
                SetSize(Width, Height);
                SetPosition(X, Y);
                SelectedLayer.OnKeyBindDown += OnKeyDown;
                SelectedLayer.OnQuery();
                SelectedLayer.GrabFocus();
            }
        }

        protected virtual (IAssetDrawable Elem, Vector2i Offset) GetIcon(TLayerArgs args) => (default!, default);

        protected virtual string GetText(TLayerArgs args) => string.Empty;

        public override void SetSize(int width, int height)
        {
            base.SetSize(width, height);
            TabWindow.SetSize(ExtraTabWindowWidth + (Layers.Count * ItemWidth), 24);
            if (SelectedLayer != null)
                SelectedLayer.SetPreferredSize();
        }

        public override void SetPosition(int x, int y)
        {
            base.SetPosition(x, y);
            TabWindow.SetPosition(Width - TabWindow.Width - 20, 32);
            int tabInfoX = TabWindow.X + (ItemWidth / 2);
            int tabInfoY = TabWindow.Y + 5;
            foreach (var layer in Layers)
            {
                var iconOffset = Icons[layer.Key].Offset;
                Icons[layer.Key].Elem.SetPosition(tabInfoX + iconOffset.X, tabInfoY + iconOffset.Y);
                Texts[layer.Key].SetPosition(tabInfoX, tabInfoY);
                tabInfoX += ItemWidth;
            }
            if (SelectedLayer != null)
            {
                UiUtils.GetCenteredParams(SelectedLayer.Width, SelectedLayer.Height, out var bounds);
                SelectedLayer.SetPosition(bounds.Left, bounds.Top);
            }
        }

        public override void Draw()
        {
            base.Draw();
            TabWindow.Draw();
            TabDeco.Draw(TabWindow.X - 32, TabWindow.Y - 7);
            foreach(var layer in Layers)
            {
                var color = (layer.Value == SelectedLayer) ? Color.White : Color.Gray;
                Icons[layer.Key].Elem.Color = color;
                Icons[layer.Key].Elem.Draw();
                Texts[layer.Key].Color = color;
                Texts[layer.Key].Draw();
            }
            GraphicsEx.SetColor(Color.White);
            if (SelectedLayer != null)
                SelectedLayer.Draw();
        }

        public override void Update(float dt)
        {
            base.Update(dt);
            TabWindow.Update(dt);
            if (SelectedLayer != null)
                SelectedLayer.Update(dt);
        }

        public override UiResult<TResult>? GetResult()
        {
            var res = SelectedLayer?.GetResult();
            if (res != null)
                SelectedLayer?.OnQueryFinish();
            return res;
        }
    }
}
