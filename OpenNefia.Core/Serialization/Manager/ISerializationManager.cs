using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using OpenNefia.Core.Serialization.Manager.Definition;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.Markdown.Validation;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;

namespace OpenNefia.Core.Serialization.Manager
{
    public interface ISerializationManager
    {
        /// <summary>
        ///     Initializes the serialization manager.
        /// </summary>
        void Initialize();

        /// <summary>
        ///     Shuts down the serialization manager.
        /// </summary>
        void Shutdown();

        #region Data Definitions

        /// <summary>
        ///     Checks if a type has a data definition defined for it.
        /// </summary>
        /// <param name="type">The type to check for.</param>
        /// <returns>True if it does, false otherwise.</returns>
        bool HasDataDefinition(Type type);

        /// <summary>
        /// Checks if a type can be serialized.
        /// </summary>
        /// <param name="type">The type to check for.</param>
        /// <returns>True if it can be, false otherwise.</returns>
        bool CanSerializeType(Type type);

        #endregion

        #region Validation

        /// <summary>
        ///     Validates that a node has all the properties required by a certain type with its serializer.
        /// </summary>
        /// <param name="type">The type to check for.</param>
        /// <param name="node">The node to check.</param>
        /// <param name="context">The context to use, if any.</param>
        /// <returns>
        ///     A node with whether or not <see cref="node"/> is valid and which of its fields
        ///     are invalid, if any.
        /// </returns>
        ValidationNode ValidateNode(Type type, DataNode node, ISerializationContext? context = null);

        /// <summary>
        ///     Validates that a node has all the properties required by a certain type with its serializer.
        /// </summary>
        /// <param name="node">The node to check.</param>
        /// <param name="context">The context to use, if any.</param>
        /// <returns>
        ///     A node with whether or not <see cref="node"/> is valid and which of its fields
        ///     are invalid, if any.
        /// </returns>
        ValidationNode ValidateNode<T>(DataNode node, ISerializationContext? context = null);

        ValidationNode ValidateNodeWith(Type type, Type typeSerializer, DataNode node, ISerializationContext? context = null);

        ValidationNode ValidateNodeWith<TType, TSerializer, TNode>(TNode node, ISerializationContext? context = null)
            where TSerializer : ITypeValidator<TType, TNode>
            where TNode : DataNode;

        #endregion

        #region Read
        
        /// <summary>
        ///     Deserializes a node into an object, populating it.
        /// </summary>
        /// <param name="type">The type of object to deserialize into.</param>
        /// <param name="node">The node to deserialize.</param>
        /// <param name="context">The context to use, if any.</param>
        /// <param name="skipHook">Whether or not to skip running <see cref="ISerializationHooks"/></param>
        /// <param name="value">The value to read into. If none is supplied, a new object will be created.</param>
        /// <returns>The deserialized object or null.</returns>
        public object? Read(Type type, DataNode node, ISerializationContext? context = null, bool skipHook = false,
            object? value = null);

        /// <summary>
        ///     Deserializes a node into a populated object of the given generic type <see cref="T"/>
        /// </summary>
        /// <param name="node">The node to deserialize.</param>
        /// <param name="context">The context to use, if any.</param>
        /// <param name="skipHook">Whether or not to skip running <see cref="ISerializationHooks"/></param>
        /// <param name="value">The value to read into. If none is supplied, a new object will be created.</param>
        /// <typeparam name="T">The type of object to create and populate.</typeparam>
        /// <returns>The deserialized object, or null.</returns>
        T Read<T>(DataNode node, ISerializationContext? context = null, bool skipHook = false, T? value = default);

        object? ReadWithTypeSerializer(Type type, Type serializer, DataNode node,
            ISerializationContext? context = null, bool skipHook = false, object? value = null);

        #endregion

        #region Write

        /// <summary>
        ///     Serializes a value into a node.
        /// </summary>
        /// <param name="value">The value to serialize.</param>
        /// <param name="alwaysWrite">
        ///     Whether or not to always write the given values into the resulting node,
        ///     even if they are the default.
        /// </param>
        /// <param name="context">The context to use, if any.</param>
        /// <typeparam name="T">The type to serialize.</typeparam>
        /// <returns>A serialized datanode created from the given <see cref="value"/>.</returns>
        DataNode WriteValue<T>(T value, bool alwaysWrite = false, ISerializationContext? context = null);

        /// <summary>
        ///     Serializes a value into a node.
        /// </summary>
        /// <param name="type">The type of the <see cref="value"/> to serialize as.</param>
        /// <param name="value">The value to serialize.</param>
        /// <param name="alwaysWrite">
        ///     Whether or not to always write the given values into the resulting node,
        ///     even if they are the default.
        /// </param>
        /// <param name="context">The context to use, if any.</param>
        /// <returns>
        ///     A serialized datanode created from the given <see cref="value"/>
        ///     of type <see cref="type"/>.
        /// </returns>
        DataNode WriteValue(Type type, object? value, bool alwaysWrite = false, ISerializationContext? context = null);

        DataNode WriteWithTypeSerializer(Type type, Type serializer, object? value, bool alwaysWrite = false,
            ISerializationContext? context = null);

        #endregion

        #region Copy

        /// <summary>
        ///     Copies the values of one object into another.
        ///     This does not guarantee that the object passed as <see cref="target"/>
        ///     is actually mutated.
        /// </summary>
        /// <param name="source">The object to copy values from.</param>
        /// <param name="target">The object to copy values into.</param>
        /// <param name="context">The context to use, if any.</param>
        /// <param name="skipHook">Whether or not to skip running <see cref="ISerializationHooks"/></param>
        void Copy(object? source, ref object? target, ISerializationContext? context = null, bool skipHook = false);

        /// <summary>
        ///     Copies the values of one object into another.
        ///     This does not guarantee that the object passed as <see cref="target"/>
        ///     is actually mutated.
        /// </summary>
        /// <param name="source">The object to copy values from.</param>
        /// <param name="target">The object to copy values into.</param>
        /// <param name="context">The context to use, if any.</param>
        /// <param name="skipHook">Whether or not to skip running <see cref="ISerializationHooks"/></param>
        /// <typeparam name="T">The type of the objects to copy from and into.</typeparam>
        void Copy<T>(T source, ref T target, ISerializationContext? context = null, bool skipHook = false);

        /// <summary>
        ///     Creates a copy of the given object.
        /// </summary>
        /// <param name="source">The object to copy.</param>
        /// <param name="context">The context to use, if any.</param>
        /// <param name="skipHook">Whether or not to skip running <see cref="ISerializationHooks"/></param>
        /// <returns>A copy of the given object.</returns>
        [MustUseReturnValue]
        object? Copy(object? source, ISerializationContext? context = null, bool skipHook = false);

        /// <summary>
        ///     Creates a copy of the given object.
        /// </summary>
        /// <param name="source">The object to copy.</param>
        /// <param name="context">The context to use, if any.</param>
        /// <param name="skipHook">Whether or not to skip running <see cref="ISerializationHooks"/></param>
        /// <typeparam name="T">The type of the object to copy.</typeparam>
        /// <returns>A copy of the given object.</returns>
        [MustUseReturnValue]
        T Copy<T>(T source, ISerializationContext? context = null, bool skipHook = false);

        [MustUseReturnValue]
        object? CopyWithTypeSerializer(Type typeSerializer, object? source, object? target,
            ISerializationContext? context = null, bool skipHook = false);

        #endregion

        #region Equality Comparison

        /// <summary>
        ///     Compares two objects for equality.
        /// </summary>
        /// <param name="left">The first object to compare.</param>
        /// <param name="right">The second object to compare.</param>
        /// <param name="context">The context to use, if any.</param>
        /// <param name="skipHook">Whether or not to skip running <see cref="ISerializationHooks"/></param>
        /// <returns>
        ///     True if all non-excluded serializable fields on both objects are equivalent.
        /// </returns>
        bool Compare(object? left, object? right, ISerializationContext? context = null, bool skipHook = false);

        #endregion

        #region Flags And Constants

        Type GetFlagTypeFromTag(Type tagType);

        int GetFlagHighestBit(Type tagType);

        Type GetConstantTypeFromTag(Type tagType);

        #endregion

        #region Composition

        DataNode PushComposition(Type type, DataNode[] parents, DataNode child, ISerializationContext? context = null);

        public TNode PushComposition<TType, TNode>(TNode[] parents, TNode child, ISerializationContext? context = null) where TNode : DataNode
        {
            // ReSharper disable once CoVariantArrayConversion
            return (TNode)PushComposition(typeof(TType), parents, child, context);
        }

        public TNode PushCompositionWithGenericNode<TNode>(Type type, TNode[] parents, TNode child, ISerializationContext? context = null) where TNode : DataNode
        {
            // ReSharper disable once CoVariantArrayConversion
            return (TNode)PushComposition(type, parents, child, context);
        }

        #endregion
    }

    internal interface ISerializationManagerInternal : ISerializationManager
    {
        bool IsValidatingOnly { get; set; }
    }
}
