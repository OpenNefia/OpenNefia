using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using ICSharpCode.Decompiler.IL;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Definition;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Sequence;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;
using OpenNefia.Core.Utility;
using YamlDotNet.Core.Tokens;
using static ICSharpCode.Decompiler.TypeSystem.ReflectionHelper;

namespace OpenNefia.Core.Serialization.Manager
{
    public partial class SerializationManager
    {
        private delegate object? ReadDelegate(
            Type type,
            DataNode node,
            ISerializationContext? context = null,
            bool skipHook = false,
            object? value = null);

        private readonly ConcurrentDictionary<(Type value, Type node), ReadDelegate> _readers = new();

        public T Read<T>(DataNode node, ISerializationContext? context = null, bool skipHook = false, T? value = default) //todo paul this default should be null
        {
            return (T)Read(typeof(T), node, context, skipHook, value)!;
        }

        public object? Read(Type type, DataNode node, ISerializationContext? context = null, bool skipHook = false, object? value = null)
        {
            var val = GetOrCreateReader(type, node)(type, node, context, skipHook, value);
            ReadNullCheck(type, val);
            return val;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReadNullCheck(Type type, object? val)
        {
            if (!type.IsNullable() && val == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(Read)}-Call returned a null value for non-nullable type {type}");
            }
        }

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
                //todo paul serializers in the context should also override default serializers for array etc
                var contextParam = Expression.Parameter(typeof(ISerializationContext), "context");
                var skipHookParam = Expression.Parameter(typeof(bool), "skipHook");
                var valueParam = Expression.Parameter(typeof(object), "value");

                Expression call;

                if (value.IsEnum)
                {
                    call = node switch
                    {
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
                    call = node switch
                    {
                        MappingDataNode => Expression.Call(
                            instanceConst,
                            nameof(ReadDelegateMapping),
                            new[] { value },
                            Expression.Convert(nodeParam, typeof(MappingDataNode))),
                        _ => Expression.Call(
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
                        _ => call = Expression.Call(
                            instanceConst,
                            nameof(ReadSelfSerialize),
                            new[] { value },
                            Expression.Convert(nodeParam, typeof(ValueDataNode)),
                            instantiatorConst, valueParam)
                    };
                }
                else if (instance.TryGetTypeReader(value, nodeType, out var reader))
                {
                    var readerType = typeof(ITypeReader<,>).MakeGenericType(value, nodeType);
                    var readerConst = Expression.Constant(reader, readerType);

                    call = Expression.Call(
                        instanceConst,
                        nameof(ReadWithTypeReader),
                        new[] { value, nodeType },
                        Expression.Convert(nodeParam, nodeType),
                        readerConst,
                        contextParam,
                        skipHookParam,
                        valueParam);
                }
                else if (value.IsArray)
                {
                    var elementType = value.GetElementType()!;

                    switch (node)
                    {
                        case ValueDataNode:
                            call = Expression.Call(
                                instanceConst,
                                nameof(ReadArrayValue),
                                new[] { elementType },
                                Expression.Convert(nodeParam, typeof(ValueDataNode)),
                                contextParam,
                                skipHookParam);
                            break;
                        case SequenceDataNode seqNode:
                            var isSealed = elementType.IsPrimitive || elementType.IsEnum ||
                                           elementType == typeof(string) || elementType.IsSealed;

                            if (isSealed && seqNode.Sequence.Count > 0)
                            {
                                var arrayReader = instance.GetOrCreateReader(elementType, seqNode.Sequence[0]);
                                var arrayReaderConst = Expression.Constant(arrayReader);

                                call = Expression.Call(
                                    instanceConst,
                                    nameof(ReadArraySequenceSealed),
                                    new[] { elementType },
                                    Expression.Convert(nodeParam, typeof(SequenceDataNode)),
                                    arrayReaderConst,
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
                                case SequenceDataNode seqNode:
                                    var isSealed2 = elementType.IsPrimitive || elementType.IsEnum ||
                                                   elementType == typeof(string) || elementType.IsSealed;

                                    if (isSealed2 && seqNode.Sequence.Count > 0)
                                    {
                                        var arrayReader = instance.GetOrCreateReader(elementType, seqNode.Sequence[0]);
                                        var arrayReaderConst = Expression.Constant(arrayReader);

                                        call = Expression.Call(
                                            instanceConst,
                                            nameof(ReadArraySequenceSealedMultiDim),
                                            new[] { elementType },
                                            Expression.Convert(nodeParam, typeof(MappingDataNode)),
                                            arrayReaderConst,
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
                            skipHookParam,
                            valueParam),
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
                            skipHookParam,
                            valueParam),
                        SequenceDataNode => throw new ArgumentException($"No mapping node provided for type {value} at line: {node.Start.Line}"),
                        _ => throw new ArgumentException($"Unknown node type {nodeType} provided. Expected mapping node at line: {node.Start.Line}")
                    };
                }

                if (nullable)
                {
                    call = Expression.Condition(
                    Expression.Call(
                            instanceConst,
                            nameof(IsNull),
                            Type.EmptyTypes,
                            nodeParam),
                        Expression.Convert(value.IsValueType ? Expression.Call(instanceConst, nameof(GetNullable), new[] { value }) : Expression.Constant(null), typeof(object)),
                        Expression.Convert(call, typeof(object)));
                }
                else
                {
                    call = Expression.Convert(call, typeof(object));
                }

                return Expression.Lambda<ReadDelegate>(
                    call,
                    typeParam,
                    nodeParam,
                    contextParam,
                    skipHookParam,
                    valueParam).Compile();
            }, (node, this));
        }

        private T? GetNullable<T>() where T : struct
        {
            return null;
        }

        private bool IsNull(DataNode node)
        {
            return node is ValueDataNode valueDataNode && valueDataNode.Value.Trim().ToLower() is "null" or "";
        }

        private T[] ReadArrayValue<T>(
            ValueDataNode value,
            ISerializationContext? context = null,
            bool skipHook = false)
        {
            var array = new T[1];
            array[0] = Read<T>(value, context, skipHook);
            return array;
        }

        private T[] ReadArraySequence<T>(
            SequenceDataNode node,
            ISerializationContext? context = null,
            bool skipHook = false)
        {
            var array = new T[node.Sequence.Count];

            for (var i = 0; i < node.Sequence.Count; i++)
            {
                array[i] = Read<T>(node.Sequence[i], context, skipHook);
            }

            return array;
        }

        private T[] ReadArraySequenceSealed<T>(
            SequenceDataNode node,
            ReadDelegate elementReader,
            ISerializationContext? context = null,
            bool skipHook = false)
        {
            var type = typeof(T);
            var array = new T[node.Sequence.Count];

            for (var i = 0; i < node.Sequence.Count; i++)
            {
                var subNode = node.Sequence[i];
                var result = elementReader(type, subNode, context, skipHook);
                ReadNullCheck(type, result);
                array[i] = (T)result!;
            }

            return array;
        }

        private Array? ReadArrayValueMultiDim<T>(
            MappingDataNode mapping,
            ISerializationContext? context = null,
            bool skipHook = false)
        {
            var lengthsNode = (SequenceDataNode)mapping["lengths"];
            var rank = lengthsNode.Sequence.Count;
            var value = (ValueDataNode)mapping["elements"];
            if (value.Value == "null")
            {
                return null;
            }

            var array = Array.CreateInstance(typeof(T), 1);
            array.SetValue(Read<T>(value, context, skipHook), 0);
            return array;
        }

        private Array ReadArraySequenceMultiDim<T>(
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

            var indices = new long[array.Rank];
            var cumulativeLengths = array.GetCumulativeLengths();

            for (var i = 0; i < node.Sequence.Count; i++)
            {
                var subNode = node.Sequence[i];
                var result = Read<T>(subNode, context, skipHook);

                for (int dim = array.Rank - 1; dim >= 0; dim--)
                {
                    indices[dim] = i / cumulativeLengths[dim] % lengths[dim];
                }

                array.SetValue(result!, indices);
            }

            return array;
        }

        private Array ReadArraySequenceSealedMultiDim<T>(
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

            var indices = new long[array.Rank];
            var cumulativeLengths = array.GetCumulativeLengths();

            for (var i = 0; i < node.Sequence.Count; i++)
            {
                var subNode = node.Sequence[i];
                var result = elementReader(type, subNode, context, skipHook);
                ReadNullCheck(type, result);

                for (int dim = array.Rank - 1; dim >= 0; dim--)
                {
                    indices[dim] = i / cumulativeLengths[dim] % lengths[dim];
                }

                array.SetValue((T)result!, indices);
            }

            return array;
        }

        private TEnum ReadEnumValue<TEnum>(ValueDataNode node) where TEnum : struct
        {
            return Enum.Parse<TEnum>(node.Value, true);
        }

        private TEnum ReadEnumSequence<TEnum>(SequenceDataNode node) where TEnum : struct
        {
            return Enum.Parse<TEnum>(string.Join(", ", node.Sequence), true);
        }

        private TDelegate ReadDelegateValue<TDelegate>(ValueDataNode node)
            where TDelegate : Delegate
        {
            var split = node.Value.Split(':');
            if (split.Length != 2)
            {
                throw new ArgumentException($"Could not parse delegate name from string '{split}', it should have the value 'Namespace.Of.Type:MethodName'");
            }

            var systemTypeName = split[0];
            var methodName = split[1];

            return ReadDelegateValue<TDelegate>(systemTypeName, methodName);
        }

        private TDelegate ReadDelegateMapping<TDelegate>(MappingDataNode node)
            where TDelegate : Delegate
        {
            var systemTypeName = node.Cast<ValueDataNode>("system").Value;
            var methodName = node.Cast<ValueDataNode>("method").Value;

            return ReadDelegateValue<TDelegate>(systemTypeName, methodName);
        }

        private TDelegate ReadDelegateValue<TDelegate>(string systemTypeName, string methodName)
            where TDelegate : Delegate
        {
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

            return Expression.Lambda<TDelegate>(call, parameters).Compile();
        }

        private TValue? ReadSelfSerialize<TValue>(
            ValueDataNode node,
            InstantiationDelegate<object> instantiator,
            object? rawValue = null)
            where TValue : ISelfSerialize
        {
            if (node.Value == "null")
            {
                return default; //todo paul this default should be null
            }

            var value = (TValue)(rawValue ?? instantiator());
            value.Deserialize(node.Value);

            return value;
        }

        private TValue ReadWithTypeReader<TValue, TNode>(
            TNode node,
            ITypeReader<TValue, TNode> reader,
            ISerializationContext? context = null,
            bool skipHook = false,
            object? value = null)
            where TNode : DataNode
        {
            if (context != null &&
                context.TypeReaders.TryGetValue((typeof(TValue), typeof(TNode)), out var readerUnCast))
            {
                reader = (ITypeReader<TValue, TNode>)readerUnCast;
            }

            return reader.Read(this, node, DependencyCollection, skipHook, context, value == null ? default : (TValue)value);
        }

        private TValue ReadGenericValue<TValue>(
            ValueDataNode node,
            InstantiationDelegate<object> instantiator,
            DataDefinition? definition,
            bool populate,
            bool hooks,
            ISerializationContext? context = null,
            bool skipHook = false,
            object? instance = null)
        {
            var type = typeof(TValue);

            if (context != null &&
                context.TypeReaders.TryGetValue((typeof(TValue), typeof(ValueDataNode)), out var readerUnCast))
            {
                var reader = (ITypeReader<TValue, ValueDataNode>)readerUnCast;
                return reader.Read(this, node, DependencyCollection, skipHook, context, instance == null ? default : (TValue)instance);
            }

            if (definition == null)
            {
                throw new ArgumentException($"No data definition found for type {type} with node type {node.GetType()} when reading");
            }

            instance ??= instantiator();

            if (populate)
            {
                ((IPopulateDefaultValues)instance).PopulateDefaultValues();
            }

            if (node.Value != string.Empty)
            {
                throw new ArgumentException($"No mapping node provided for type {type} at line: {node.Start.Line}");
            }

            if (!skipHook && hooks)
            {
                ((ISerializationHooks)instance).AfterDeserialization();
            }

            return (TValue)instance;
        }

        private TValue ReadGenericMapping<TValue>(
            MappingDataNode node,
            InstantiationDelegate<object> instantiator,
            DataDefinition? definition,
            bool populate,
            bool hooks,
            ISerializationContext? context = null,
            bool skipHook = false,
            object? instance = null)
        {
            var type = typeof(TValue);
            instance ??= instantiator();

            if (context != null &&
                context.TypeReaders.TryGetValue((type, typeof(MappingDataNode)), out var readerUnCast))
            {
                var reader = (ITypeReader<TValue, MappingDataNode>)readerUnCast;
                return reader.Read(this, node, DependencyCollection, skipHook, context, (TValue?)instance);
            }

            if (definition == null)
            {
                throw new ArgumentException($"No data definition found for type {type} with node type {node.GetType()} when reading");
            }

            if (populate)
            {
                ((IPopulateDefaultValues)instance).PopulateDefaultValues();
            }

            var result = (TValue)definition.Populate(instance, node, this, context, skipHook)!;

            if (!skipHook && hooks)
            {
                ((ISerializationHooks)result).AfterDeserialization();
            }

            return result;
        }

        public object? ReadWithTypeSerializer(Type type, Type serializer, DataNode node, ISerializationContext? context = null,
            bool skipHook = false, object? value = null)
        {
            return ReadWithSerializerRaw(type, node, serializer, context, skipHook, value);
        }
    }
}
