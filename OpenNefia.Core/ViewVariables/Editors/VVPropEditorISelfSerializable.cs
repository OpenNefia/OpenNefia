using OpenNefia.Core.Serialization;
using OpenNefia.Core.UI.Wisp;
using OpenNefia.Core.UI.Wisp.Controls;
using System;
using System.Globalization;

namespace OpenNefia.Core.ViewVariables.Editors
{
    internal sealed class VVPropEditorISelfSerializable<T> : VVPropEditor where T : ISelfSerialize
    {
        protected override WispControl MakeUI(object? value)
        {
            var lineEdit = new LineEdit
            {
                Text = ((ISelfSerialize)value!).Serialize(),
                Editable = !ReadOnly,
                HorizontalExpand = true,
            };

            if (!ReadOnly)
            {
                lineEdit.OnTextEntered += e =>
                {
                    var instance = (ISelfSerialize)Activator.CreateInstance(typeof(T))!;
                    instance.Deserialize(e.Text);
                    ValueChanged(instance);
                };
            }

            return lineEdit;
        }
    }
}
