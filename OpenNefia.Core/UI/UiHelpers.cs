using OpenNefia.Core.Log;
using OpenNefia.Core.UI.Element;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.UI
{
    public static class UiHelpers
    {
        private static IEnumerable<AbstractFieldInfo> GetChildAnnotatedFields(UiElement elem)
        {
            return elem.GetType().GetAllPropertiesAndFields()
                .Where(info => info.HasAttribute<ChildAttribute>());
        }

        public static void AddChildrenFromAttributesRecursive(UiElement parent)
        {
            foreach (var info in GetChildAnnotatedFields(parent))
            {
                var child = info.GetValue(parent);
                if (child is not UiElement childElem)
                {
                    Logger.WarningS("ui", $"Could not add child '{info.Name}' ({child}) to parent {nameof(UiElement)} {parent}");
                    continue;
                }

                if (childElem.Parent != null)
                    continue;

                parent.AddChild(childElem);
            }

            foreach (var child in parent.Children)
            {
                AddChildrenFromAttributesRecursive(child);
            }
        }

        public static void AddChildrenRecursive(this UiElement parent, UiElement child)
        {
            parent.AddChild(child);
            AddChildrenFromAttributesRecursive(child);
        }
    }
}
