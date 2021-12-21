﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Why.Core.IoC;
using Why.Core.Serialization.Markdown;
using Why.Core.Serialization.Markdown.Validation;
using Why.Core.Serialization.TypeSerializers.Interfaces;

namespace Why.Core.Serialization.Manager
{
    public partial class SerializationManager
    {
        private readonly Dictionary<(Type Type, Type DataNodeType), object> _typeValidators = new();

        private bool TryValidateWithTypeValidator(
            Type type,
            DataNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context,
            [NotNullWhen(true)] out ValidationNode? valid)
        {
            // TODO Serialization: do this shit w/ delegates
            var method = typeof(SerializationManager).GetRuntimeMethods().First(m =>
                m.Name == nameof(TryValidateWithTypeValidator) && m.GetParameters().Length == 4).MakeGenericMethod(type, node.GetType());

            var arr = new object?[] {node, dependencies, context, null};
            var res = method.Invoke(this, arr);

            if (res as bool? ?? false)
            {
                valid = (ValidationNode) arr[3]!;
                return true;
            }

            valid = null;
            return false;
        }

        private bool TryValidateWithTypeValidator<T, TNode>(
            TNode node,
            IDependencyCollection dependencies,
            ISerializationContext? context,
            [NotNullWhen(true)] out ValidationNode? valid)
            where TNode : DataNode
        {
            if (TryGetValidator<T, TNode>(null, out var reader))
            {
                valid = reader.Validate(this, node, dependencies, context);
                return true;
            }

            valid = null;
            return false;
        }

        private bool TryGetValidator<T, TNode>(
            ISerializationContext? context,
            [NotNullWhen(true)] out ITypeValidator<T, TNode>? reader)
            where TNode : DataNode
        {
            if (context != null && context.TypeValidators.TryGetValue((typeof(T), typeof(TNode)), out var rawTypeValidator) ||
                _typeValidators.TryGetValue((typeof(T), typeof(TNode)), out rawTypeValidator))
            {
                reader = (ITypeReader<T, TNode>) rawTypeValidator;
                return true;
            }

            return TryGetGenericValidator(out reader);
        }

        private bool TryGetGenericValidator<T, TNode>([NotNullWhen(true)] out ITypeValidator<T, TNode>? rawReader)
            where TNode : DataNode
        {
            rawReader = null;

            if (typeof(T).IsGenericType)
            {
                var typeDef = typeof(T).GetGenericTypeDefinition();

                Type? serializerTypeDef = null;

                foreach (var (key, val) in _genericValidatorTypes)
                {
                    if (typeDef.HasSameMetadataDefinitionAs(key.Type) && key.DataNodeType.IsAssignableFrom(typeof(TNode)))
                    {
                        serializerTypeDef = val;
                        break;
                    }
                }

                if (serializerTypeDef == null) return false;

                var serializerType = serializerTypeDef.MakeGenericType(typeof(T).GetGenericArguments());
                rawReader = (ITypeValidator<T, TNode>) RegisterSerializer(serializerType)!;

                return true;
            }

            return false;
        }
    }
}
