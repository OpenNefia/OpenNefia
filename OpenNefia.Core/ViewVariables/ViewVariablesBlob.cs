using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Core.ViewVariables
{
    internal abstract class ViewVariablesBlob
    {
    }

    internal sealed class ViewVariablesBlobMetadata : ViewVariablesBlob
    {
        /// <summary>
        ///     A list of traits for this object.
        ///     A trait is basically saying "this object can be viewed in a specific way".
        ///     There's a trait for "this object has members that are directly accessible to VV (the usual),
        ///     A trait for objects that are <see cref="IEnumerable"/>, etc...
        /// </summary>
        /// <seealso cref="ViewVariablesManager.TraitIdsFor" />
        public List<ViewVariablesTrait> Traits { get; set; } = new();

        /// <summary>
        ///     The pretty type name of the object.
        /// </summary>
        public string ObjectTypePretty { get; set; } = string.Empty;

        /// <summary>
        ///     The assembly qualified type name of the object.
        /// </summary>
        public string ObjectType { get; set; } = string.Empty;

        /// <summary>
        ///     The <see cref="Object.ToString"/> output of the object.
        /// </summary>
        public string Stringified { get; set; } = string.Empty;
    }

    /// <summary>
    ///     Contains the VV-accessible members (fields or properties) of a remote object.
    ///     Requested by <see cref="ViewVariablesRequestMembers"/>.
    /// </summary>
    internal sealed class ViewVariablesBlobMembers : ViewVariablesBlob
    {
        /// <summary>
        ///     A list of VV-accessible the remote object has.
        /// </summary>
        public List<(string groupName, List<MemberData> groupMembers)> MemberGroups { get; set; }
            = new();

        /// <summary>
        ///     Data for a specific property.
        /// </summary>
        public sealed class MemberData
        {
            /// <summary>
            ///     Whether the property can be edited by this client.
            /// </summary>
            public bool Editable { get; set; }

            /// <summary>
            ///     Assembly qualified type name of the property.
            /// </summary>
            public string Type { get; set; } = string.Empty;

            /// <summary>
            ///     Pretty type name of the property.
            /// </summary>
            public string TypePretty { get; set; } = string.Empty;

            /// <summary>
            ///     Name of the property.
            /// </summary>
            public string Name { get; set; } = string.Empty;

            /// <summary>
            ///     Index of the property to be referenced when modifying it.
            /// </summary>
            public int PropertyIndex { get; set; }

            /// <summary>
            ///     Value of the property.
            /// </summary>
            public object? Value { get; set; }
        }
    }

    /// <summary>
    ///     Contains the type names of the components of a remote <see cref="Robust.Shared.GameObjects.EntityUid"/>.
    ///     Requested by <see cref="ViewVariablesRequestEntityComponents"/>.
    /// </summary>
    internal sealed class ViewVariablesBlobEntityComponents : ViewVariablesBlob
    {
        public List<Entry> ComponentTypes { get; set; } = new();

        // This might as well be a ValueTuple but I couldn't get that to work.
        public sealed class Entry : IComparable<Entry>
        {
            public int CompareTo(Entry? other)
            {
                if (ReferenceEquals(this, other)) return 0;
                if (ReferenceEquals(null, other)) return 1;
                return string.Compare(Stringified, other.Stringified, StringComparison.Ordinal);
            }

            public string FullName { get; set; } = string.Empty;
            public string Stringified { get; set; } = string.Empty;
            public string ComponentName { get; set; } = string.Empty;
        }
    }

    internal sealed class ViewVariablesBlobAllValidComponents : ViewVariablesBlob
    {
        public List<string> ComponentTypes { get; set; } = new();
    }

    internal sealed class ViewVariablesBlobAllPrototypes : ViewVariablesBlob
    {
        public List<string> Prototypes { get; set; } = new();
        public string Variant { get; set; } = string.Empty;
    }

    internal sealed class ViewVariablesBlobEnumerable : ViewVariablesBlob
    {
        /// <summary>
        ///     The list of objects inside the range specified by the
        ///     <see cref="ViewVariablesRequestEnumerable"/> used to request this blob.
        /// </summary>
        public List<object> Objects { get; set; } = new();
    }
}
