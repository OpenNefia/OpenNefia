using OpenNefia.Core.Serialization.Manager.Attributes;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.Core.Prototypes
{
    /// <summary>
    /// Indicates that this prototype or component can map to a pseudo-datatype in
    /// one or more HSP variants of Elona.
    /// </summary>
    public interface IHspIds<T> where T : struct
    {
        /// <summary>
        /// Identifier of the HSP variant of Elona this prototype first appeared in, if any.
        /// </summary>
        /// <remarks>
        /// There must be an entry in <see cref="HspIds"/> for this variant.
        /// </remarks>
        string? HspOrigin { get; }

        /// <summary>
        /// Dictionary mapping from variant identifier to the integer ID of the prototype/component
        /// in that variant's source code.
        /// </summary>
        IReadOnlyDictionary<string, T> HspIds { get; }
    }

    public static class IHspIdsExt
    {
        public static T? GetCanonicalHspId<T>(this IHspIds<T> ids) where T: struct
        {
            if (ids.HspOrigin == null)
                return null;

            if (!ids.HspIds.TryGetValue(ids.HspOrigin, out var hspId))
                throw new InvalidOperationException($"This data definition originates from variant '{ids.HspOrigin}', but there is no ID for that variant in hspIds.");

            return hspId;
        }
    }

    /// <summary>
    /// Identifiers for common variants of Elona as used by OpenNefia.
    /// </summary>
    public static class ElonaVariants
    {
        public const string Elona122 = "elona122";
    }
}