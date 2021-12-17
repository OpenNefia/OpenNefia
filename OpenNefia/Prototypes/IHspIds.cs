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
        /// Dictionary mapping from variant identifier to the integer ID of the prototype/component
        /// in that variant's source code.
        /// </summary>
        HspIds<T>? HspIds { get; }
    }

    public class HspIds<T> : Dictionary<string, T> where T : struct
    {
        /// <summary>
        /// Identifier of the HSP variant of Elona this prototype first appeared in, if any.
        /// </summary>
        /// <remarks>
        /// There must be an entry in <see cref="HspIds"/> for this variant.
        /// </remarks>
        public string HspOrigin { get; }

        public HspIds(string hspOrigin)
        {
            HspOrigin = hspOrigin;
        }

        public T GetCanonical()
        {
            return this[HspOrigin];
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