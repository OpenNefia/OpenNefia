// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Analyzer.Utilities.Extensions;
using Microsoft.CodeAnalysis;

namespace OpenNefia.EditorExtension
{
    internal static class INamedTypeSymbolExtensions
    {
        public static IEnumerable<INamedTypeSymbol> GetBaseTypesAndThis(this INamedTypeSymbol type)
        {
            INamedTypeSymbol current = type;
            while (current != null)
            {
                yield return current;
                current = current.BaseType;
            }
        }

        /// <summary>
        /// Returns a value indicating whether <paramref name="type"/> derives from, or implements
        /// any generic construction of, the type defined by <paramref name="parentType"/>.
        /// </summary>
        /// <remarks>
        /// This method only works when <paramref name="parentType"/> is a definition,
        /// not a constructed type.
        /// </remarks>
        /// <example>
        /// <para>
        /// If <paramref name="parentType"/> is the class <see cref="Stack{T}"/>, then this
        /// method will return <see langword="true"/> when called on <c>Stack&gt;int></c>
        /// or any type derived it, because <c>Stack&gt;int></c> is constructed from
        /// <see cref="Stack{T}"/>.
        /// </para>
        /// <para>
        /// Similarly, if <paramref name="parentType"/> is the interface <see cref="IList{T}"/>,
        /// then this method will return <see langword="true"/> for <c>List&gt;int></c>
        /// or any other class that extends <see cref="IList{T}"/> or an class that implements it,
        /// because <c>IList&gt;int></c> is constructed from <see cref="IList{T}"/>.
        /// </para>
        /// </example>
        public static bool DerivesFromOrImplementsAnyConstructionOf(this INamedTypeSymbol type, INamedTypeSymbol parentType)
        {
            if (!parentType.IsDefinition)
            {
                throw new ArgumentException($"The type {nameof(parentType)} is not a definition; it is a constructed type", nameof(parentType));
            }

            for (INamedTypeSymbol baseType = type.OriginalDefinition;
                baseType != null;
                baseType = baseType.BaseType?.OriginalDefinition)
            {
                if (baseType.Equals(parentType))
                {
                    return true;
                }
            }

            if (type.OriginalDefinition.AllInterfaces.Any(baseInterface => baseInterface.OriginalDefinition.Equals(parentType)))
            {
                return true;
            }

            return false;
        }
    }
}
