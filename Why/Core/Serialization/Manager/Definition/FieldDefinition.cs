using System;
using Why.Core.Serialization.Manager.Attributes;
using Why.Core.Utility;

namespace Why.Core.Serialization.Manager.Definition
{
    internal class FieldDefinition
    {
        public FieldDefinition(
            DataFieldAttribute attr,
            string tag,
            object? defaultValue,
            AbstractFieldInfo fieldInfo,
            AbstractFieldInfo backingField,
            InheritanceBehavior inheritanceBehavior)
        {
            BackingField = backingField;
            Attribute = attr;
            Tag = tag;
            DefaultValue = defaultValue;
            FieldInfo = fieldInfo;
            InheritanceBehavior = inheritanceBehavior;
        }

        public DataFieldAttribute Attribute { get; }

        public string Tag { get; }

        public object? DefaultValue { get; }

        public InheritanceBehavior InheritanceBehavior { get; }

        public AbstractFieldInfo BackingField { get; }

        public AbstractFieldInfo FieldInfo { get; }

        public Type FieldType => FieldInfo.FieldType;

        public object? GetValue(object? obj)
        {
            return BackingField.GetValue(obj);
        }

        public void SetValue(object? obj, object? value)
        {
            BackingField.SetValue(obj, value);
        }
    }
}
