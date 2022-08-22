using OpenNefia.Core.Containers;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Wisp;
using OpenNefia.Core.UI.Wisp.CustomControls;
using OpenNefia.Core.UserInterface;
using OpenNefia.Core.Utility;
using OpenNefia.Core.ViewVariables.Editors;
using OpenNefia.Core.ViewVariables.Instances;
using OpenNefia.Core.ViewVariables.Traits;
using System.Collections;

namespace OpenNefia.Core.ViewVariables
{
    public sealed class ViewVariablesManager : IViewVariablesManagerInternal
    {
        [Dependency] private readonly IUserInterfaceManager _uiManager = default!;

        private readonly Dictionary<Type, HashSet<Type>> _cachedTraits = new();
        private readonly List<ViewVariablesPropertyMatcher> _matchers = new()
        {
            new ViewVariablesBuiltinPropertyMatcher()
        };

        private readonly Vector2i _defaultWindowSize = (640, 420);

        private readonly Dictionary<ViewVariablesInstance, DefaultWindow> _windows = new();

        /// <summary>
        ///     Figures out which VV traits an object type has.
        /// </summary>
        /// <seealso cref="ViewVariablesBlobMetadata.Traits"/>
        public ICollection<Type> TraitIdsFor(Type type)
        {
            if (!_cachedTraits.TryGetValue(type, out var traits))
            {
                traits = new HashSet<Type>();
                _cachedTraits.Add(type, traits);
                if (ViewVariablesUtility.TypeHasVisibleMembers(type))
                {
                    traits.Add(typeof(ViewVariablesTraitMembers));
                }

                if (typeof(IEnumerable).IsAssignableFrom(type))
                {
                    traits.Add(typeof(ViewVariablesTraitEnumerable));
                }

                if (typeof(EntityUid).IsAssignableFrom(type))
                {
                    traits.Add(typeof(ViewVariablesTraitEntity));
                }
            }

            return traits;
        }

        public VVPropEditor PropertyFor(Type? type)
        {
            if (type == null)
                return new VVPropEditorDummy();

            foreach (var matcher in _matchers)
            {
                var propEditorType = matcher.PropEditorFor(type);
                if (propEditorType != null)
                    return propEditorType;
            }

            return new VVPropEditorDummy();
        }

        public void OpenVV(object obj, IWispLayer? layer = null)
        {
            // TODO all layers should inherit IWispLayer at some point
            if (layer == null)
            {
                layer = _uiManager.CurrentLayer as IWispLayer;
                if (layer == null)
                {
                    Logger.ErrorS("vv", $"Can't open ViewVariables in this context: {_uiManager.CurrentLayer}");
                    return;
                }
            }

            ViewVariablesInstance instance = new ViewVariablesInstanceObject(this);

            var window = new DefaultWindow { Title = "View Variables" };
            instance.Initialize(window, obj);
            window.OnClose += () => _closeInstance(instance, false);
            _windows.Add(instance, window);
            window.ExactSize = _defaultWindowSize;

            DefaultWindow? lastWindow = GetLastOpenedWindow();
            if (lastWindow != null)
                window.OpenAt(layer, lastWindow.Position + (20, 20));
            else
                window.OpenCentered(layer);
        }

        private DefaultWindow? GetLastOpenedWindow()
        {
            if (_windows.Count == 0)
                return null;

            var control = _uiManager.ControlFocused;
            while (control != null)
            {
                if (control is DefaultWindow win && _windows.ContainsValue(win))
                    return win;

                control = control.Parent;
            }

            return null;
        }

        private void _closeInstance(ViewVariablesInstance instance, bool closeWindow)
        {
            if (!_windows.TryGetValue(instance, out var window))
            {
                throw new ArgumentException();
            }

            if (closeWindow)
            {
                window.Dispose();
            }

            _windows.Remove(instance);
            instance.Close();
        }
    }
}
