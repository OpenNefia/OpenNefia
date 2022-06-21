﻿using OpenNefia.Core.Input;
using OpenNefia.Core.Maths;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UI.Wisp.Drawing;
using OpenNefia.Core.UserInterface;
using static OpenNefia.Core.UI.Wisp.Controls.BoxContainer;

namespace OpenNefia.Core.ViewVariables
{
    internal sealed class ViewVariablesPropertyControl : PanelContainer
    {
        public BoxContainer VBox { get; }
        public BoxContainer TopContainer { get; }
        public BoxContainer BottomContainer { get; }
        public Label NameLabel { get; }

        private readonly Label _bottomLabel;

        private readonly IViewVariablesManagerInternal _viewVariablesManager;

        public ViewVariablesPropertyControl(IViewVariablesManagerInternal viewVars)
        {
            EventFilter = UIEventFilterMode.Pass;

            _viewVariablesManager = viewVars;

            // ToolTip = "Click to expand";
            MinHeight = 25;

            VBox = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                SeparationOverride = 0
            };
            AddChild(VBox);

            TopContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                VerticalExpand = true
            };
            VBox.AddChild(TopContainer);

            BottomContainer = new BoxContainer
            {
                Orientation = LayoutOrientation.Horizontal,
                Visible = false
            };
            VBox.AddChild(BottomContainer);

            //var smallFont = new VectorFont(_resourceCache.GetResource<FontResource>("/Fonts/CALIBRI.TTF"), 10);

            _bottomLabel = new Label
            {
                //    FontOverride = smallFont,
                FontColorOverride = Color.DarkGray
            };
            BottomContainer.AddChild(_bottomLabel);

            NameLabel = new Label();
            TopContainer.AddChild(NameLabel);
        }

        public VVPropEditor SetProperty(ViewVariablesBlobMembers.MemberData member)
        {
            NameLabel.Text = member.Name;
            var type = Type.GetType(member.Type);

            _bottomLabel.Text = $"Type: {member.TypePretty}";
            VVPropEditor editor = _viewVariablesManager.PropertyFor(type);

            var view = editor.Initialize(member.Value, !member.Editable);
            if (!view.HorizontalExpand)
            {
                NameLabel.HorizontalExpand = true;
            }

            NameLabel.MinSize = new Vector2(150, 0);
            TopContainer.AddChild(view);
            /*
            _beingEdited = obj;
            _editedProperty = propertyInfo;
            DebugTools.Assert(propertyInfo.DeclaringType != null);
            DebugTools.Assert(propertyInfo.DeclaringType.IsInstanceOfType(obj));

            DebugTools.Assert(ViewVariablesUtility.TryGetViewVariablesAccess(fieldInfo, out var access));
            NameLabel.Text = propertyInfo.Name;

            _bottomLabel.Text = $"Type: {propertyInfo.PropertyType.FullName}";

            var editor = vvm.PropertyFor(propertyInfo.PropertyType);
            var value = propertyInfo.GetValue(obj);

            var view = editor.Initialize(value, access != VVAccess.ReadWrite);
            if (view.SizeFlagsHorizontal != SizeFlags.FillExpand)
            {
                NameLabel.HorizontalExpand = true;
            }
            NameLabel.MinSize = new Vector2(150, 0);
            TopContainer.AddChild(view);
            editor.OnValueChanged += v => { propertyInfo.SetValue(obj, v); };
            */
            return editor;
        }

        public void SetStyle(bool other)
        {
            PanelOverride = GetAlternatingStyleBox(other);
        }

        public static StyleBox GetAlternatingStyleBox(bool other)
        {
            var box = new StyleBoxFlat();
            box.BackgroundColor = other ? Color.Transparent : Color.Black.WithAlpha(0.25f);
            box.SetContentMarginOverride(StyleBox.Margin.Vertical, 1);
            box.SetContentMarginOverride(StyleBox.Margin.Horizontal, 5);
            return box;
        }

        protected internal override void KeyBindDown(GUIBoundKeyEventArgs args)
        {
            base.KeyBindDown(args);

            if (args.Function != EngineKeyFunctions.UIClick)
            {
                return;
            }

            BottomContainer.Visible = !BottomContainer.Visible;
            args.Handle();
        }
    }
}
