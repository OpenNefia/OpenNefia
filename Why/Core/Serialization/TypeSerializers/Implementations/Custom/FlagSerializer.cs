﻿using System;
using Why.Core.IoC;
using Why.Core.Serialization.Manager;
using Why.Core.Serialization.Manager.Result;
using Why.Core.Serialization.Markdown;
using Why.Core.Serialization.Markdown.Sequence;
using Why.Core.Serialization.Markdown.Validation;
using Why.Core.Serialization.Markdown.Value;
using Why.Core.Serialization.TypeSerializers.Interfaces;

namespace Why.Core.Serialization.TypeSerializers.Implementations.Custom
{
    public class FlagSerializer<TTag> : ITypeSerializer<int, ValueDataNode>, ITypeReader<int, SequenceDataNode>
    {
        public ValidationNode Validate(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, ISerializationContext? context = null)
        {
            var flagType = serializationManager.GetFlagTypeFromTag(typeof(TTag));
            return Enum.TryParse(flagType, node.Value, out _) ? new ValidatedValueNode(node) : new ErrorNode(node, "Failed parsing flag.", false);
        }

        public DeserializationResult Read(ISerializationManager serializationManager, ValueDataNode node,
            IDependencyCollection dependencies, bool skipHook, ISerializationContext? context = null)
        {
            var flagType = serializationManager.GetFlagTypeFromTag(typeof(TTag));
            return new DeserializedValue((int)Enum.Parse(flagType, node.Value));
        }

        public DataNode Write(ISerializationManager serializationManager, int value, bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            var sequenceNode = new SequenceDataNode();
            var flagType = serializationManager.GetFlagTypeFromTag(typeof(TTag));

            // Assumption: a bitflag enum has a constructor for every bit value such that
            // that bit is set in some other constructor i.e. if a 1 appears somewhere in
            // the bits of one of the enum constructors, there is an enum constructor which
            // is 1 just in that position.
            //
            // Otherwise, this code may throw an exception
            var maxFlagValue = serializationManager.GetFlagHighestBit(typeof(TTag));

            for (var bitIndex = 1; bitIndex <= maxFlagValue; bitIndex++)
            {
                var bitValue = 1 << bitIndex;

                if ((bitValue & value) == bitValue)
                {
                    var flagName = Enum.GetName(flagType, bitValue);

                    if (flagName == null)
                    {
                        throw new InvalidOperationException($"No bitflag corresponding to bit {bitIndex} in {flagType}, but it was set anyways.");
                    }

                    sequenceNode.Add(new ValueDataNode(flagName));
                }
            }

            return sequenceNode;
        }

        public int Copy(ISerializationManager serializationManager, int source, int target, bool skipHook,
            ISerializationContext? context = null)
        {
            return source;
        }

        public ValidationNode Validate(ISerializationManager serializationManager, SequenceDataNode node,
            IDependencyCollection dependencies, ISerializationContext? context = null)
        {
            var flagType = serializationManager.GetFlagTypeFromTag(typeof(TTag));

            foreach (var elem in node.Sequence)
            {
                if (elem is not ValueDataNode valueDataNode) return new ErrorNode(node, "Invalid flagtype in flag-sequence.", true);
                if (!Enum.TryParse(flagType, valueDataNode.Value, out _)) return new ErrorNode(node, "Failed parsing flag in flag-sequence", false);
            }

            return new ValidatedValueNode(node);
        }

        public DeserializationResult Read(ISerializationManager serializationManager, SequenceDataNode node,
            IDependencyCollection dependencies, bool skipHook, ISerializationContext? context = null)
        {
            var flagType = serializationManager.GetFlagTypeFromTag(typeof(TTag));
            var flags = 0;

            foreach (var elem in node.Sequence)
            {
                if (elem is not ValueDataNode valueDataNode) throw new InvalidNodeTypeException();
                flags |= (int) Enum.Parse(flagType, valueDataNode.Value);
            }

            return new DeserializedValue(flags);
        }
    }
}
