using OpenNefia.Content.UI.Element;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Graphics;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Layer;
using OpenNefia.Core.UI.Element;
using OpenNefia.Content.UI.Element.Containers;
using OpenNefia.Core.Input;
using OpenNefia.Core.Maths;
using OpenNefia.Content.World;
using OpenNefia.Core.GameObjects;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Game;
using OpenNefia.Content.Charas;
using OpenNefia.Content.Skills;
using OpenNefia.Content.Levels;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Content.Equipment;
using OpenNefia.Core.Stats;
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.Currency;
using OpenNefia.Content.Hud;
using System.Diagnostics.CodeAnalysis;
using static OpenNefia.Content.Hud.HudAttributeWidget;
using ICSharpCode.Decompiler.Semantics;
using OpenNefia.Core.Configuration;
using OpenNefia.Core;
using OpenNefia.Core.EngineVariables;

namespace OpenNefia.Content.UI.Hud
{
    [Flags]
    public enum WidgetDrawFlags
    {
        Never = 0,
        Normal = 1 << 0,
        Backlog = 1 << 1,
        Always = Normal + Backlog,
    }

    public enum WidgetAnchor
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
    }

    public sealed class WidgetInstance
    {
        public BaseHudWidget Widget { get; }
        public Vector2 Position { get; set; }
        public WidgetDrawFlags DrawFlags { get; set; }
        public WidgetAnchor Anchor { get; set; }

        public WidgetInstance(BaseHudWidget widget, WidgetAnchor anchor = default, Vector2 position = default, Func<Vector2> size = default!,
            WidgetDrawFlags flags = WidgetDrawFlags.Always)
        {
            Widget = widget;
            Widget.Initialize();
            Position = position;
            DrawFlags = flags;
            Anchor = anchor;
        }
    }

    public class HudLayer : UiLayer, IHudLayer, IBacklog
    {
        [Dependency] private readonly IFieldLayer _field = default!;
        [Dependency] private readonly IGraphics _graphics = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;

        private List<WidgetInstance> Widgets = new();

        public HudMessageBoxWidget HudMessageWindow { get; private set; } = default!;

        public UIBox2 GameBounds => new(0, 0, _graphics.WindowSize.X, _graphics.WindowSize.Y - HudMinimapWidget.MinimapHeight);
        public UIBox2 GamePixelBounds => new(0, 0, _graphics.WindowPixelSize.X, _graphics.WindowPixelSize.Y - HudMinimapWidget.MinimapHeight * UIScale);
        public IBacklog Backlog => HudMessageWindow;
        public IHudMessageWindow MessageWindow => HudMessageWindow;
        public bool IsShowingBacklog => HudMessageWindow.IsShowingBacklog;

        [Child] private UiFpsCounter FpsCounter;
        [Child] private UiMessageWindowBacking MessageBoxBacking = default!;
        [Child] private UiMessageWindowBacking BacklogBacking = default!;
        [Child] private UiHudBar HudBar = default!;

        [EngineVariable("Elona.DebugHudWidgetBounds")]
        public bool DebugHudWidgetBounds { get; set; } = false;

        public const int HudZOrder = 20000000;

        public HudLayer()
        {
            IoCManager.InjectDependencies(this);

            FpsCounter = new UiFpsCounter();
            MessageBoxBacking = new UiMessageWindowBacking();
            BacklogBacking = new UiMessageWindowBacking(UiMessageWindowBacking.MessageBackingType.Expanded);
            HudBar = new UiHudBar();
        }

        public void Initialize()
        {
            CanKeyboardFocus = true;

            foreach (var widget in Widgets)
            {
                RemoveChild(widget.Widget);
            }
            Widgets.Clear();

            AddDefaultWidgets();

            // This is so the widgets will have the correct UI scaling.
            foreach (var widget in Widgets)
            {
                UiHelpers.AddChildrenRecursive(this, widget.Widget);
                EntitySystem.InjectDependencies(widget.Widget);
            }

            _field.OnScreenRefresh += RefreshWidgets;
        }

        public void RefreshWidgets()
        {
            foreach (var widget in Widgets)
            {
                widget.Widget.RefreshWidget();
            }
            UpdateWidgetPositions();
        }

        private void AddDefaultWidgets()
        {
            HudMessageWindow = new HudMessageBoxWidget();

            Widgets.Add(new(HudMessageWindow, WidgetAnchor.BottomLeft,
                new(HudMinimapWidget.MinimapWidth + 25, -84 + Constants.INF_MSGH),
                () => new(Width - HudMinimapWidget.MinimapWidth - 75,
                0)));

            Widgets.Add(new(new HudMinimapWidget(), WidgetAnchor.BottomLeft, new(0, 0)));
            Widgets.Add(new(new HudExpWidget(), WidgetAnchor.BottomLeft, new(5, -104)));
            Widgets.Add(new(new HudAreaNameWidget(), WidgetAnchor.BottomLeft, new(HudMinimapWidget.MinimapWidth + 18, -17)));
            Widgets.Add(new(new HudDateWidget(), WidgetAnchor.TopLeft, new(80, 8)));
            Widgets.Add(new(new HudClockWidget(), WidgetAnchor.TopLeft, new(0, 0)));

            var iconX = 285;
            foreach (HudSkillIconType type in Enum.GetValues<HudSkillIconType>())
            {
                if (type > HudSkillIconType.Cha)
                    iconX += 5;
                Widgets.Add(new(new HudAttributeWidget(type), WidgetAnchor.BottomLeft, new(iconX, -17)));
                iconX += 47;
            }

            Widgets.Add(new(new HudHPBarWidget(), WidgetAnchor.BottomLeft, new(260, -93), flags: WidgetDrawFlags.Normal));
            Widgets.Add(new(new HudMountHPBarWidget(), WidgetAnchor.BottomLeft, new(260, -93 - 22), flags: WidgetDrawFlags.Normal));
            Widgets.Add(new(new HudMPBarWidget(), WidgetAnchor.BottomLeft, new(400, -93), flags: WidgetDrawFlags.Normal));
            Widgets.Add(new(new HudStaminaBarWidget(), WidgetAnchor.BottomLeft, new(260 - 120, -93), flags: WidgetDrawFlags.Normal));

            Widgets.Add(new(new HudGoldWidget(), WidgetAnchor.BottomRight, new(-220, -104), flags: WidgetDrawFlags.Normal));
            Widgets.Add(new(new HudPlatinumWidget(), WidgetAnchor.BottomRight, new(-90, -104), flags: WidgetDrawFlags.Normal));

            Widgets.Add(new(new HudAutoTurnWidget(), WidgetAnchor.BottomRight, new(-156, -55), flags: WidgetDrawFlags.Never));
            Widgets.Add(new(new HudStatusIndicators(), WidgetAnchor.BottomLeft, new(8, -118)));
            Widgets.Add(new(new HudBuffIconsWidget(), WidgetAnchor.BottomRight, new(-8, -8 -HudMinimapWidget.MinimapHeight)));
        }

        public bool TryGetWidget<T>([NotNullWhen(true)] out T? widget, [NotNullWhen(true)] out WidgetInstance? instance)
            where T : class, IHudWidget
        {
            foreach (var other in Widgets)
            {
                if (other.Widget is T widgetT)
                {
                    widget = widgetT;
                    instance = other;
                    return true;
                }
            }

            widget = null;
            instance = null;
            return false;
        }

        public bool TryGetWidget<T>([NotNullWhen(true)] out T? widget)
            where T : class, IHudWidget
            => TryGetWidget(out widget, out _);

        public bool TryGetWidgetInstance<T>([NotNullWhen(true)] out WidgetInstance? instance)
            where T : class, IHudWidget
            => TryGetWidget<T>(out _, out instance);

        public override void SetSize(float width, float height)
        {
            // TODO remove
            LayerUIScale = _graphics.WindowScale;
            LayerTileScale = _config.GetCVar(CVars.DisplayTileScale);

            base.SetSize(width, height);

            FpsCounter.SetSize(400, 500);
            MessageBoxBacking.SetSize(width + 200, 72);
            BacklogBacking.SetSize(width + 200, 600);
            HudBar.SetSize(Width + 200, UiHudBar.HudBarHeight);

            foreach (var widget in Widgets)
            {
                widget.Widget.SetPreferredSize();
            }
        }

        public override void SetPosition(float x, float y)
        {
            base.SetPosition(x, y);

            UpdateWidgetPositions();

            FpsCounter.Update(0); // so that TextWidth is available
            FpsCounter.SetPosition(Width - FpsCounter.Text.TextWidth - 5, 5);
            MessageBoxBacking.SetPosition(0, Height - HudMinimapWidget.MinimapHeight);
            HudBar.SetPosition(0, Height - 18);
            BacklogBacking.SetPosition(127, Height - 467);
        }

        private void UpdateWidgetPositions()
        {
            foreach (var widget in Widgets)
            {
                Vector2 anchor = widget.Anchor switch
                {
                    WidgetAnchor.BottomLeft => new(0, Height - widget.Widget.Height),
                    WidgetAnchor.BottomRight => new(Width - widget.Widget.Width, Height - widget.Widget.Height),
                    WidgetAnchor.TopRight => new(Width - widget.Widget.Width, 0),
                    _ => new(0, 0),
                };
                widget.Widget.SetPosition(anchor.X + widget.Position.X, anchor.Y + widget.Position.Y);
            }
        }

        public override void Update(float dt)
        {
            // TODO make these into widgets
            FpsCounter.Update(dt);
            MessageBoxBacking.Update(dt);
            BacklogBacking.Update(dt);
            HudBar.Update(dt);

            foreach (var widget in Widgets)
            {
                widget.Widget.Update(dt);
            }
        }

        public override void Draw()
        {
            GraphicsEx.SetColor(Color.White);

            if (IsShowingBacklog)
            {
                BacklogBacking.Draw();
            }

            MessageBoxBacking.Draw();
            HudBar.Draw();
            HudMessageWindow.Draw();

            foreach (var widget in Widgets)
            {
                if (!widget.DrawFlags.HasFlag(WidgetDrawFlags.Normal))
                    continue;

                if (IsShowingBacklog && !widget.DrawFlags.HasFlag(WidgetDrawFlags.Backlog))
                    continue;

                GraphicsEx.SetColor(Color.White);
                widget.Widget.Draw();

                if (DebugHudWidgetBounds)
                {
                    GraphicsEx.SetColor(Color.Red);
                    GraphicsS.RectangleS(widget.Widget.UIScale, Love.DrawMode.Line, widget.Widget.Rect);
                }
            }

            FpsCounter.Draw();
        }

        public override void Dispose()
        {
            _field.OnScreenRefresh -= RefreshWidgets;
        }

        public void ToggleBacklog(bool visible)
        {
        }

        public void ClearWidgets()
        {
            foreach (var widget in Widgets)
            {
                RemoveChild(widget.Widget);
            }
            Widgets.Clear();
        }
    }
}
