using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.Input;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Layer;

namespace OpenNefia.Tests.Core.UserInterface
{
    [TestFixture]
    [TestOf(typeof(UserInterfaceManager))]
    public class UserInterfaceManager_Test : OpenNefiaUnitTest
    {
        private IUserInterfaceManagerInternal _userInterfaceManager = default!;

        [OneTimeSetUp]
        public void Setup()
        {
            _userInterfaceManager = IoCManager.Resolve<IUserInterfaceManagerInternal>();
            _userInterfaceManager.InitializeTesting();
        }

        [Test]
        [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
        public void TestMouseDown()
        {
            // We create 4 controls.
            // Control 2 is set to stop mouse events,
            // Control 3 pass,
            // Control 4 ignore.
            // We check that 4 and 1 do not receive events, that 3 receives before 2, and that positions are correct.
            var control1 = new UiElement
            {
            };
            var control2 = new UiElement
            {
                EventFilter = UIEventFilterMode.Stop
            };
            var control3 = new UiElement
            {
                EventFilter = UIEventFilterMode.Pass
            };
            var control4 = new UiElement
            {
                EventFilter = UIEventFilterMode.Ignore
            };

            control1.SetSize(50, 50);
            control2.SetSize(50, 50);
            control3.SetSize(50, 50);
            control4.SetSize(50, 50);

            var layer = new UiLayer();
            layer.AddChild(control1);

            _userInterfaceManager.PushLayer(layer);
            control1.SetPosition(0, 0);
            control1.AddChild(control2);
            control2.SetPosition(5, 5);
            control2.AddChild(control3);
            control3.SetPosition(10, 10);
            control3.AddChild(control4);
            control4.SetPosition(15, 15);

            var mouseEvent = new BoundKeyEventArgs(EngineKeyFunctions.UISelect, BoundKeyState.Down,
                new ScreenCoordinates(30, 30), true);

            var control2Fired = false;
            var control3Fired = false;

            control1.OnKeyBindDown += _ => Assert.Fail("Control 1 should not get a mouse event.");

            void Control2MouseDown(GUIBoundKeyEventArgs ev)
            {
                Assert.That(control2Fired, Is.False);
                Assert.That(control3Fired, Is.True);

                Assert.That(ev.RelativePixelPosition, Is.EqualTo(new Vector2(25, 25)));

                control2Fired = true;
            }

            control2.OnKeyBindDown += Control2MouseDown;

            control3.OnKeyBindDown += ev =>
            {
                Assert.That(control2Fired, Is.False);
                Assert.That(control3Fired, Is.False);

                Assert.That(ev.RelativePixelPosition, Is.EqualTo(new Vector2(20, 20)));

                control3Fired = true;
            };

            control4.OnKeyBindDown += _ => Assert.Fail("Control 4 should not get a mouse event.");

            _userInterfaceManager.KeyBindDown(mouseEvent);

            Assert.Multiple(() =>
            {
                Assert.That(control2Fired, Is.True);
                Assert.That(control3Fired, Is.True);
            });

            // Step two: instead of relying on stop for control2 to prevent the event reaching control1,
            // handle the event in control2.

            control2Fired = false;
            control3Fired = false;

            control2.OnKeyBindDown -= Control2MouseDown;
            control2.OnKeyBindDown += ev =>
            {
                Assert.That(control2Fired, Is.False);
                Assert.That(control3Fired, Is.True);

                Assert.That(ev.RelativePixelPosition, Is.EqualTo(new Vector2(25, 25)));

                control2Fired = true;
                ev.Handle();
            };
            control2.EventFilter = UIEventFilterMode.Pass;

            _userInterfaceManager.KeyBindDown(mouseEvent);

            Assert.Multiple(() =>
            {
                Assert.That(control2Fired, Is.True);
                Assert.That(control3Fired, Is.True);
            });

            control1.Dispose();
            control2.Dispose();
            control3.Dispose();
            control4.Dispose();
        }

        [Test]
        public void TestGrabKeyboardFocus()
        {
            Assert.That(_userInterfaceManager.KeyboardFocused, Is.Null);
            var control1 = new UiElement { CanKeyboardFocus = true };
            var control2 = new UiElement { CanKeyboardFocus = true };

            control1.GrabKeyboardFocus();
            Assert.That(_userInterfaceManager.KeyboardFocused, Is.EqualTo(control1));
            Assert.That(control1.HasKeyboardFocus(), Is.EqualTo(true));

            control1.ReleaseKeyboardFocus();
            Assert.That(_userInterfaceManager.KeyboardFocused, Is.Null);

            control1.Dispose();
            control2.Dispose();
        }

        [Test]
        public void TestGrabKeyboardFocusSteal()
        {
            Assert.That(_userInterfaceManager.KeyboardFocused, Is.Null);
            var control1 = new UiElement { CanKeyboardFocus = true };
            var control2 = new UiElement { CanKeyboardFocus = true };

            control1.GrabKeyboardFocus();
            control2.GrabKeyboardFocus();
            Assert.That(_userInterfaceManager.KeyboardFocused, Is.EqualTo(control2));
            control2.ReleaseKeyboardFocus();
            Assert.That(_userInterfaceManager.KeyboardFocused, Is.Null);

            control1.Dispose();
            control2.Dispose();
        }

        [Test]
        public void TestGrabKeyboardFocusOtherRelease()
        {
            Assert.That(_userInterfaceManager.KeyboardFocused, Is.Null);
            var control1 = new UiElement { CanKeyboardFocus = true };
            var control2 = new UiElement { CanKeyboardFocus = true };

            control1.GrabKeyboardFocus();
            control2.ReleaseKeyboardFocus();
            Assert.That(_userInterfaceManager.KeyboardFocused, Is.EqualTo(control1));
            _userInterfaceManager.ReleaseKeyboardFocus();
            Assert.That(_userInterfaceManager.KeyboardFocused, Is.Null);

            control1.Dispose();
            control2.Dispose();
        }

        [Test]
        public void TestGrabKeyboardFocusNull()
        {
            Assert.That(() => _userInterfaceManager.GrabKeyboardFocus(null!), Throws.ArgumentNullException);
            Assert.That(() => _userInterfaceManager.ReleaseKeyboardFocus(null!), Throws.ArgumentNullException);
        }

        [Test]
        public void TestGrabKeyboardFocusBlocked()
        {
            var control = new UiElement();
            Assert.That(() => _userInterfaceManager.GrabKeyboardFocus(control), Throws.ArgumentException);
        }

        [Test]
        public void TestGrabKeyboardFocusOnClick()
        {
            var control = new UiElement
            {
                CanControlFocus = true,
                CanKeyboardFocus = true,
                KeyboardFocusOnClick = true,
                EventFilter = UIEventFilterMode.Stop
            };

            control.SetSize(50, 50);

            var layer = new UiLayer();
            layer.AddChild(control);

            _userInterfaceManager.PushLayer(layer);

            _userInterfaceManager.HandleCanFocusDown(new ScreenCoordinates(30, 30), out _);

            Assert.That(_userInterfaceManager.KeyboardFocused, Is.EqualTo(control));
            _userInterfaceManager.ReleaseKeyboardFocus();
            Assert.That(_userInterfaceManager.KeyboardFocused, Is.Null);

            control.Dispose();
        }

        /// <summary>
        ///     Assert that indeed nothing happens when the control has focus modes off.
        /// </summary>
        [Test]
        public void TestNotGrabKeyboardFocusOnClick()
        {
            var control = new UiElement
            {
                EventFilter = UIEventFilterMode.Stop
            };

            control.SetSize(50, 50);

            var layer = new UiLayer();
            layer.AddChild(control);

            _userInterfaceManager.PushLayer(layer);

            var pos = new ScreenCoordinates(30, 30);

            var mouseEvent = new GUIBoundKeyEventArgs(EngineKeyFunctions.UISelect, BoundKeyState.Down,
                pos, true, pos.Position / 1f - control.Position, pos.Position - control.PixelPosition);

            _userInterfaceManager.KeyBindDown(mouseEvent);

            Assert.That(_userInterfaceManager.KeyboardFocused, Is.Null);

            control.Dispose();
        }

        private class PassFilter : IBoundKeyEventFilter
        {
            public bool Triggered { get; private set; } = false;

            public bool FilterEvent(UiElement element, GUIBoundKeyEventArgs evt)
            {
                Triggered = true;
                return true;
            }
        }

        private class IgnoreFilter : IBoundKeyEventFilter
        {
            public bool Triggered { get; private set; } = false;

            public bool FilterEvent(UiElement element, GUIBoundKeyEventArgs evt)
            {
                Triggered = true;
                return false;
            }
        }

        [Test]
        public void TestBoundKeyEventFilters()
        {
            var passControl = new UiElement
            {
                EventFilter = UIEventFilterMode.Pass,
            };
            var ignoreControl = new UiElement
            {
                EventFilter = UIEventFilterMode.Pass
            };

            var passFilter = new PassFilter();
            var passFilter2 = new PassFilter();
            var ignoreFilter = new IgnoreFilter();
            var ignoreFilter2 = new IgnoreFilter();
            passControl.BoundKeyEventFilters.Add(passFilter);
            passControl.BoundKeyEventFilters.Add(passFilter2);
            ignoreControl.BoundKeyEventFilters.Add(ignoreFilter);
            ignoreControl.BoundKeyEventFilters.Add(ignoreFilter2);

            passControl.SetSize(50, 50);
            ignoreControl.SetSize(50, 50);

            var layer = new UiLayer();
            layer.AddChild(passControl);
            layer.AddChild(ignoreControl);

            _userInterfaceManager.PushLayer(layer);
            passControl.SetPosition(0, 0);
            ignoreControl.SetPosition(50, 50);

            var passControlFired = false;

            void PassControlMouseDown(GUIBoundKeyEventArgs ev)
            {
                passControlFired = true;
            }

            passControl.OnKeyBindDown += PassControlMouseDown;
            ignoreControl.OnKeyBindDown += _ => Assert.Fail("Ignore control should not get a mouse event.");

            var mouseEvent1 = new BoundKeyEventArgs(EngineKeyFunctions.UISelect, BoundKeyState.Down,
                new ScreenCoordinates(30, 30), true);
            var mouseEvent2 = new BoundKeyEventArgs(EngineKeyFunctions.UISelect, BoundKeyState.Down,
                new ScreenCoordinates(80, 80), true);

            _userInterfaceManager.KeyBindDown(mouseEvent1);
            _userInterfaceManager.KeyBindDown(mouseEvent2);

            Assert.Multiple(() =>
            {
                Assert.That(passControlFired, Is.True, "Pass control fired");
                Assert.That(passFilter.Triggered, Is.True, "Pass filter triggered");
                Assert.That(ignoreFilter.Triggered, Is.True, "Ignore filter triggered");
                Assert.That(passFilter2.Triggered, Is.True, "Pass filter 2 triggered");
                Assert.That(ignoreFilter2.Triggered, Is.False, "Ignore filter 2 triggered");
            });

            passControl.Dispose();
            ignoreControl.Dispose();
        }
    }
}
