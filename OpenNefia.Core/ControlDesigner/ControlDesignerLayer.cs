using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Wisp;
using OpenNefia.Core.UI.Wisp.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.UserInterface.XAML.HotReload;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Input;
using OpenNefia.Core.UI.Element;
using OpenNefia.XamlInjectors;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Log;
using XamlX.Ast;
using XamlX.Parsers;
using OpenNefia.Core.Maths;

namespace OpenNefia.Core.ControlDesigner
{
    public sealed class ControlDesignerLayer : WispLayerWithResult<UINone, UINone>
    {
        [Dependency] private readonly IReflectionManager _reflectionManager = default!;
        [Dependency] private readonly IXamlHotReloadManager _xamlHotReload = default!;

        public override int? DefaultZOrder => 30000000;

        private record Data(WispControl Control, Type ControlType, XamlResource XamlResource);

        private Data? _current;

        private LayoutContainer _layoutArea;
        private Button _hotReloadButton;

        public ControlDesignerLayer()
        {
            WispRoot.AddChild(new PanelContainer()
            {
                Class = "designerBackground",
            });

            var openXamlButton = new Button()
            {
                ExactSize = MaxSize = (120, 20),
                Text = "Open XAML..."
            };
            openXamlButton.OnPressed += QueryOpen;

            _hotReloadButton = new Button()
            {
                ExactSize = MaxSize = (120, 20),
                Text = "Hot Reload",
                Disabled = true
            };
            _hotReloadButton.OnPressed += DoHotReload;

            var debugCheckBox = new CheckBox()
            {
                ExactSize = MaxSize = (120, 20),
                Text = "Debug",
                Pressed = Debug
            };
            debugCheckBox.OnPressed += ToggleDebug;

            var spacer = new BoxContainer()
            {
                HorizontalExpand = true
            };

            var closeButton = new Button()
            {
                HorizontalAlignment = WispControl.HAlignment.Center,
                Text = "Exit",
            };
            closeButton.OnPressed += _ => Cancel();

            var toolbar = new PanelContainer()
            {
                Class = "designerToolbar",
                Margin = new Maths.Thickness(5)
            };

            var toolbarContents = new BoxContainer()
            {
                Orientation = BoxContainer.LayoutOrientation.Horizontal,
                VerticalAlignment = WispControl.VAlignment.Top,
            };
            toolbarContents.AddChild(openXamlButton);
            toolbarContents.AddChild(_hotReloadButton);
            toolbarContents.AddChild(debugCheckBox);
            toolbarContents.AddChild(spacer);
            toolbarContents.AddChild(closeButton);
            toolbar.AddChild(toolbarContents);

            WispRoot.AddChild(toolbar);

            _layoutArea = new LayoutContainer()
            {
                HorizontalExpand = true,
                VerticalExpand = true
            };

            WispRoot.AddChild(_layoutArea);

            OnKeyBindDown += KeyBindDown;
        }

        private void ToggleDebug(BaseButton.ButtonEventArgs evt)
        {
            Debug = evt.Button.Pressed;
        }

        private void KeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel)
            {
                Cancel();
            }
        }

        private void QueryOpen(BaseButton.ButtonEventArgs obj)
        {
            var dir = Directory.GetCurrentDirectory();

            if (_current != null)
            {
                var xamlDir = Path.GetDirectoryName(_current.XamlResource.FilePath);
                if (Directory.Exists(xamlDir))
                {
                    dir = xamlDir;
                }
            }

            var result = NativeFileDialogSharp.Dialog.FileOpen("xaml", dir);

            if (result.IsOk && result.Path.ToLowerInvariant().EndsWith(".xaml"))
            {
                LoadXaml(result.Path);
            }
        }

        private void LoadXaml(string xamlPath)
        {
            try
            {
                var xamlDoc = XDocumentXamlParser.Parse(File.ReadAllText(xamlPath));

                var className = XamlAstHelpers.GetClassNameFromXaml(xamlPath, xamlDoc);
                var controlType = _reflectionManager.GetType(className);

                if (!typeof(WispControl).IsAssignableFrom(controlType))
                {
                    throw new InvalidDataException($"Type '{controlType}' does not inherit from {nameof(WispControl)}");
                }

                var xamlResource = new XamlResource(controlType, xamlPath);
                var designingControl = (WispControl)Activator.CreateInstance(controlType)!;

                if (_current != null)
                {
                    _layoutArea.RemoveChild(_current.Control);
                    _current = null;
                }

                _current = new Data(designingControl, controlType, xamlResource);

                _layoutArea.AddChild(_current.Control);
                _current.Control.Measure(_layoutArea.Size);
                _current.Control.ExactSize = _current.Control.DesiredSize;
                LayoutContainer.SetPosition(_current.Control, (_layoutArea.Size - _current.Control.ExactSize) / 2);
                _hotReloadButton.Disabled = false;

                Logger.InfoS("wisp.designer", $"Loaded control {controlType.Name}. ({xamlPath})");
            }
            catch (Exception ex)
            {
                Logger.ErrorS("wisp.designer", $"Failed to load XAML from {xamlPath}");
                Logger.ErrorS("wisp.designer", ex.ToString());
            }
        }

        private void RebuildControl()
        {
            if (_current == null)
            {
                Logger.WarningS("wisp.designer", "No control to reinstantiate!");
                return;
            }

            try
            {
                var designingControl = (WispControl)Activator.CreateInstance(_current.ControlType)!;

                _layoutArea.RemoveChild(_current.Control);

                _current = new Data(designingControl, _current.ControlType, _current.XamlResource);

                _layoutArea.AddChild(_current.Control);
                _current.Control.Measure(_layoutArea.Size);
                _current.Control.ExactSize = _current.Control.DesiredSize;
                LayoutContainer.SetPosition(_current.Control, (_layoutArea.Size - _current.Control.ExactSize) / 2);
                _hotReloadButton.Disabled = false;

                Logger.InfoS("wisp.designer", $"Rebuilt the current control.");
            }
            catch (Exception ex)
            {
                Logger.ErrorS("wisp.designer", $"Failed to reinstantiate control '{_current.ControlType}'");
                Logger.ErrorS("wisp.designer", ex.ToString());
            }
        }

        private void DoHotReload(BaseButton.ButtonEventArgs evt)
        {
            if (_current == null)
            {
                Logger.WarningS("wisp.designer", "No control to hot reload!");
                return;
            }

            try
            {
                _xamlHotReload.HotReloadXamlControl(_current.ControlType, _current.XamlResource.FilePath);
                RebuildControl();
            }
            catch (Exception ex)
            {
                Logger.ErrorS("wisp.designer", $"Failed to hot reload control '{_current.ControlType}'");
                Logger.ErrorS("wisp.designer", ex.ToString());
            }
        }
    }
}
