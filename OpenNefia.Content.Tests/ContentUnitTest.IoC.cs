using Moq;
using OpenNefia.Content.DebugView;
using OpenNefia.Content.Hud;
using OpenNefia.Content.Repl;
using OpenNefia.Content.UI.Hud;
using OpenNefia.Content.UI.Layer;
using OpenNefia.Core.Audio;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Rendering;
using OpenNefia.Tests;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Tests
{
    public partial class ContentUnitTest
    {
        public static void RegisterIoC()
        {
            ContentIoC.Register();

            IoCManager.RegisterInstance<IFieldLayer>(new Mock<IFieldLayer>().Object, true);
            IoCManager.RegisterInstance<IHudLayer>(new DummyHudLayer(), true);
            IoCManager.RegisterInstance<IReplLayer>(new Mock<IReplLayer>().Object, true);
            IoCManager.RegisterInstance<IDebugViewLayer>(new Mock<IDebugViewLayer>().Object, true);
            IoCManager.RegisterInstance<IAssetManager>(new Mock<IAssetManager>().Object, true);
        }
    }

    public class DummyHudLayer : DummyLayer, IHudLayer
    {
        public IHudMessageWindow MessageWindow { get; } = new DummyMessageWindow();

        public UIBox2 GameBounds => default!;
        public UIBox2 GamePixelBounds => default!;

        public IBacklog Backlog => default!;

        public void ClearWidgets()
        {
        }

        public void RefreshWidgets()
        {
        }

        public void Initialize()
        {
        }

        public void ToggleBacklog(bool visible)
        {
        }

        public void UpdateMinimap()
        {
        }

        public void UpdateTime()
        {
        }

        public bool TryGetWidget<T>([NotNullWhen(true)] out T? widget, [NotNullWhen(true)] out WidgetInstance? instance)
            where T : class, IHudWidget
        {
            widget = null;
            instance = null;
            return false;
        }

        public bool TryGetWidget<T>([NotNullWhen(true)] out T? widget)
            where T : class, IHudWidget
        {
            widget = null;
            return false;
        }

        public bool TryGetWidgetInstance<T>([NotNullWhen(true)] out WidgetInstance? instance)
            where T : class, IHudWidget
        {
            instance = null;
            return false;
        }
    }

    public class DummyMessageWindow : DummyDrawable, IHudMessageWindow
    {
        public float PosX { get; set; }
        public float PosY { get; set; }

        public bool Movable => false;

        public bool IsShowingBacklog => false;

        public void Print(string queryText, Color? color = null)
        {
        }

        public void Newline()
        {
        }

        public void Clear()
        {
        }

        public void RefreshWidget()
        {
        }

        public void Initialize()
        {
        }

        public void ToggleBacklog(bool visible)
        {
        }
    }
}