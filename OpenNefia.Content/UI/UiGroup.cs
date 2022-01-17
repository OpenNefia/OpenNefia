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

        public virtual void TabEnter()
        {

        }

        public virtual void TabExit()
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
            switch(args.Function)
            {
                case var next when next == EngineKeyFunctions.UINextTab:
                    index++;
                    SelectedArgs = queryArgs[index >= queryArgs.Count ? 0 : index];
                    ShowSelectedLayer();
                    args.Handle();
                    break;
                case var prev when prev == EngineKeyFunctions.UIPreviousTab:
                    index--;
                    SelectedArgs = queryArgs[index < 0 ? queryArgs.Count - 1 : index];
                    ShowSelectedLayer();
                    args.Handle();
                    break;
            }
        }

        public override void OnQuery()
        {
            base.OnQuery();
        }

        protected virtual void ShowSelectedLayer()
        {
            if (SelectedLayer != null)
            {
                RemoveChild(SelectedLayer);
                SelectedLayer.OnKeyBindDown -= OnKeyDown;
                SelectedLayer.TabExit();
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
                SelectedLayer.TabEnter();
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
                SelectedLayer?.TabExit();
            return res;
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }

    public class JournalGroupUiArgs
    {
        public enum LogTab
        {
            Backlog,
            Journal,
            Chat
        }

        public LogTab Type;

        public JournalGroupUiArgs(LogTab type)
        {
            Type = type;
        }
    }
    
    public class JournalUiLayer : GroupableUiLayer<JournalGroupUiArgs, UINone>
    {
        public JournalUiLayer()
        {
            EventFilter = UIEventFilterMode.Pass;
            CanControlFocus = true;
        }
        protected virtual void OnKeyDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel)
                Cancel();
        }

        public override void TabEnter()
        {
            base.TabEnter();
            OnKeyBindDown += OnKeyDown;
        }

        public override void TabExit()
        {
            base.TabExit();
            OnKeyBindDown -= OnKeyDown;
        }
    }

    public class BacklogUiLayer : JournalUiLayer
    {
        [Dependency] private readonly IHudLayer _hud = default!;

        public override void TabEnter()
        {
            base.TabEnter();
            _hud.ToggleBacklog(true);
            Sounds.Play(Protos.Sound.Log);
        }

        public override void TabExit()
        {
            base.TabExit();
            _hud.ToggleBacklog(false);
        }

        protected override void OnKeyDown(GUIBoundKeyEventArgs args)
        {
            base.OnKeyDown(args);
            if (args.Function == EngineKeyFunctions.UIBacklog)
                Cancel();
        }
    }

    public class JournalUiGroupArgs : UiGroupArgs<JournalUiLayer, JournalGroupUiArgs>
    {
        public JournalUiGroupArgs(JournalGroupUiArgs.LogTab type)
        {
            foreach(JournalGroupUiArgs.LogTab logType in Enum.GetValues(typeof(JournalGroupUiArgs.LogTab)))
            {
                var args = new JournalGroupUiArgs(logType);
                if (logType == type)
                    SelectedArgs = args;
                Layers[args] = logType switch
                {
                    JournalGroupUiArgs.LogTab.Backlog => new BacklogUiLayer(),
                    _ => new JournalUiLayer()
                };
            }
        }
    }

    public class JournalUiGroup : UiGroup<JournalUiLayer, JournalUiGroupArgs, JournalGroupUiArgs, UINone>
    {
        protected override (IAssetDrawable Elem, Vector2i Offset) GetIcon(JournalGroupUiArgs args)
        {
            var icon = args.Type switch
            {
                JournalGroupUiArgs.LogTab.Backlog => InventoryIcon.Log,
                JournalGroupUiArgs.LogTab.Journal => InventoryIcon.Read,
                JournalGroupUiArgs.LogTab.Chat => InventoryIcon.Chat,
                _ => InventoryIcon.Drink
            };
            return (InventoryHelpers.MakeIcon(icon), new(-12, -32));
        }

        protected override string GetText(JournalGroupUiArgs args)
        {
            return Loc.GetString($"Elona.Hud.LogGroup.{args.Type}");
        }
    }
}
