using OpenNefia.Core.UI;
using OpenNefia.Core.UI.Wisp;
using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UserInterface.XAML;
using OpenNefia.Core.UserInterface.XAML.HotReload;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Input;
using OpenNefia.Core.UI.Element;
using OpenNefia.XamlInjectors;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Log;
using XamlX.Parsers;
using OpenNefia.Core.Maths;
using OpenNefia.Core.HotReload;
using OpenNefia.Core.Asynchronous;
using static OpenNefia.XamlInjectors.XamlCompiler;

namespace OpenNefia.Core.ControlDesigner
{
    /// <summary>
    /// <para>
    /// UI for previewing and modifying controls. This acts as a substitute for
    /// a "designer view"-type plugin in-editor. The usage is as follows:
    /// </para>
    /// <para>
    /// <list type="number">
    /// <item>
    /// Open the XAML file of the control you want to edit using this layer.
    /// </item>
    /// <item>
    /// Open an external editor and make changes to the XAML. Watch as your
    /// edits are applied after saving.
    /// </item>
    /// <item>
    /// Open Visual Studio, make changes to the corresponding <c>.xaml.cs</c> file,
    /// and do a Hot Reload. The control will also update itself in that case.
    /// (NOTE: The call to <see cref="OpenNefiaXamlLoader.Load(object)"/> must be in the control's
    /// constructor for this to work, due to performance reasons).
    /// </item>
    /// <item>
    /// Open an external editor and modify the currently used stylesheet, to see
    /// your changes instantly.
    /// </item>
    /// </list>
    /// </para>
    /// </summary>
    public sealed class ControlDesignerLayer : WispLayerWithResult<UINone, UINone>
    {
        [Dependency] private readonly IReflectionManager _reflectionManager = default!;
        [Dependency] private readonly IXamlHotReloadManager _xamlHotReload = default!;
        [Dependency] private readonly IHotReloadWatcher _hotReloadWatcher = default!;
        [Dependency] private readonly ITaskManager _taskManager = default!;

        public const string StyleClassDesignerBackground = "designerBackground";
        public const string StyleClassDesignerToolbar = "designerToolbar";
        public const string StyleClassDesignerPointer = "designerPointer";

        public override int? DefaultZOrder => 30000000;

        private record Data(WispControl Control, Type ControlType, XamlResource XamlResource);

        private Data? _current;
        private FileSystemWatcher? _watcher;

        private bool _watching = true;

        private LayoutContainer _layoutArea;
        private Button _hotReloadButton;
        private CheckBox _debugCheckBox;
        private CheckBox _watchCheckBox;
        private Label _errorLabel;
        private MeasurementPointer _pointer;

        public ControlDesignerLayer()
        {
            WispRoot.AddChild(new PanelContainer()
            {
                Class = StyleClassDesignerBackground,
            });

            var openXamlButton = new Button()
            {
                ExactSize = MaxSize = (120, 20),
                Margin = new Thickness(5),
                Text = "Open XAML..."
            };
            openXamlButton.OnPressed += PromptOpenFile;

            _hotReloadButton = new Button()
            {
                ExactSize = MaxSize = (120, 20),
                Margin = new Thickness(5),
                Text = "Hot Reload"
            };
            _hotReloadButton.OnPressed += _ => DoHotReload();

            _debugCheckBox = new CheckBox()
            {
                ExactSize = MaxSize = (120, 20),
                Margin = new Thickness(5),
                Text = "Debug"
            };
            _debugCheckBox.OnPressed += ToggleDebug;

            _watchCheckBox = new CheckBox()
            {
                ExactSize = MaxSize = (120, 20),
                Margin = new Thickness(5),
                Text = "Watch"
            };
            _watchCheckBox.OnPressed += ToggleWatching;

            var spacer = new BoxContainer()
            {
                HorizontalExpand = true
            };

            var closeButton = new Button()
            {
                HorizontalAlignment = WispControl.HAlignment.Center,
                Margin = new Thickness(5),
                Text = "Exit",
            };
            closeButton.OnPressed += _ => Cancel();

            var toolbar = new PanelContainer()
            {
                Class = StyleClassDesignerToolbar,
                Margin = new Thickness(5)
            };

            var toolbarContents = new BoxContainer()
            {
                Orientation = BoxContainer.LayoutOrientation.Horizontal,
                VerticalAlignment = WispControl.VAlignment.Top,
            };
            toolbarContents.AddChild(openXamlButton);
            toolbarContents.AddChild(_hotReloadButton);
            toolbarContents.AddChild(_debugCheckBox);
            toolbarContents.AddChild(_watchCheckBox);
            toolbarContents.AddChild(spacer);
            toolbarContents.AddChild(closeButton);
            toolbar.AddChild(toolbarContents);

            WispRoot.AddChild(toolbar);

            _layoutArea = new LayoutContainer()
            {
                HorizontalExpand = true,
                VerticalExpand = true
            };

            _errorLabel = new Label()
            {
                HorizontalAlignment = WispControl.HAlignment.Stretch,
                VerticalAlignment = WispControl.VAlignment.Bottom,
                VAlign = Label.VAlignMode.Top,
                Margin = new Thickness(5)
            };
            WispRoot.AddChild(_errorLabel);

            WispRoot.AddChild(_layoutArea);

            _pointer = new MeasurementPointer()
            {
                Class = StyleClassDesignerPointer
            };
            WispRoot.AddChild(_pointer);

            OnKeyBindDown += KeyBindDown;
        }

        private void ToggleDebug(BaseButton.ButtonEventArgs evt)
        {
            Debug = evt.Button.Pressed;
        }

        private void ToggleWatching(BaseButton.ButtonEventArgs evt)
        {
            if (evt.Button.Pressed)
            {
                StartWatching();
            }
            else
            {
                StopWatching();
            }
        }

        private void StartWatching()
        {
            _watching = true;
            if (_watcher != null)
                _watcher.EnableRaisingEvents = true;
        }

        private void StopWatching()
        {
            _watching = false;
            if (_watcher != null)
                _watcher.EnableRaisingEvents = false;
        }

        private void KeyBindDown(GUIBoundKeyEventArgs args)
        {
            if (args.Function == EngineKeyFunctions.UICancel)
            {
                Cancel();
            }
        }

        protected override void Resized()
        {
            base.Resized();
            RecenterControl();
        }

        private void RecenterControl()
        {
            if (_current == null)
                return;

            // BUG: Measure() hasn't been run here yet?
            _current.Control.Measure(_layoutArea.Size);
            _current.Control.ExactSize = _current.Control.DesiredSize;
            LayoutContainer.SetPosition(_current.Control, (_layoutArea.Size - _current.Control.ExactSize) / 2);
        }

        public override void OnQuery()
        {
            base.OnQuery();

            _hotReloadWatcher.OnUpdateApplication += OnHotReload;
        }

        public override void OnQueryFinish()
        {
            base.OnQueryFinish();

            _hotReloadWatcher.OnUpdateApplication -= OnHotReload;
            StopWatching();
            _watcher?.Dispose();
            _watcher = null;
        }

        public override void Update(float dt)
        {
            base.Update(dt);

            _debugCheckBox.Pressed = Debug;
            _hotReloadButton.Disabled = _current == null;
            _watchCheckBox.Pressed = _watching;

            Love.Mouse.GetPosition(out var mx, out var my);
            _pointer.Position = new((float)mx / UIScale, ((float)my / UIScale) - Height);
        }

        private void OnHotReload(HotReloadEventArgs args)
        {
            if (!IsQuerying())
                return;

            if (args.UpdatedTypes != null && _current != null && args.UpdatedTypes.Contains(_current.ControlType))
            {
                Logger.InfoS("wisp.designer", $"Detected hot reload on current control type ({_current.ControlType}).");
                RebuildControl();
            }
        }

        private void PromptOpenFile(BaseButton.ButtonEventArgs obj)
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

                _pointer.Target = null;

                var xamlResource = new XamlResource(controlType, xamlPath);
                var designingControl = Activator.CreateInstance(controlType) as WispControl;
                if (designingControl == null)
                    throw new InvalidOperationException($"Failed to instantiate control of type {controlType}.");

                if (_current != null)
                {
                    if (_current.Control.Parent == _layoutArea)
                        _layoutArea.RemoveChild(_current.Control);
                    _current.Control.Dispose();
                }

                _current = new Data(designingControl, controlType, xamlResource);

                _layoutArea.AddChild(_current.Control);
                RecenterControl();

                CreateFileWatcher(_current.XamlResource);

                _pointer.Target = _current.Control;
                _errorLabel.Text = null;

                Logger.InfoS("wisp.designer", $"Loaded control {controlType.Name}. ({xamlPath})");
            }
            catch (Exception ex)
            {
                Logger.ErrorS("wisp.designer", $"Failed to load XAML from {xamlPath}");
                Logger.ErrorS("wisp.designer", ex.ToString());
                _errorLabel.Text = "Error: " + ex.Message;
            }
        }

        private void CreateFileWatcher(IResource xamlResource)
        {
            _watcher?.Dispose();
            _watcher = new FileSystemWatcher(Path.GetDirectoryName(xamlResource.FilePath)!, "*.xaml")
            {
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.LastWrite
            };
            _watcher.Changed += (_, args) =>
            {
                switch (args.ChangeType)
                {
                    case WatcherChangeTypes.Renamed:
                    case WatcherChangeTypes.Deleted:
                        return;
                    case WatcherChangeTypes.Created:
                    case WatcherChangeTypes.Changed:
                    case WatcherChangeTypes.All:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _taskManager.RunOnMainThread(() =>
                {
                    if (_current?.XamlResource.FilePath == args.FullPath)
                    {
                        Logger.DebugS("wisp.designer", $"Detected file change: {args.FullPath}");
                        DoHotReload();
                    }
                });
            };
            _watcher.EnableRaisingEvents = _watching;
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
                _pointer.Target = null;

                var designingControl = (WispControl)Activator.CreateInstance(_current.ControlType)!;
                if (designingControl == null)
                    throw new InvalidOperationException($"Failed to instantiate control of type {_current.ControlType}.");

                if (_current.Control.Parent == _layoutArea)
                    _layoutArea.RemoveChild(_current.Control);
                _current.Control.Dispose();

                _current = new Data(designingControl, _current.ControlType, _current.XamlResource);

                _layoutArea.AddChild(_current.Control);
                RecenterControl();

                _errorLabel.Text = null;
                _pointer.Target = _current.Control;

                Logger.DebugS("wisp.designer", $"Rebuilt the current control.");
            }
            catch (Exception ex)
            {
                Logger.ErrorS("wisp.designer", $"Failed to reinstantiate control '{_current.ControlType}'");
                Logger.ErrorS("wisp.designer", ex.ToString());
                _errorLabel.Text = "Error: " + ex.Message;
            }
        }

        private void DoHotReload()
        {
            if (_current == null)
            {
                Logger.WarningS("wisp.designer", "No control to hot reload!");
                return;
            }

            try
            {
                _xamlHotReload.HotReloadControlXaml(_current.ControlType, _current.XamlResource.FilePath);
                RebuildControl();

                _errorLabel.Text = null;
            }
            catch (Exception ex)
            {
                Logger.ErrorS("wisp.designer", $"Failed to hot reload control '{_current.ControlType}'");
                Logger.ErrorS("wisp.designer", ex.ToString());
                _errorLabel.Text = "Error: " + ex.Message;
            }
        }

        public void Dispose()
        {
            _watcher?.Dispose();
        }
    }
}
