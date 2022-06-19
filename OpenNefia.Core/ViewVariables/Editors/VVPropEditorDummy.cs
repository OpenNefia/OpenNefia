using OpenNefia.Core.Log;
using OpenNefia.Core.UI.Wisp;
using OpenNefia.Core.UI.Wisp.Controls;
using System;
using System.Globalization;

namespace OpenNefia.Core.ViewVariables.Editors
{
    /// <summary>
    ///     Just writes out the ToString of the object.
    ///     The ultimate fallback.
    /// </summary>
    internal sealed class VVPropEditorDummy : VVPropEditor
    {
        protected override WispControl MakeUI(object? value)
        {
            if (!ReadOnly)
            {
                Logger.WarningS("vv", $"ViewVariablesPropertyEditorDummy being selected for editable field: {value?.GetType()}.");
            }
            return new Label
            {
                Text = value == null ? "null" : value.ToString() ?? "<null ToString()>",
                Align = Label.AlignMode.Right,
                HorizontalExpand = true
            };
        }
    }
}
