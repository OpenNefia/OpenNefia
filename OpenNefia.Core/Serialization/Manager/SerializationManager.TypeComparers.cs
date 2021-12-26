using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using OpenNefia.Core.Serialization.Markdown;
using OpenNefia.Core.Serialization.TypeSerializers.Interfaces;

namespace OpenNefia.Core.Serialization.Manager
{
    public partial class SerializationManager
    {
        private delegate bool CompareDelegate(
            object objA,
            object objB,
            out bool areSame,
            bool skipHook,
            ISerializationContext? context);

        private readonly Dictionary<Type, object> _typeComparers = new();
        private readonly ConcurrentDictionary<Type, CompareDelegate> _comparerDelegates = new();

        private CompareDelegate GetOrCreateCompareDelegate(Type type)
        {
            return _comparerDelegates
                .GetOrAdd(type, (_, t) =>
                {
                    var instanceParam = Expression.Constant(this);
                    var objAParam = Expression.Parameter(typeof(object), "objA");
                    var objBParam = Expression.Parameter(typeof(object), "objB");
                    var areSameParam = Expression.Parameter(typeof(DataNode).MakeByRefType(), "areSame");
                    var skipHook = Expression.Parameter(typeof(bool), "skipHook");
                    var contextParam = Expression.Parameter(typeof(ISerializationContext), "context");

                    var call = Expression.Call(
                        instanceParam,
                        nameof(TryCompare),
                        new[] { t },
                        Expression.Convert(objAParam, t),
                        Expression.Convert(objBParam, t),
                        areSameParam,
                        skipHook,
                        contextParam);

                    return Expression.Lambda<CompareDelegate>(
                        call,
                        objAParam,
                        objBParam,
                        areSameParam,
                        skipHook,
                        contextParam).Compile();
                }, type);
        }

        private bool TryCompareRaw(
            Type type,
            object objA,
            object objB,
            out bool areSame,
            bool skipHook = false,
            ISerializationContext? context = null)
        {
            return GetOrCreateCompareDelegate(type)(objA, objB, out areSame, skipHook, context);
        }

        private bool TryGetComparer<T>(
            ISerializationContext? context,
            [NotNullWhen(true)] out ITypeComparer<T>? comparer)
        {
            if (context != null && context.TypeComparers.TryGetValue(typeof(T), out var rawTypeComparer) ||
                _typeComparers.TryGetValue(typeof(T), out rawTypeComparer))
            {
                comparer = (ITypeComparer<T>)rawTypeComparer;
                return true;
            }

            return TryGetGenericComparer(out comparer);
        }

        private bool TryCompare<T>(
            T objA,
            T objB,
            out bool areSame,
            bool skipHook = false,
            ISerializationContext? context = null)
        {
            areSame = false;
            if (TryGetComparer<T>(context, out var comparer))
            {
                areSame = comparer.Compare(this, objA, objB, skipHook, context);
                return true;
            }

            return false;
        }

        private bool TryGetGenericComparer<T>([NotNullWhen(true)] out ITypeComparer<T>? rawComparer)
        {
            rawComparer = null;

            if (typeof(T).IsGenericType)
            {
                var typeDef = typeof(T).GetGenericTypeDefinition();

                Type? serializerTypeDef = null;

                foreach (var (key, val) in _genericComparerTypes)
                {
                    if (typeDef.HasSameMetadataDefinitionAs(key))
                    {
                        serializerTypeDef = val;
                        break;
                    }
                }

                if (serializerTypeDef == null) return false;

                var serializerType = serializerTypeDef.MakeGenericType(typeof(T).GetGenericArguments());
                rawComparer = (ITypeComparer<T>)RegisterSerializer(serializerType)!;

                return true;
            }

            return false;
        }
    }
}
