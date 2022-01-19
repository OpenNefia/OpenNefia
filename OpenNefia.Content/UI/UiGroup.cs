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

    public class UiGroup<TLayer, TArgs, TSublayerArgs, TResult> : UiLayerWithResult<TArgs, TResult>
        where TArgs : UiGroupArgs<TLayer, TSublayerArgs>
        where TSublayerArgs : notnull
        where TResult : class
        where TLayer : GroupableUiLayer<TSublayerArgs, TResult>, new()
    {
        private Dictionary<TSublayerArgs, TLayer> Layers = new();
        private Dictionary<TSublayerArgs, UiText> Texts = new();
        private Dictionary<TSublayerArgs, AssetDrawable?> Icons = new();
        
        [Child] private UiTopicWindow TabWindow = default!;
        private IAssetInstance TabDeco;

        private TSublayerArgs SelectedArgs = default!;
        private TLayer SelectedLayer = default!;

        private const float ItemWidth = 50f;
        private const float ExtraTabWindowWidth = 30f;

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
            Texts = Layers.ToDictionary(x => x.Key, y => (UiText)new UiTextOutlined(UiFonts.HUDTabText, GetText(y.Key)));
            Icons = Layers.ToDictionary(x => x.Key, y => GetIcon(y.Key));
            ShowSelectedLayer();
        }

        public override void GrabFocus()
        {
            base.GrabFocus();
            SelectedLayer.GrabFocus();
        }

        private void OnKeyDown(GUIBoundKeyEventArgs args)
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

                UserInterfaceManager.InitializeLayer<TLayer, TSublayerArgs, TResult>(SelectedLayer, SelectedArgs);

                SetSize(Width, Height);
                SetPosition(X, Y);
                SelectedLayer.OnKeyBindDown += OnKeyDown;
                SelectedLayer.OnQuery();
                SelectedLayer.GrabFocus();
            }
        }

        protected virtual AssetDrawable? GetIcon(TSublayerArgs args) => null;

        protected virtual string GetText(TSublayerArgs args) => string.Empty;

        public override void SetSize(float width, float height)
        {
            base.SetSize(width, height);
            TabWindow.SetSize(ExtraTabWindowWidth + (Layers.Count * ItemWidth), 24);
            if (SelectedLayer != null)
                SelectedLayer.SetPreferredSize();
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);
            TabWindow.SetPosition(Width - TabWindow.Width - 20, 32);
            var tabInfoX = TabWindow.X + (ItemWidth / 2);
            var tabInfoY = TabWindow.Y + 5;
            foreach (var layer in Layers)
            {
                Icons[layer.Key]?.SetPosition(tabInfoX, tabInfoY);
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
            TabDeco.Draw(UIScale, TabWindow.X - 32, TabWindow.Y - 7);

            foreach(var layer in Layers)
            {
                var color = (layer.Value == SelectedLayer) ? Color.White : Color.Gray;

                var icon = Icons[layer.Key];
                if (icon != null)
                {
                    icon.Color = color;
                    icon.Draw();
                }

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
