using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Why.Core.Log;
using Why.Core.Serialization.Manager.Attributes;
using Why.Core.Serialization.Manager.Result;
using Why.Core.Serialization.Markdown.Mapping;
using Why.Core.Serialization.Markdown.Validation;
using Why.Core.Serialization.Markdown.Value;
using Why.Core.Utility;
using static Why.Core.Serialization.Manager.SerializationManager;

namespace Why.Core.Serialization.Manager.Definition
{
    public partial class DataDefinition
    {
        private readonly DeserializeDelegate _deserialize;
        private readonly PopulateDelegateSignature _populate;
        private readonly SerializeDelegateSignature _serialize;
        private readonly CopyDelegateSignature _copy;

        public DataDefinition(Type type)
        {
            Type = type;

            var fieldDefs = GetFieldDefinitions();

            Duplicates = fieldDefs
                .Where(f =>
                    fieldDefs.Count(df => df.Tag == f.Tag) > 1)
                .Select(f => f.Tag)
                .Distinct()
                .ToArray();

            var fields = fieldDefs;

            fields.Sort((a, b) => b.Attribute.Priority.CompareTo(a.Attribute.Priority));

            BaseFieldDefinitions = fields.ToImmutableArray();
            DefaultValues = fieldDefs.Select(f => f.DefaultValue).ToArray();

            _deserialize = EmitDeserializationDelegate();
            _populate = EmitPopulateDelegate();
            _serialize = EmitSerializeDelegate();
            _copy = EmitCopyDelegate();

            var fieldAccessors = new AccessField<object, object?>[BaseFieldDefinitions.Length];
            var fieldAssigners = new AssignField<object, object?>[BaseFieldDefinitions.Length];

            for (var i = 0; i < BaseFieldDefinitions.Length; i++)
            {
                var fieldDefinition = BaseFieldDefinitions[i];

                fieldAccessors[i] = EmitFieldAccessor(fieldDefinition);
                fieldAssigners[i] = EmitFieldAssigner(fieldDefinition);
            }

            FieldAccessors = fieldAccessors.ToImmutableArray();
            FieldAssigners = fieldAssigners.ToImmutableArray();
        }

        public Type Type { get; }

        private string[] Duplicates { get; }
        private object?[] DefaultValues { get; }

        private ImmutableArray<AccessField<object, object?>> FieldAccessors { get; }
        private ImmutableArray<AssignField<object, object?>> FieldAssigners { get; }

        internal ImmutableArray<FieldDefinition> BaseFieldDefinitions { get; }

        public DeserializationResult Populate(object target, DeserializedFieldEntry[] fields)
        {
            return _populate(target, fields, DefaultValues);
        }

        public DeserializationResult Populate(
            object target,
            MappingDataNode mapping,
            ISerializationManager serialization,
            ISerializationContext? context,
            bool skipHook)
        {
            var fields = _deserialize(mapping, serialization, context, skipHook);
            return _populate(target, fields, DefaultValues);
        }

        public MappingDataNode Serialize(
            object obj,
            ISerializationManager serialization,
            ISerializationContext? context,
            bool alwaysWrite)
        {
            return _serialize(obj, serialization, context, alwaysWrite, DefaultValues);
        }

        public object Copy(
            object source,
            object target,
            ISerializationManager serialization,
            ISerializationContext? context)
        {
            return _copy(source, target, serialization, context);
        }

        public ValidationNode Validate(
            ISerializationManager serialization,
            MappingDataNode mapping,
            ISerializationContext? context)
        {
            var validatedMapping = new Dictionary<ValidationNode, ValidationNode>();

            foreach (var (key, val) in mapping.Children)
            {
                if (key is not ValueDataNode valueDataNode)
                {
                    validatedMapping.Add(new ErrorNode(key, "Key not ValueDataNode."), new InconclusiveNode(val));
                    continue;
                }

                var field = BaseFieldDefinitions.FirstOrDefault(f => f.Tag == valueDataNode.Value);
                if (field == null)
                {
                    var error = new ErrorNode(
                        key,
                        $"Field \"{valueDataNode.Value}\" not found in \"{Type}\".",
                        false);

                    validatedMapping.Add(error, new InconclusiveNode(val));
                    continue;
                }

                var keyValidated = serialization.ValidateNode(typeof(string), key, context);
                ValidationNode valValidated = field.Attribute.CustomTypeSerializer != null
                    ? serialization.ValidateNodeWith(field.FieldType,
                        field.Attribute.CustomTypeSerializer, val, context)
                    : serialization.ValidateNode(field.FieldType, val, context);

                validatedMapping.Add(keyValidated, valValidated);
            }

            return new ValidatedMappingNode(validatedMapping);
        }

        public bool CanCallWith(object obj) => Type.IsInstanceOfType(obj);

        public bool TryGetDuplicates([NotNullWhen(true)] out string[] duplicates)
        {
            duplicates = Duplicates;
            return duplicates.Length > 0;
        }

        private List<FieldDefinition> GetFieldDefinitions()
        {
            var dummyObject = Activator.CreateInstance(Type) ?? throw new NullReferenceException();
            var fieldDefinitions = new List<FieldDefinition>();

            foreach (var abstractFieldInfo in Type.GetAllPropertiesAndFields())
            {
                if (abstractFieldInfo.IsBackingField())
                {
                    continue;
                }

                if (!abstractFieldInfo.TryGetAttribute(out DataFieldAttribute? dataField, true))
                {
                    continue;
                }

                // Default to lowercased field name from C# if no tag name is provided.
                // Tag names will be lowerCamelCase.
                // This doesn't handle things like "ID".
                var tag = dataField.Tag ?? abstractFieldInfo.MemberInfo.Name.FirstCharToLowerCase();

                var backingField = abstractFieldInfo;

                if (abstractFieldInfo is SpecificPropertyInfo propertyInfo)
                {
                    // We only want the most overriden instance of a property for the type we are working with
                    if (!propertyInfo.IsMostOverridden(Type))
                    {
                        continue;
                    }

                    if (propertyInfo.PropertyInfo.GetMethod == null)
                    {
                        Logger.ErrorS(LogCategory, $"Property {propertyInfo} is annotated with DataFieldAttribute but has no getter");
                        continue;
                    }
                    else if (propertyInfo.PropertyInfo.SetMethod == null)
                    {
                        if (!propertyInfo.TryGetBackingField(out var backingFieldInfo))
                        {
                            Logger.ErrorS(LogCategory, $"Property {propertyInfo} in type {propertyInfo.DeclaringType} is annotated with DataFieldAttribute as non-readonly but has no auto-setter");
                            continue;
                        }

                        backingField = backingFieldInfo;
                    }
                }

                var inheritanceBehaviour = InheritanceBehavior.Default;
                if (abstractFieldInfo.HasAttribute<AlwaysPushInheritanceAttribute>(true))
                {
                    inheritanceBehaviour = InheritanceBehavior.Always;
                }
                else if (abstractFieldInfo.HasAttribute<NeverPushInheritanceAttribute>(true))
                {
                    inheritanceBehaviour = InheritanceBehavior.Never;
                }

                var fieldDefinition = new FieldDefinition(
                    dataField,
                    tag,
                    abstractFieldInfo.GetValue(dummyObject),
                    abstractFieldInfo,
                    backingField,
                    inheritanceBehaviour);

                fieldDefinitions.Add(fieldDefinition);
            }

            return fieldDefinitions;
        }
    }
}
