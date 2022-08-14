using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization;
using OpenNefia.Core.UI.Wisp;
using OpenNefia.Core.UI.Wisp.Controls;
using OpenNefia.Core.UI.Wisp.CustomControls;
using OpenNefia.Core.Utility;
using OpenNefia.Core.ViewVariables;
using static OpenNefia.Core.UI.Wisp.Controls.BoxContainer;

namespace OpenNefia.Core.ViewVariables
{
    /// <summary>
    ///     Controls the behavior of a VV window.
    /// </summary>
    internal abstract class ViewVariablesInstance
    {
        public readonly IViewVariablesManagerInternal ViewVariablesManager;

        protected ViewVariablesInstance(IViewVariablesManagerInternal vvm)
        {
            ViewVariablesManager = vvm;
        }

        /// <summary>
        ///     Initializes this instance to work on an object.
        /// </summary>
        /// <param name="window">The window to initialize by adding GUI components.</param>
        /// <param name="obj">The object that is being VV'd</param>
        public abstract void Initialize(DefaultWindow window, object obj);

        /// <summary>
        ///     Invoked to "clean up" the instance, such as closing remote sessions.
        /// </summary>
        public virtual void Close()
        {
        }

        protected internal static IEnumerable<IGrouping<Type, WispControl>> LocalPropertyList(object obj, IViewVariablesManagerInternal vvm)
        {
            var styleOther = false;
            var type = obj.GetType();

            var members = new List<(MemberInfo, VVAccess, object? value, Action<object?, bool> onValueChanged, Type)>();

            foreach (var fieldInfo in type.GetAllFields())
            {
                if (!ViewVariablesUtility.TryGetViewVariablesAccess(fieldInfo, out var access))
                {
                    continue;
                }

                members.Add((fieldInfo, (VVAccess)access, fieldInfo.GetValue(obj), (v, _) => fieldInfo.SetValue(obj, v),
                    fieldInfo.FieldType));
            }

            foreach (var propertyInfo in type.GetAllProperties())
            {
                if (!ViewVariablesUtility.TryGetViewVariablesAccess(propertyInfo, out var access))
                {
                    continue;
                }

                if (!propertyInfo.IsBasePropertyDefinition())
                {
                    continue;
                }

                members.Add((propertyInfo, (VVAccess)access, propertyInfo.GetValue(obj),
                    (v, _) => propertyInfo.GetSetMethod(true)!.Invoke(obj, new[] { v }), propertyInfo.PropertyType));
            }

            var groupedSorted = members
                .OrderBy(p => p.Item1.Name)
                .GroupBy(p => p.Item1.DeclaringType!, tuple =>
                {
                    var (memberInfo, access, value, onValueChanged, memberType) = tuple;
                    var data = new ViewVariablesBlobMembers.MemberData
                    {
                        Editable = access == VVAccess.ReadWrite,
                        Name = memberInfo.Name,
                        Type = memberType.AssemblyQualifiedName!,
                        TypePretty = TypeAbbreviation.Abbreviate(memberType),
                        Value = value
                    };

                    var propertyEdit = new ViewVariablesPropertyControl(vvm);
                    propertyEdit.SetStyle(styleOther = !styleOther);
                    var editor = propertyEdit.SetProperty(data);
                    editor.OnValueChanged += onValueChanged;
                    return propertyEdit;
                })
                .OrderByDescending(p => p.Key, TypeHelpers.TypeInheritanceComparer);

            return groupedSorted;
        }

        protected static WispControl MakeTopBar(string top, string bottom)
        {
            if (top == bottom)
            {
                return new Label { Text = top, ClipText = true };
            }

            //var smallFont =
            //    new VectorFont(IoCManager.Resolve<IResourceCache>().GetResource<FontResource>("/Fonts/CALIBRI.TTF"),
            //        10);

            // Custom ToString() implementation.
            var headBox = new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                SeparationOverride = 0
            };
            headBox.AddChild(new Label { Text = top, ClipText = true });
            headBox.AddChild(new Label
            {
                Text = bottom,
                //    FontOverride = smallFont,
                FontColorOverride = Color.DarkGray,
                ClipText = true
            });
            return headBox;
        }
    }
}
