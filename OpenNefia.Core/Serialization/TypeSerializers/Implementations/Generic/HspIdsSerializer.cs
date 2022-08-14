using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Manager.Result;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;

namespace OpenNefia.Core.Serialization.TypeSerializers.Implementations
{
    [TypeSerializer]
    public class HspIdsSerializer<T> : ITypeSerializer<HspIds<T>, MappingDataNode> where T : struct
    {
        public DeserializationResult Read(ISerializationManager serializationManager,
            MappingDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null)
        {
            var mappedFields = new Dictionary<DeserializationResult, DeserializationResult>();

            string? origin = null;

            foreach (var (key, val) in node.Children)
            {
                if (val.Tag == "!*" || node.Children.Count == 1)
                {
                    var (keyVal, keyResult) = serializationManager.ReadWithValueOrThrow<string>(key, context, skipHook);
                    origin = keyVal!;
                }
            }

            var hspIds = new HspIds<T>(origin!);

            foreach (var (key, val) in node.Children)
            {
                var (keyVal, keyResult) = serializationManager.ReadWithValueOrThrow<string>(key, context, skipHook);
                var (valueVal, valueResult) = serializationManager.ReadWithValueOrThrow<T>(val, skipHook: skipHook);

                mappedFields.Add(keyResult, valueResult);
                hspIds.Add(keyVal, valueVal);
            }

            return new DeserializedHspIds<T>(hspIds, mappedFields);
        }

        public ValidationNode Validate(ISerializationManager serializationManager,
            MappingDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            var mapping = new Dictionary<ValidationNode, ValidationNode>();

            string? origin = null;

            foreach (var (key, val) in node.Children)
            {
                var keyResult = serializationManager.ValidateNode(typeof(string), key, context);

                ValidationNode valueResult;

                if (val is ValueDataNode valNode)
                {
                    if (val.Tag == "!*" || node.Children.Count == 1)
                    {
                        if (origin != null)
                        {
                            valueResult = new ErrorNode(val, "Cannot have more than one origin.");
                            continue;
                        }

                        origin = valNode.Value;
                    }
                    valueResult = serializationManager.ValidateNode(typeof(T), val, context);
                }
                else
                {
                    valueResult = new ErrorNode(val, $"hspIds entry must be an integer.");
                }


                mapping.Add(keyResult, valueResult);
            }

            return origin != null ?
                new ValidatedMappingNode(mapping)
                : new ErrorNode(node, "No node in hspIds list tagged as origin ('!*')");
        }

        public DataNode Write(ISerializationManager serializationManager,
            HspIds<T> value, bool alwaysWrite = false, ISerializationContext? context = null)
        {
            var compSequence = new MappingDataNode();

            foreach (var (variantId, hspId) in value)
            {
                var keyNode = serializationManager.WriteValue(typeof(string), variantId, alwaysWrite, context);
                var valueNode = serializationManager.WriteValue(typeof(T), hspId, alwaysWrite, context);
                compSequence.Add(keyNode, valueNode);
            }

            return compSequence;
        }

        [MustUseReturnValue]
        public HspIds<T> Copy(ISerializationManager serializationManager,
            HspIds<T> source, HspIds<T> target, bool skipHook, ISerializationContext? context = null)
        {
            target.Clear();
            target.EnsureCapacity(source.Count);

            foreach (var (variantId, hspId) in source)
            {
                target.Add(variantId, serializationManager.CreateCopy(hspId, context)!);
            }

            return target;
        }

        public bool Compare(ISerializationManager serializationManager, HspIds<T> left, HspIds<T> right, bool skipHook,
            ISerializationContext? context = null)
        {
            if (left.Count != right.Count)
                return false;

            foreach (var (id, compLeft) in left)
            {
                if (!right.TryGetValue(id, out var compRight))
                    return false;

                if (!serializationManager.Compare(compLeft, compRight))
                    return false;
            }

            return true;
        }
    }
}
