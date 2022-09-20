using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Manager.Definition;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Sequence;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Serialization.Manager
{
    public partial class SerializationManager : ISerializationManagerInternal
    {
        [IoC.Dependency] private readonly IReflectionManager _reflectionManager = default!;

        public const string LogCategory = "serialization";

        public bool IsValidatingOnly { get; set; }

        private bool _initializing;
        private bool _initialized;

        // Using CWT<,> here in case we ever want assembly unloading.
        private static readonly ConditionalWeakTable<Type, DataDefinition> DataDefinitions = new();
        private readonly HashSet<Type> _copyByRefRegistrations = new();

        public IDependencyCollection DependencyCollection { get; private set; } = default!;

        public void Initialize()
        {
            if (_initializing)
                throw new InvalidOperationException($"{nameof(SerializationManager)} is already being initialized.");

            if (_initialized)
                throw new InvalidOperationException($"{nameof(SerializationManager)} has already been initialized.");

            _initializing = true;

            DependencyCollection = IoCManager.Instance ?? throw new NullReferenceException();

            var flagsTypes = new ConcurrentBag<Type>();
            var constantsTypes = new ConcurrentBag<Type>();
            var typeSerializers = new ConcurrentBag<Type>();
            var meansDataDef = new ConcurrentBag<Type>();
            var implicitDataDefForInheritors = new ConcurrentBag<Type>();

            CollectAttributedTypes(flagsTypes, constantsTypes, typeSerializers, meansDataDef, implicitDataDefForInheritors);

            InitializeFlagsAndConstants(flagsTypes, constantsTypes);
            InitializeTypeSerializers(typeSerializers);

            // This is a bag, not a hash set.
            // Duplicates are fine since the CWT<,> won't re-run the constructor if it's already in there.
            var registrations = new ConcurrentBag<Type>();

            foreach (var baseType in implicitDataDefForInheritors)
            {
                // Inherited attributes don't work with interfaces.
                if (baseType.IsInterface)
                {
                    foreach (var child in _reflectionManager.GetAllChildren(baseType))
                    {
                        if (child.IsAbstract || child.IsGenericTypeDefinition)
                            continue;

                        registrations.Add(child);
                    }
                }
                else if (!baseType.IsAbstract && !baseType.IsGenericTypeDefinition)
                {
                    registrations.Add(baseType);
                }
            }

            Parallel.ForEach(_reflectionManager.FindAllTypes(), type =>
            {
                foreach (var meansDataDefAttr in meansDataDef)
                {
                    if (type.IsDefined(meansDataDefAttr))
                        registrations.Add(type);
                }
            });

            var sawmill = Logger.GetSawmill(LogCategory);

            Parallel.ForEach(registrations, type =>
            {
                if (type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition)
                {
                    sawmill.Debug(
                        $"Skipping registering data definition for type {type} since it is abstract or an interface");
                    return;
                }

                if (!type.IsValueType && type.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                    .FirstOrDefault(m => m.GetParameters().Length == 0) == null)
                {
                    sawmill.Debug(
                        $"Skipping registering data definition for type {type} since it has no parameterless ctor");
                    return;
                }

                DataDefinitions.GetValue(type, t => CreateDataDefinition(t, DependencyCollection));
            });

            var error = new StringBuilder();

            foreach (var (type, definition) in DataDefinitions)
            {
                if (definition.TryGetDuplicates(out var definitionDuplicates))
                {
                    error.Append($"{type}: [{string.Join(", ", definitionDuplicates)}]\n");
                }
            }

            if (error.Length > 0)
            {
                throw new ArgumentException($"Duplicate data field tags found in:\n{error}");
            }

            _copyByRefRegistrations.Add(typeof(Type));

            _initialized = true;
            _initializing = false;
        }

        private void CollectAttributedTypes(
            ConcurrentBag<Type> flagsTypes,
            ConcurrentBag<Type> constantsTypes,
            ConcurrentBag<Type> typeSerializers,
            ConcurrentBag<Type> meansDataDef,
            ConcurrentBag<Type> implicitDataDefForInheritors)
        {
            // IsDefined is extremely slow. Great.
            Parallel.ForEach(_reflectionManager.FindAllTypes(), type =>
            {
                if (type.IsDefined(typeof(FlagsForAttribute), false))
                    flagsTypes.Add(type);

                if (type.IsDefined(typeof(ConstantsForAttribute), false))
                    constantsTypes.Add(type);

                if (type.IsDefined(typeof(TypeSerializerAttribute)))
                    typeSerializers.Add(type);

                if (type.IsDefined(typeof(MeansDataDefinitionAttribute)))
                    meansDataDef.Add(type);

                if (type.IsDefined(typeof(ImplicitDataDefinitionForInheritorsAttribute), true))
                    implicitDataDefForInheritors.Add(type);

                if (type.IsDefined(typeof(CopyByRefAttribute)))
                    _copyByRefRegistrations.Add(type);
            });
        }

        private static DataDefinition CreateDataDefinition(Type t, IDependencyCollection collection)
        {
            return new(t, collection);
        }

        public void Shutdown()
        {
            DependencyCollection = null!;

            _constantsMapping.Clear();
            _flagsMapping.Clear();

            _genericWriterTypes.Clear();
            _genericReaderTypes.Clear();
            _genericCopierTypes.Clear();
            _genericValidatorTypes.Clear();

            _typeWriters.Clear();
            _typeReaders.Clear();
            _typeCopiers.Clear();
            _typeValidators.Clear();

            DataDefinitions.Clear();

            _copyByRefRegistrations.Clear();

            _highestFlagBit.Clear();

            _readers.Clear();

            _initialized = false;
        }

        public bool HasDataDefinition(Type type)
        {
            if (type.IsGenericTypeDefinition) throw new NotImplementedException($"Cannot yet check data definitions for generic types. ({type})");
            return DataDefinitions.TryGetValue(type, out _);
        }

        private bool HasGenericWriter(Type type)
        {
            var typeDef = type.GetGenericTypeDefinition();

            foreach (var (key, val) in _genericWriterTypes)
            {
                if (typeDef.HasSameMetadataDefinitionAs(key))
                    return true;
            }

            return false;
        }

        public bool CanSerializeType(Type type)
        {
            if (type.IsArray)
                return CanSerializeType(type.GetElementType()!);

            var hasTypeSerializer =
                _typeWriters.ContainsKey(type)
                    && _typeReaders.Keys.Select(v => v.Type).Contains(type)
                    && _typeComparers.ContainsKey(type)
                    && _typeValidators.Keys.Select(v => v.Type).Contains(type)
                    && _typeCopiers.ContainsKey(type);

            if (type.IsGenericType)
            {
                hasTypeSerializer |= HasGenericWriter(type);
            }

            return HasDataDefinition(type) || hasTypeSerializer;
        }

        public ValidationNode ValidateNode(Type type, DataNode node, ISerializationContext? context = null)
        {
            var underlyingType = type.EnsureNotNullableType();

            if (underlyingType.IsArray)
            {
                if (node is not SequenceDataNode sequenceDataNode) return new ErrorNode(node, "Invalid nodetype for array.", true);
                var elementType = underlyingType.GetElementType();
                if (elementType == null)
                    throw new ArgumentException($"Failed to get elementtype of arraytype {underlyingType}", nameof(underlyingType));
                var validatedList = new List<ValidationNode>();
                foreach (var dataNode in sequenceDataNode.Sequence)
                {
                    validatedList.Add(ValidateNode(elementType, dataNode, context));
                }

                return new ValidatedSequenceNode(validatedList);
            }

            if (underlyingType.IsEnum)
            {
                var enumName = node switch
                {
                    ValueDataNode valueNode => valueNode.Value,
                    SequenceDataNode sequenceNode => string.Join(", ", sequenceNode.Sequence),
                    _ => null
                };

                if (enumName == null)
                {
                    return new ErrorNode(node, $"Invalid node type {node.GetType().Name} for enum {underlyingType}.");
                }

                if (!Enum.TryParse(underlyingType, enumName, true, out var enumValue))
                {
                    return new ErrorNode(node, $"{enumValue} is not a valid enum value of type {underlyingType}", false);
                }

                return new ValidatedValueNode(node);
            }

            if (node.Tag?.StartsWith("!type:") ?? false)
            {
                var typeString = node.Tag.Substring(6);
                try
                {
                    underlyingType = ResolveConcreteType(underlyingType, typeString);
                }
                catch (InvalidOperationException)
                {
                    return new ErrorNode(node, $"Failed to resolve !type tag: {typeString}", false);
                }
            }

            if (TryValidateWithTypeValidator(underlyingType, node, DependencyCollection, context, out var valid)) return valid;

            if (typeof(ISelfSerialize).IsAssignableFrom(underlyingType))
                return node is ValueDataNode valueDataNode ? new ValidatedValueNode(valueDataNode) : new ErrorNode(node, "Invalid nodetype for ISelfSerialize", true);

            if (TryGetDefinition(underlyingType, out var dataDefinition))
            {
                return node switch
                {
                    ValueDataNode valueDataNode => valueDataNode.Value == "" ? new ValidatedValueNode(valueDataNode) : new ErrorNode(node, "Invalid nodetype for Datadefinition", false),
                    MappingDataNode mappingDataNode => dataDefinition.Validate(this, mappingDataNode, context),
                    _ => new ErrorNode(node, "Invalid nodetype for Datadefinition", true)
                };
            }

            return new ErrorNode(node, "Failed to read node.", false);
        }

        public ValidationNode ValidateNode<T>(DataNode node, ISerializationContext? context = null)
        {
            return ValidateNode(typeof(T), node, context);
        }

        public ValidationNode ValidateNodeWith(Type type, Type typeSerializer, DataNode node,
            ISerializationContext? context = null)
        {
            var method =
                typeof(SerializationManager).GetRuntimeMethods().First(m => m.Name == nameof(ValidateWithSerializer))!.MakeGenericMethod(
                    type, node.GetType(), typeSerializer);
            return (ValidationNode)method.Invoke(this, new object?[] { node, context })!;
        }

        public ValidationNode ValidateNodeWith<TType, TSerializer, TNode>(TNode node,
            ISerializationContext? context = null)
            where TSerializer : ITypeValidator<TType, TNode>
            where TNode : DataNode
        {
            return ValidateNodeWith(typeof(TType), typeof(TSerializer), node, context);
        }

        internal DataDefinition? GetDefinition<T>()
        {
            return GetDefinition(typeof(T));
        }

        internal DataDefinition? GetDefinition(Type type)
        {
            return DataDefinitions.TryGetValue(type, out var dataDefinition)
                ? dataDefinition
                : null;
        }

        internal bool TryGetDefinition(Type type, [NotNullWhen(true)] out DataDefinition? dataDefinition)
        {
            dataDefinition = GetDefinition(type);
            return dataDefinition != null;
        }

        public DataNode WriteValue<T>(T value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            return WriteValue(typeof(T), value, alwaysWrite, context);
        }

        public DataNode WriteValue(Type type, object? value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            if (underlyingType.IsArray)
            {
                if (underlyingType.GetArrayRank() == 1)
                {
                    if (value == null)
                        return new ValueDataNode("null");

                    var sequenceNode = new SequenceDataNode();
                    var array = (Array)value;

                    foreach (var val in array)
                    {
                        var serializedVal = WriteValue(val.GetType(), val, alwaysWrite, context);
                        sequenceNode.Add(serializedVal);
                    }

                    return sequenceNode;
                }
                else
                {
                    // Serialize the multidimensional array like so:
                    // ["elements"] => { { 1, 2, 3 }, { 4, 5, 6 } }
                    // ["lengths"] => { 3, 2 }

                    DataNode elementsNode;
                    SequenceDataNode lengthsNode;

                    if (value == null)
                    {
                        elementsNode = new ValueDataNode("null");
                        lengthsNode = new SequenceDataNode();

                        for (int i = 0; i < type.GetArrayRank(); i++)
                        {
                            lengthsNode.Add(new ValueDataNode("0"));
                        }
                    }
                    else
                    {
                        var seqElementsNode = new SequenceDataNode();
                        elementsNode = seqElementsNode;
                        var array = (Array)value;

                        foreach (var val in array)
                        {
                            var serializedVal = WriteValue(val.GetType(), val, alwaysWrite, context);
                            seqElementsNode.Add(serializedVal);
                        }

                        lengthsNode = new SequenceDataNode();

                        for (int i = 0; i < array.Rank; i++)
                        {
                            lengthsNode.Add(new ValueDataNode(array.GetLongLength(i).ToString()));
                        }
                    }

                    var mappingNode = new MappingDataNode(new Dictionary<DataNode, DataNode>()
                {
                    {new ValueDataNode("lengths"), lengthsNode},
                    {new ValueDataNode("elements"), elementsNode}
                });

                    return mappingNode;
                }
            }

            if (value == null) return new MappingDataNode();

            if (underlyingType.IsEnum)
            {
                // Enums implement IConvertible.
                // Need it for the culture overload.
                var convertible = (IConvertible)value;
                return new ValueDataNode(convertible.ToString(CultureInfo.InvariantCulture));
            }

            if (value is ISerializationHooks serHook)
                serHook.BeforeSerialization();

            if (TryWriteRaw(underlyingType, value, out var node, alwaysWrite, context))
            {
                return node;
            }

            if (typeof(ISelfSerialize).IsAssignableFrom(underlyingType))
            {
                var selfSerObj = (ISelfSerialize)value;
                return new ValueDataNode(selfSerObj.Serialize());
            }

            var currentType = underlyingType;
            var mapping = new MappingDataNode();
            if (underlyingType.IsAbstract || underlyingType.IsInterface)
            {
                mapping.Tag = $"!type:{value.GetType().Name}";
                currentType = value.GetType();
            }

            if (!TryGetDefinition(currentType, out var dataDef))
            {
                throw new InvalidOperationException($"No data definition found for type {type} when writing");
            }

            if (dataDef.CanCallWith(value) != true)
            {
                throw new ArgumentException($"Supplied value does not fit with data definition of {type}.");
            }

            var newMapping = dataDef.Serialize(value, this, context, alwaysWrite);
            mapping = mapping.Merge(newMapping);

            return mapping;
        }

        public DataNode WriteWithTypeSerializer(Type type, Type serializer, object? value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            // TODO Serialization: just return null
            if (type.IsNullable() && value == null) return new MappingDataNode();

            return WriteWithSerializerRaw(type, serializer, value!, context, alwaysWrite);
        }

        private void CopyToTarget(object source, ref object target, ISerializationContext? context = null, bool skipHook = false)
        {
            var sourceType = source.GetType();
            var targetType = target.GetType();

            if (sourceType.IsValueType && targetType.IsValueType)
            {
                target = source;
                return;
            }

            if (sourceType.IsValueType != targetType.IsValueType)
            {
                throw new InvalidOperationException(
                    $"Source and target do not match. Source ({sourceType}) is value type? {sourceType.IsValueType}. Target ({targetType}) is value type? {targetType.IsValueType}");
            }

            if (sourceType.IsArray && targetType.IsArray)
            {
                var sourceArray = (Array)source;
                var targetArray = (Array)target;
                var elementType = sourceArray.GetType().GetElementType()!;

                Array newArray;

                if (sourceArray.Rank != targetArray.Rank)
                {
                    throw new InvalidOperationException(
                        $"Source and target arrays must have the same dimensions. Source: {sourceArray.Rank}, Target: {targetArray.Rank}");
                }

                if (sourceArray.Rank == 1)
                {
                    if (sourceArray.Length == targetArray.Length)
                    {
                        newArray = targetArray;
                    }
                    else
                    {
                        newArray = Array.CreateInstance(elementType, sourceArray.Length);
                    }
                }
                else
                {
                    newArray = Array.CreateInstance(elementType, sourceArray.GetLongLengths());
                }

                if (sourceArray.Rank == 1)
                {
                    for (var i = 0; i < sourceArray.LongLength; i++)
                    {
                        newArray.SetValue(Copy(sourceArray.GetValue(i), context, skipHook), i);
                    }
                }
                else
                {
                    var indices = new long[sourceArray.Rank];
                    var cumulativeLengths = sourceArray.GetCumulativeLengths();

                    for (var i = 0; i < sourceArray.LongLength; i++)
                    {
                        for (int dim = sourceArray.Rank - 1; dim >= 0; dim--)
                        {
                            indices[dim] = i / cumulativeLengths[dim] % sourceArray.GetLongLength(dim);
                        }
                        newArray.SetValue(Copy(sourceArray.GetValue(indices), context, skipHook), indices);
                    }
                }

                target = newArray;
                return;
            }

            // hack to work around System.Type fields showing up as System.RuntimeType at runtime
            // since it causes TypeSerializer to be ignored (System.Type is abstract and
            // System.RuntimeType is private)
            if (typeof(Type).IsAssignableFrom(sourceType)) sourceType = typeof(Type);
            if (typeof(Type).IsAssignableFrom(targetType)) targetType = typeof(Type);

            if (sourceType.IsArray != targetType.IsArray)
            {
                throw new InvalidOperationException(
                    $"Source and target do not match. Source ({sourceType}) is array type? {sourceType.IsArray}. Target ({targetType}) is array type? {targetType.IsArray}");
            }

            if (!TypeHelpers.TrySelectCommonType(sourceType, targetType, out var commonType))
            {
                throw new InvalidOperationException("Could not find common type in Copy!");
            }

            if (_copyByRefRegistrations.Contains(commonType) || commonType.IsEnum)
            {
                target = source;
                return;
            }

            if (TryCopyRaw(commonType, source, ref target, skipHook, context))
            {
                return;
            }

            if (target is IPopulateDefaultValues populateDefaultValues)
            {
                populateDefaultValues.PopulateDefaultValues();
            }

            if (!TryGetDefinition(commonType, out var dataDef))
            {
                throw new InvalidOperationException($"No data definition found for type {commonType} when copying");
            }

            target = dataDef.Copy(source, target, this, context);

            if (!skipHook && target is ISerializationHooks afterHooks)
            {
                afterHooks.AfterDeserialization();
            }
        }

        public void Copy(object? source, ref object? target, ISerializationContext? context = null, bool skipHook = false)
        {
            if (target == null || source == null)
            {
                target = Copy(source, context, skipHook);
            }
            else
            {
                CopyToTarget(source, ref target, context, skipHook);
            }
        }

        public void Copy<T>(T source, ref T target, ISerializationContext? context = null, bool skipHook = false)
        {
            var temp = (object?)target;
            Copy(source, ref temp, context, skipHook);
            target = (T)temp!;
        }

        [MustUseReturnValue]
        public object? CopyWithTypeSerializer(Type typeSerializer, object? source, object? target,
            ISerializationContext? context = null, bool skipHook = false)
        {
            if (source == null || target == null) return source;

            return CopyWithSerializerRaw(typeSerializer, source, ref target, skipHook, context);
        }

        private object CreateCopyInternal(Type type, object source, ISerializationContext? context = null, bool skipHook = false)
        {
            if (type.IsPrimitive ||
                type.IsEnum ||
                source is string ||
                _copyByRefRegistrations.Contains(type))
            {
                return source;
            }

            var target = Activator.CreateInstance(source.GetType())!;
            CopyToTarget(source, ref target, context, skipHook);
            return target;
        }

        public object? Copy(object? source, ISerializationContext? context = null, bool skipHook = false)
        {
            if (source == null) return null;
            return CreateCopyInternal(source.GetType(), source, context, skipHook);
        }

        public T Copy<T>(T source, ISerializationContext? context = null, bool skipHook = false)
        {
            return (T)Copy((object?)source, context, skipHook)!;
        }

        public bool Compare(object? left, object? right, ISerializationContext? context = null, bool skipHook = false)
        {
            if (left == null && right == null)
                return true;

            if (left == null || right == null)
                return false;

            var leftType = left.GetType();
            var rightType = right.GetType();

            if (leftType != rightType)
            {
                return false;
            }

            if (leftType.IsArray != rightType.IsArray)
            {
                return false;
            }

            if (leftType.IsArray && rightType.IsArray)
            {
                var leftArray = (Array)left;
                var rightArray = (Array)right;

                if (leftArray.Rank != rightArray.Rank)
                {
                    return false;
                }

                for (int i = 0; i < leftArray.Rank; i++)
                {
                    if (leftArray.GetLongLength(i) != rightArray.GetLongLength(i))
                    {
                        return false;
                    }
                }

                if (leftArray.Rank == 1)
                {
                    for (var i = 0; i < leftArray.LongLength; i++)
                    {
                        if (!Compare(leftArray.GetValue(i), rightArray.GetValue(i), context, skipHook))
                            return false;
                    }
                }
                else
                {
                    var indices = new long[leftArray.Rank];
                    var cumulativeLengths = leftArray.GetCumulativeLengths();

                    for (var i = 0; i < leftArray.LongLength; i++)
                    {
                        for (int dim = leftArray.Rank - 1; dim >= 0; dim--)
                        {
                            indices[dim] = i / cumulativeLengths[dim] % leftArray.GetLongLength(dim);
                        }
                        if (!Compare(leftArray.GetValue(indices), rightArray.GetValue(indices), context, skipHook))
                            return false;
                    }
                }

                return true;
            }

            if (leftType.IsEnum)
            {
                return left.Equals(right);
            }

            if (TryCompareRaw(leftType, left, right, out var result, skipHook, context))
            {
                return result;
            }

            if (!TryGetDefinition(leftType, out var dataDef))
            {
                throw new InvalidOperationException($"No data definition found for type {leftType} when comparing");
            }

            var areSame = dataDef.Compare(left, right, this, context);

            if (!skipHook && left is ISerializationHooks leftAfterHooks)
            {
                areSame &= leftAfterHooks.AfterCompare(right);
            }

            return areSame;
        }

        private static Type ResolveConcreteType(Type baseType, string typeName)
        {
            var reflection = IoCManager.Resolve<IReflectionManager>();
            var type = reflection.YamlTypeTagLookup(baseType, typeName);
            if (type == null)
            {
                throw new InvalidOperationException($"Type '{baseType}' is abstract, but could not find concrete type '{typeName}'.");
            }

            return type;
        }
    }
}
