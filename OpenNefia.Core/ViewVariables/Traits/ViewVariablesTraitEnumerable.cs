using System;
using System.Collections;
using System.Globalization;
using OpenNefia.Core.ViewVariables.Editors;
using OpenNefia.Core.ViewVariables.Instances;
using OpenNefia.Core.Utility;
using OpenNefia.Core.UI.Wisp.Controls;
using static OpenNefia.Core.UI.Wisp.Controls.BoxContainer;
using OpenNefia.Core.UI.Wisp;

namespace OpenNefia.Core.ViewVariables.Traits
{
    internal sealed class ViewVariablesTraitEnumerable : ViewVariablesTrait
    {
        private const int ElementsPerPage = 25;
        private readonly List<object?> _cache = new();
        private int _page;
        private IEnumerator? _enumerator;
        private bool _ended;

        private Button _leftButton = default!;
        private Button _rightButton = default!;
        private LineEdit _pageLabel = default!;
        private BoxContainer _controlsHBox = default!;
        private BoxContainer _elementsVBox = default!;

        private int HighestKnownPage => Math.Max(0, (_cache.Count + ElementsPerPage - 1) / ElementsPerPage - 1);

        public override void Initialize(ViewVariablesInstanceObject instance)
        {
            base.Initialize(instance);

            var enumerable = (IEnumerable)instance.Object;
            _enumerator = enumerable.GetEnumerator();

            var outerVBox = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical
            };
            _controlsHBox = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                HorizontalAlignment = WispControl.HAlignment.Center
            };

            {
                // Page navigational controls.
                _leftButton = new Button { Text = "<<", Disabled = true };
                _pageLabel = new LineEdit { Text = "0", MinSize = (60, 0) };
                _rightButton = new Button { Text = ">>" };

                _leftButton.OnPressed += _leftButtonPressed;
                _pageLabel.OnTextEntered += _lineEditTextEntered;
                _rightButton.OnPressed += _rightButtonPressed;

                _controlsHBox.AddChild(_leftButton);
                _controlsHBox.AddChild(_pageLabel);
                _controlsHBox.AddChild(_rightButton);
            }

            outerVBox.AddChild(_controlsHBox);

            _elementsVBox = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical
            };
            outerVBox.AddChild(_elementsVBox);

            instance.AddTab(nameof(IEnumerable), outerVBox);
        }

        public override void Refresh()
        {
            _cache.Clear();
            _ended = false;
            
            var enumerable = (IEnumerable)Instance.Object;
            _enumerator = enumerable.GetEnumerator();

            _moveToPage(_page);
        }

        private void _lineEditTextEntered(LineEdit.LineEditEventArgs obj)
        {
            _moveToPage(int.Parse(obj.Text, CultureInfo.InvariantCulture));
        }

        private void _rightButtonPressed(BaseButton.ButtonEventArgs obj)
        {
            _moveToPage(_page + 1);
        }

        private void _leftButtonPressed(BaseButton.ButtonEventArgs obj)
        {
            _moveToPage(_page - 1);
        }

        private void _moveToPage(int page)
        {
            if (page < 0)
            {
                page = 0;
            }

            if (page > HighestKnownPage || !_ended && page == HighestKnownPage)
            {
                if (_ended)
                {
                    // The requested page is higher than the highest page we have (and we know this because the enumerator ended).
                    page = HighestKnownPage;
                }
                else
                {
                    // The page is higher than the highest page we have, but the enumerator hasn't ended yet so that might be valid.
                    // Gotta get more data.
                     _cacheTo((page + 1) * ElementsPerPage);

                    if (page > HighestKnownPage)
                    {
                        // We tried, but the enumerator ended before we reached our goal.
                        // Oh well.
                        DebugTools.Assert(_ended);
                        page = HighestKnownPage;
                    }
                }
            }

            _elementsVBox.DisposeAllChildren();

            for (var i = page * ElementsPerPage; i < ElementsPerPage * (page + 1) && i < _cache.Count; i++)
            {
                var element = _cache[i];
                VVPropEditor editor;
                if (element == null)
                {
                    editor = new VVPropEditorDummy();
                }
                else
                {
                    var type = element.GetType();
                    editor = Instance.ViewVariablesManager.PropertyFor(type);
                }

                var control = editor.Initialize(element, true);

                _elementsVBox.AddChild(control);
            }

            _page = page;

            _updateControls();
        }

        private void _updateControls()
        {
            if (_ended && HighestKnownPage == 0)
            {
                // BUG: this is ignored when TabContainer selects this tab
                // since changes to Visible are always fully propagated by any parents.
                _controlsHBox.Visible = false;
                return;
            }
            else
            {
                _controlsHBox.Visible = true;
            }


            _leftButton.Disabled = _page == 0;
            _pageLabel.Text = $"{_page + 1}";
            _rightButton.Disabled = _page == HighestKnownPage && _ended;
        }

        private void _cacheTo(int index)
        {
            if (index < _cache.Count)
            {
                // This check is probably redundant, oh well.
                return;
            }
            
            DebugTools.AssertNotNull(_enumerator);
            while (_cache.Count < index)
            {
                if (!_enumerator!.MoveNext())
                {
                    _ended = true;
                    break;
                }

                _cache.Add(_enumerator!.Current);
            }
        }
    }
}
