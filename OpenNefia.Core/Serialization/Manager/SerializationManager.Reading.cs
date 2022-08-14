using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using ICSharpCode.Decompiler.IL;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Definition;
using OpenNefia.Core.Serialization.Manager.Result;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Sequence;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;
using OpenNefia.Core.Utility;
using YamlDotNet.Core.Tokens;

namespace OpenNefia.Core.Serialization.Manager
{
    public partial class SerializationManager
    {
        private delegate DeserializationResult ReadDelegate(
            Type type,
            DataNode node,
            ISerializationContext? context = null,
            bool skipHook = false);

        private readonly ConcurrentDictionary<(Type value, Type node), ReadDelegate> _readers = new();

        private ReadDelegate GetOrCreateReader(Type value, DataNode node)
        {
            if (node.Tag?.StartsWith("!type:") ?? false)
            {
                var typeString = node.Tag.Substring(6);
                value = ResolveConcreteType(value, typeString);
            }

            return _readers.GetOrAdd((value, node.GetType()), static (tuple, vfArgument) =>
            {
                var (value, nodeType) = tuple;
                var (node, instance) = vfArgument;

                var nullable = value.IsNullable();
                value = value.EnsureNotNullableType();

                var instanceConst = Expression.Constant(instance);

                var typeParam = Expression.Parameter(typeof(Type), "type");
                var nodeParam = Expression.Parameter(typeof(DataNode), "node");
                var contextParam = Expression.Parameter(typeof(ISerializationContext), "context");
                var skipHookParam = Expression.Parameter(typeof(bool), "skipHook");

                MethodCallExpression call;

                if (value.IsArray)
                {
                    var elementType = value.GetElementType()!;

                    switch (node)
                    {
                        // BUG: does this even work?
                        // typeof(T[]?) acts as typeof(T[]) at runtime.
                        // https://stackoverflow.com/a/62186551
                        case ValueDataNode when nullable:
                            call = Expression.Call(
                                instanceConst,
                                nameof(ReadArrayValue),
                                new[] { elementType },
                                Expression.Convert(nodeParam, typeof(ValueDataNode)));
                            break;
                        case SequenceDataNode seqNode:
                            var isSealed = elementType.IsPrimitive || elementType.IsEnum ||
                                           elementType == typeof(string) || elementType.IsSealed;

                            if (isSealed && seqNode.Sequence.Count > 0)
                            {
                                var reader = instance.GetOrCreateReader(elementType, seqNode.Sequence[0]);
                                var readerConst = Expression.Constant(reader);

                                call = Expression.Call(
                                    instanceConst,
                                    nameof(ReadArraySequenceSealed),
                                    new[] { elementType },
                                    Expression.Convert(nodeParam, typeof(SequenceDataNode)),
                                    readerConst,
                                    contextParam,
                                    skipHookParam);

                                break;
                            }

                            call = Expression.Call(
                                instanceConst,
                                nameof(ReadArraySequence),
                                new[] { elementType },
                                Expression.Convert(nodeParam, typeof(SequenceDataNode)),
                                contextParam,
                                skipHookParam);
                            break;

                        case MappingDataNode mappingNode:
                            var elementsNode = mappingNode["elements"];
                            switch (elementsNode)
                            {
                                // BUG: does this even work?
                                // typeof(T[]?) acts as typeof(T[]) at runtime.
                                // https://stackoverflow.com/a/62186551
                                case ValueDataNode when nullable:
                                    call = Expression.Call(
                                        instanceConst,
                                        nameof(ReadArrayValueMultiDim),
                                        new[] { elementType },
                                        Expression.Convert(nodeParam, typeof(MappingDataNode)));
                                    break;

                                case SequenceDataNode seqNode:
                                    var isSealed2 = elementType.IsPrimitive || elementType.IsEnum ||
                                                   elementType == typeof(string) || elementType.IsSealed;

                                    if (isSealed2 && seqNode.Sequence.Count > 0)
                                    {
                                        var reader = instance.GetOrCreateReader(elementType, seqNode.Sequence[0]);
                                        var readerConst = Expression.Constant(reader);

                                        call = Expression.Call(
                                            instanceConst,
                                            nameof(ReadArraySequenceSealedMultiDim),
                                            new[] { elementType },
                                            Expression.Convert(nodeParam, typeof(MappingDataNode)),
                                            readerConst,
                                            contextParam,
                                            skipHookParam);

                                        break;
                                    }

                                    call = Expression.Call(
                                        instanceConst,
                                        nameof(ReadArraySequenceMultiDim),
                                        new[] { elementType },
                                        Expression.Convert(nodeParam, typeof(MappingDataNode)),
                                        contextParam,
                                        skipHookParam);
                                    break;
                                default:
                                    throw new ArgumentException($"Cannot read array from data node type {elementsNode.GetType()}");
                            }
                            break;
                        default:
                            throw new ArgumentException($"Cannot read array from data node type {nodeType}");
                    }
                }
                else if (value.IsEnum)
                {
                    call = node switch
                    {
                        ValueDataNode when nullable => Expression.Call(
                            instanceConst,
                            nameof(ReadEnumNullable),
                            new[] { value },
                            Expression.Convert(nodeParam, typeof(ValueDataNode))),
                        ValueDataNode => Expression.Call(
                            instanceConst,
                            nameof(ReadEnumValue),
                            new[] { value },
                            Expression.Convert(nodeParam, typeof(ValueDataNode))),
                        SequenceDataNode => Expression.Call(
                            instanceConst,
                            nameof(ReadEnumSequence),
                            new[] { value },
                            Expression.Convert(nodeParam, typeof(SequenceDataNode))),
                        _ => throw new InvalidNodeTypeException(
                            $"Cannot serialize node as {value}, unsupported node type {node.GetType()}")
                    };
                }
                else if (typeof(Delegate).IsAssignableFrom(value))
                {
                    if (node is not ValueDataNode)
                    {
                        throw new InvalidNodeTypeException(
                            $"Cannot read {nameof(Delegate)} from node type {nodeType}. Expected {nameof(ValueDataNode)}");
                    }

                    call = value.IsValueType switch
                    {
                        true when nullable => call = Expression.Call(
                            instanceConst,
                            nameof(ReadDelegateNullableValue),
                            new[] { value },
                            Expression.Convert(nodeParam, typeof(ValueDataNode))),
                        _ => call = Expression.Call(
                            instanceConst,
                            nameof(ReadDelegateValue),
                            new[] { value },
                            Expression.Convert(nodeParam, typeof(ValueDataNode)))
                    };
                }
                else if (value.IsAssignableTo(typeof(ISelfSerialize)))
                {
                    if (node is not ValueDataNode)
                    {
                        throw new InvalidNodeTypeException(
                            $"Cannot read {nameof(ISelfSerialize)} from node type {nodeType}. Expected {nameof(ValueDataNode)}");
                    }

                    var instantiator = instance.GetOrCreateInstantiator(value) ?? throw new NullReferenceException($"No instantiator could be made for type {value} (does it have a zero-argument constructor?)");
                    var instantiatorConst = Expression.Constant(instantiator);

                    call = value.IsValueType switch
                    {
                        true when nullable => call = Expression.Call(
                            instanceConst,
                            nameof(ReadSelfSerializeNullableStruct),
                            new[] { value },
                            Expression.Convert(nodeParam, typeof(ValueDataNode)),
                            instantiatorConst),
                        _ => call = Expression.Call(
                            instanceConst,
                            nameof(ReadSelfSerialize),
                            new[] { value },
                            Expression.Convert(nodeParam, typeof(ValueDataNode)),
                            instantiatorConst)
                    };
                }
                else if (instance.TryGetTypeReader(value, nodeType, out var reader))
                {
                    var readerType = typeof(ITypeReader<,>).MakeGenericType(value, nodeType);
                    var readerConst = Expression.Constant(reader, readerType);

                    call = node switch
                    {
                        ValueDataNode when nullable && value.IsValueType => Expression.Call(
                            instanceConst,
                            nameof(ReadWithTypeReaderNullableStruct),
                            new[] { value },
                            Expression.Convert(nodeParam, typeof(ValueDataNode)),
                            readerConst,
                            contextParam,
                            skipHookParam),
                        ValueDataNode when nullable && !value.IsValueType => Expression.Call(
                            instanceConst,
                            nameof(ReadWithTypeReaderNullable),
                            new[] { value },
                            Expression.Convert(nodeParam, typeof(ValueDataNode)),
                            readerConst,
                            contextParam,
                            skipHookParam),
                        _ => Expression.Call(
                            instanceConst,
                            nameof(ReadWithTypeReader),
                            new[] { value, nodeType },
                            Expression.Convert(nodeParam, nodeType),
                            readerConst,
                            contextParam,
                            skipHookParam)
                    };
                }
                else if (value.IsInterface || value.IsAbstract)
                {
                    throw new ArgumentException($"Unable to create an instance of an interface or abstract type. Type: {value}");
                }
                else
                {
                    var definition = instance.GetDefinition(value);
                    var definitionConst = Expression.Constant(definition, typeof(DataDefinition));

                    var instantiator = instance.GetOrCreateInstantiator(value) ?? throw new NullReferenceException($"No instantiator could be made for type {value} (does it have a zero-argument constructor?)");
                    var instantiatorConst = Expression.Constant(instantiator);

                    var populateConst = Expression.Constant(value.IsAssignableTo(typeof(IPopulateDefaultValues)));
                    var hooksConst = Expression.Constant(value.IsAssignableTo(typeof(ISerializationHooks)));

                    call = node switch
                    {
                        ValueDataNode when nullable => Expression.Call(
                            instanceConst,
                            nameof(ReadGenericNullable),
                            new[] { value },
                            Expression.Convert(nodeParam, typeof(ValueDataNode)),
                            instantiatorConst,
                            definitionConst,
                            populateConst,
                            hooksConst,
                            contextParam,
                            skipHookParam),
                        ValueDataNode => Expression.Call(
                            instanceConst,
                            nameof(ReadGenericValue),
                            new[] { value },
                            Expression.Convert(nodeParam, typeof(ValueDataNode)),
                            instantiatorConst,
                            definitionConst,
                            populateConst,
                            hooksConst,
                            contextParam,
                            skipHookParam),
                        MappingDataNode => Expression.Call(
                            instanceConst,
                            nameof(ReadGenericMapping),
                            new[] { value },
                            Expression.Convert(nodeParam, typeof(MappingDataNode)),
                            instantiatorConst,
                            definitionConst,
                            populateConst,
                            hooksConst,
                            contextParam,
                            skipHookParam),
                        SequenceDataNode => throw new ArgumentException($"No mapping node provided for type {value} at line: {node.Start.Line}"),
                        _ => throw new ArgumentException($"Unknown node type {nodeType} provided. Expected mapping node at line: {node.Start.Line}")
                    };
                }

                return Expression.Lambda<ReadDelegate>(
                    call,
                    typeParam,
                    nodeParam,
                    contextParam,
                    skipHookParam).Compile();
            }, (node, this));
        }

        private DeserializationResult ReadArrayValue<T>(ValueDataNode value)
        {
            if (value.Value == "null")
            {
                return new DeserializedValue<T[]?>(null);
            }

            throw new InvalidNodeTypeException("Cannot read an array from a value data node that is not null.");
        }

        private DeserializationResult ReadArraySequence<T>(
            SequenceDataNode node,
            ISerializationContext? context = null,
            bool skipHook = false)
        {
            var type = typeof(T);
            var array = new T[node.Sequence.Count];
            var results = new DeserializationResult[node.Sequence.Count];

            for (var i = 0; i < node.Sequence.Count; i++)
            {
                var subNode = node.Sequence[i];
                var result = Read(type, subNode, context, skipHook);

                results[i] = result;
                array[i] = (T)result.RawValue!;
            }

            return new DeserializedArray(array, results);
        }

        private DeserializationResult ReadArraySequenceSealed<T>(
            SequenceDataNode node,
            ReadDelegate elementReader,
            ISerializationContext? context = null,
            bool skipHook = false)
        {
            var type = typeof(T);
            var array = new T[node.Sequence.Count];
            var results = new DeserializationResult[node.Sequence.Count];

            for (var i = 0; i < node.Sequence.Count; i++)
            {
                var subNode = node.Sequence[i];
                var result = elementReader(type, subNode, context, skipHook);

                results[i] = result;
                array[i] = (T)result.RawValue!;
            }

            return new DeserializedArray(array, results);
        }

        private DeserializationResult ReadArrayValueMultiDim<T>(MappingDataNode mapping)
        {
            var lengthsNode = (SequenceDataNode)mapping["lengths"];
            var rank = lengthsNode.Sequence.Count;
            var value = (ValueDataNode)mapping["elements"];
            if (value.Value == "null")
            {
                var arrayType = typeof(T).MakeArrayType(rank).EnsureNullableType();
                return (DeserializedValue)Activator.CreateInstance(arrayType, null)!;
            }

            throw new InvalidNodeTypeException("Cannot read an array from a value data node that is not null.");
        }

        private DeserializationResult ReadArraySequenceMultiDim<T>(
            MappingDataNode mapping,
            ISerializationContext? context = null,
            bool skipHook = false)
        {
            var lengthsNode = (SequenceDataNode)mapping["lengths"];
            var lengths = new long[lengthsNode.Sequence.Count];
            for (int i = 0; i < lengths.Length; i++)
            {
                lengths[i] = long.Parse(((ValueDataNode)lengthsNode[i]).Value);
            }

            var node = (SequenceDataNode)mapping["elements"];
            var type = typeof(T);
            var array = Array.CreateInstance(type, lengths);
            var results = new DeserializationResult[node.Sequence.Count];

            var indices = new long[array.Rank];
            var cumulativeLengths = array.GetCumulativeLengths();

            for (var i = 0; i < node.Sequence.Count; i++)
            {
                var subNode = node.Sequence[i];
                var result = Read(type, subNode, context, skipHook);

                for (int dim = array.Rank - 1; dim >= 0; dim--)
                {
                    indices[dim] = i / cumulativeLengths[dim] % lengths[dim];
                }

                results[i] = result;
                array.SetValue(result.RawValue!, indices);
            }

            return new DeserializedArray(array, results);
        }

        private DeserializationResult ReadArraySequenceSealedMultiDim<T>(
            MappingDataNode mapping,
            ReadDelegate elementReader,
            ISerializationContext? context = null,
            bool skipHook = false)
        {
            var lengthsNode = (SequenceDataNode)mapping["lengths"];
            var lengths = new long[lengthsNode.Sequence.Count];
            for (int i = 0; i < lengths.Length; i++)
            {
                lengths[i] = long.Parse(((ValueDataNode)lengthsNode[i]).Value);
            }

            var node = (SequenceDataNode)mapping["elements"];
            var type = typeof(T);
            var array = Array.CreateInstance(type, lengths);
            var results = new DeserializationResult[node.Sequence.Count];

            var indices = new long[array.Rank];
            var cumulativeLengths = array.GetCumulativeLengths();

            for (var i = 0; i < node.Sequence.Count; i++)
            {
                var subNode = node.Sequence[i];
                var result = elementReader(type, subNode, context, skipHook);

                for (int dim = array.Rank - 1; dim >= 0; dim--)
                {
                    indices[dim] = i / cumulativeLengths[dim] % lengths[dim];
                }

                results[i] = result;
                array.SetValue(result.RawValue!, indices);
            }

            return new DeserializedArray(array, results);
        }

        private DeserializationResult ReadEnumNullable<TEnum>(ValueDataNode node) where TEnum : struct
        {
            if (node.Value == "null")
            {
                return new DeserializedValue<TEnum?>(null);
            }

            var value = Enum.Parse<TEnum>(node.Value, true);
            return new DeserializedValue<TEnum?>(value);
        }

        private DeserializationResult ReadEnumValue<TEnum>(ValueDataNode node) where TEnum : struct
        {
            var value = Enum.Parse<TEnum>(node.Value, true);
            return new DeserializedValue<TEnum>(value);
        }

        private DeserializationResult ReadEnumSequence<TEnum>(SequenceDataNode node) where TEnum : struct
        {
            var value = Enum.Parse<TEnum>(string.Join(", ", node.Sequence), true);
            return new DeserializedValue<TEnum>(value);
        }

        private DeserializationResult ReadDelegateNullableValue<TDelegate>(ValueDataNode node)
            where TDelegate : Delegate
        {
            if (node.Value == "null")
            {
                return new DeserializedValue<TDelegate?>(default);
            }

            return ReadDelegateValue<TDelegate>(node);
        }

        private DeserializationResult ReadDelegateValue<TDelegate>(ValueDataNode node)
            where TDelegate : Delegate
        {
            var split = node.Value.Split(':');
            if (split.Length != 2)
            {
                throw new ArgumentException($"Could not parse delegate name from string '{split}', it should have the value 'Namespace.Of.Type:MethodName'");
            }

            var systemTypeName = split[0];
            var methodName = split[1];

            if (!_reflectionManager.TryLooseGetType(systemTypeName, out var systemType))
            {
                throw new ArgumentException($"No type with loose typename '{systemTypeName}' found.");
            }

            if (systemType.IsInterface || systemType.IsAbstract)
            {
                throw new ArgumentException($"Delegate target type {systemType} cannot be an interface or abstract.");
            }

            if (!typeof(IEntitySystem).IsAssignableFrom(systemType))
            {
                throw new ArgumentException($"Delegate target type {systemType} does not implement {nameof(IEntitySystem)}.");
            }

            // End result should be equivalent to:
            // (...) => EntitySystem.Get<TSystem>().MethodName(...);

            var entitySystemGet = typeof(EntitySystem).GetMethod("Get", BindingFlags.Public | BindingFlags.Static, Type.EmptyTypes)!
                .MakeGenericMethod(systemType);

            var invoke = typeof(TDelegate).GetMethod("Invoke")!;
            var parameters = invoke.GetParameters()
                .Select(p => Expression.Parameter(p.ParameterType, p.Name))
                .ToList();
            var paramTypes = invoke.GetParameters()
                .Select(p => p.ParameterType)
                .ToArray();

            var method = systemType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance, paramTypes);

            if (method == null)
            {
                throw new ArgumentException($"Could not find public instance method '{methodName}' on type {systemType}.");
            }

            var call = Expression.Call(Expression.Call(entitySystemGet), method, parameters);

            var value = Expression.Lambda<TDelegate>(call, parameters).Compile();

            return new DeserializedValue<TDelegate?>(value);
        }

        private DeserializationResult ReadSelfSerialize<TValue>(
            ValueDataNode node,
            InstantiationDelegate<object> instantiator)
            where TValue : ISelfSerialize
        {
            if (node.Value == "null")
            {
                return new DeserializedValue<TValue?>(default);
            }

            var value = (TValue)instantiator();
            value.Deserialize(node.Value);

            return new DeserializedValue<TValue?>(value);
        }

        private DeserializationResult ReadSelfSerializeNullableStruct<TValue>(
            ValueDataNode node,
            InstantiationDelegate<object> instantiator)
            where TValue : struct, ISelfSerialize
        {
            if (node.Value == "null")
            {
                return new DeserializedValue<TValue?>(null);
            }

            var value = (TValue)instantiator();
            value.Deserialize(node.Value);

            return new DeserializedValue<TValue?>(value);
        }

        private DeserializationResult ReadWithTypeReaderNullable<TValue>(
            ValueDataNode node,
            ITypeReader<TValue, ValueDataNode> reader,
            ISerializationContext? context = null,
            bool skipHook = false)
        {
            if (node.Value == "null")
            {
                return new DeserializedValue<TValue?>(default);
            }

            return ReadWithTypeReader(node, reader, context, skipHook);
        }

        private DeserializationResult ReadWithTypeReaderNullableStruct<TValue>(
            ValueDataNode node,
            ITypeReader<TValue, ValueDataNode> reader,
            ISerializationContext? context = null,
            bool skipHook = false)
            where TValue : struct
        {
            if (node.Value == "null")
            {
                return new DeserializedValue<TValue?>(null);
            }

            return ReadWithTypeReader(node, reader, context, skipHook);
        }

        private DeserializationResult ReadWithTypeReader<TValue, TNode>(
            TNode node,
            ITypeReader<TValue, TNode> reader,
            ISerializationContext? context = null,
            bool skipHook = false)
            where TNode : DataNode
        {
            if (context != null &&
                context.TypeReaders.TryGetValue((typeof(TValue), typeof(TNode)), out var readerUnCast))
            {
                reader = (ITypeReader<TValue, TNode>)readerUnCast;
            }

            return reader.Read(this, node, DependencyCollection, skipHook, context);
        }

        private DeserializationResult ReadGenericNullable<TValue>(
            ValueDataNode node,
            InstantiationDelegate<object> instantiator,
            DataDefinition? definition,
            bool populate,
            bool hooks,
            ISerializationContext? context = null,
            bool skipHook = false)
        {
            if (node.Value == "null")
            {
                return new DeserializedValue<TValue?>(default);
            }

            return ReadGenericValue<TValue?>(node, instantiator, definition, populate, hooks, context, skipHook);
        }

        private DeserializationResult ReadGenericValue<TValue>(
            ValueDataNode node,
            InstantiationDelegate<object> instantiator,
            DataDefinition? definition,
            bool populate,
            bool hooks,
            ISerializationContext? context = null,
            bool skipHook = false)
        {
            var type = typeof(TValue);

            if (context != null &&
                context.TypeReaders.TryGetValue((typeof(TValue), typeof(ValueDataNode)), out var readerUnCast))
            {
                var reader = (ITypeReader<TValue, ValueDataNode>)readerUnCast;
                return reader.Read(this, node, DependencyCollection, skipHook, context);
            }

            if (definition == null)
            {
                throw new ArgumentException($"No data definition found for type {type} with node type {node.GetType()} when reading");
            }

            var instance = instantiator();

            if (populate)
            {
                ((IPopulateDefaultValues)instance).PopulateDefaultValues();
            }

            if (node.Value != string.Empty)
            {
                throw new ArgumentException($"No mapping node provided for type {type} at line: {node.Start.Line}");
            }

            // If we get an empty ValueDataNode we just use an empty mapping
            var mapping = new MappingDataNode();

            var result = definition.Populate(instance, mapping, this, context, skipHook);

            if (!skipHook && hooks)
            {
                ((ISerializationHooks)result.RawValue!).AfterDeserialization();
            }

            return result;
        }

        private DeserializationResult ReadGenericMapping<TValue>(
            MappingDataNode node,
            InstantiationDelegate<object> instantiator,
            DataDefinition? definition,
            bool populate,
            bool hooks,
            ISerializationContext? context = null,
            bool skipHook = false)
        {
            var type = typeof(TValue);
            var instance = instantiator();

            if (context != null &&
                context.TypeReaders.TryGetValue((type, typeof(MappingDataNode)), out var readerUnCast))
            {
                var reader = (ITypeReader<TValue, MappingDataNode>)readerUnCast;
                return reader.Read(this, node, DependencyCollection, skipHook, context);
            }

            if (definition == null)
            {
                throw new ArgumentException($"No data definition found for type {type} with node type {node.GetType()} when reading");
            }

            if (populate)
            {
                ((IPopulateDefaultValues)instance).PopulateDefaultValues();
            }

            var result = definition.Populate(instance, node, this, context, skipHook);

            if (!skipHook && hooks)
            {
                ((ISerializationHooks)result.RawValue!).AfterDeserialization();
            }

            return result;
        }

        public DeserializationResult Read(Type type, DataNode node, ISerializationContext? context = null, bool skipHook = false)
        {
            return GetOrCreateReader(type, node)(type, node, context, skipHook);
        }

        public object? ReadValue(Type type, DataNode node, ISerializationContext? context = null, bool skipHook = false)
        {
            return Read(type, node, context, skipHook).RawValue;
        }

        public T? ReadValueCast<T>(Type type, DataNode node, ISerializationContext? context = null, bool skipHook = false)
        {
            var value = Read(type, node, context, skipHook);

            if (value.RawValue == null)
            {
                return default;
            }

            return (T?)value.RawValue;
        }

        public T? ReadValue<T>(DataNode node, ISerializationContext? context = null, bool skipHook = false)
        {
            return ReadValueCast<T>(typeof(T), node, context, skipHook);
        }

        public DeserializationResult ReadWithTypeSerializer(Type value, Type serializer, DataNode node, ISerializationContext? context = null,
            bool skipHook = false)
        {
            return ReadWithSerializerRaw(value, node, serializer, context, skipHook);
        }
    }
}
