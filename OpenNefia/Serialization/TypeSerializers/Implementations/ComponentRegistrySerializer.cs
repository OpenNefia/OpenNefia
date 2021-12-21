using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Serialization.Manager;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Manager.Result;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Sequence;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.Markdown.Value;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;
using static OpenNefia.Core.Prototypes.EntityPrototype;

namespace OpenNefia.Core.Serialization.TypeSerializers.Implementations
{
    [TypeSerializer]
    public class ComponentRegistrySerializer : ITypeSerializer<ComponentRegistry, SequenceDataNode>
    {
        public DeserializationResult Read(ISerializationManager serializationManager,
            SequenceDataNode node,
            IDependencyCollection dependencies,
            bool skipHook,
            ISerializationContext? context = null)
        {
            var factory = dependencies.Resolve<IComponentFactory>();
            var components = new ComponentRegistry();
            var mappings = new Dictionary<DeserializationResult, DeserializationResult>();

            foreach (var componentMapping in node.Sequence.Cast<MappingDataNode>())
            {
                string compType = ((ValueDataNode) componentMapping.Get("type")).Value;
                // See if type exists to detect errors.
                if (!factory.IsRegistered(compType))
                {
                        Logger.ErrorS(SerializationManager.LogCategory, $"Unknown component '{compType}' in prototype!");
                        continue;
                }

                // Has this type already been added?
                if (components.Keys.Contains(compType))
                {
                    Logger.ErrorS(SerializationManager.LogCategory, $"Component of type '{compType}' defined twice in prototype!");
                    continue;
                }

                var copy = componentMapping.Copy()!;
                copy.Remove("type");

                var type = factory.GetRegistration(compType).Type;
                var read = serializationManager.ReadWithValueOrThrow<IComponent>(type, copy, skipHook: skipHook);

                components[compType] = read.value;
                mappings.Add(new DeserializedValue<string>(compType), read.result);
            }

            var referenceTypes = new List<Type>();
            // Assert that there are no conflicting component references.
            foreach (var componentName in components.Keys)
            {
                var registration = factory.GetRegistration(componentName);
                foreach (var compType in registration.References)
                {
                    if (referenceTypes.Contains(compType))
                    {
                        throw new InvalidOperationException(
                            $"Duplicate component reference in prototype: '{compType}'");
                    }

                    referenceTypes.Add(compType);
                }
            }

            return new DeserializedComponentRegistry(components, mappings);
        }

        public ValidationNode Validate(ISerializationManager serializationManager,
            SequenceDataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context = null)
        {
            var factory = dependencies.Resolve<IComponentFactory>();
            var components = new ComponentRegistry();
            var list = new List<ValidationNode>();

            foreach (var componentMapping in node.Sequence.Cast<MappingDataNode>())
            {
                string compType = ((ValueDataNode) componentMapping.Get("type")).Value;
                // See if type exists to detect errors.
                if (!factory.IsRegistered(compType))
                {
                    list.Add(new ErrorNode(componentMapping, $"Unknown component type {compType}."));
                    continue;
                }

                // Has this type already been added?
                if (components.Keys.Contains(compType))
                {
                    list.Add(new ErrorNode(componentMapping, "Duplicate Component."));
                    continue;
                }

                var copy = componentMapping.Copy()!;
                copy.Remove("type");

                var type = factory.GetRegistration(compType).Type;

                list.Add(serializationManager.ValidateNode(type, copy, context));
            }

            var referenceTypes = new List<Type>();

            // Assert that there are no conflicting component references.
            foreach (var componentName in components.Keys)
            {
                var registration = factory.GetRegistration(componentName);

                foreach (var compType in registration.References)
                {
                    if (referenceTypes.Contains(compType))
                    {
                        return new ErrorNode(node, "Duplicate ComponentReference.");
                    }

                    referenceTypes.Add(compType);
                }
            }

            return new ValidatedSequenceNode(list);
        }

        public DataNode Write(ISerializationManager serializationManager, ComponentRegistry value,
            bool alwaysWrite = false,
            ISerializationContext? context = null)
        {
            var compSequence = new SequenceDataNode();
            foreach (var (type, component) in value)
            {
                var node = serializationManager.WriteValue(component.GetType(), component, alwaysWrite, context);
                if (node is not MappingDataNode mapping) throw new InvalidNodeTypeException();

                mapping.Add("type", new ValueDataNode(type));
                compSequence.Add(mapping);
            }

            return compSequence;
        }

        [MustUseReturnValue]
        public ComponentRegistry Copy(ISerializationManager serializationManager, ComponentRegistry source,
            ComponentRegistry target, bool skipHook, ISerializationContext? context = null)
        {
            target.Clear();
            target.EnsureCapacity(source.Count);

            foreach (var (id, component) in source)
            {
                target.Add(id, serializationManager.CreateCopy(component, context)!);
            }

            return target;
        }
    }
}
