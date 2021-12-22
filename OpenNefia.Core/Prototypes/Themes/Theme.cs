using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Serialization.Markdown.Mapping;
using OpenNefia.Core.Serialization.Markdown.Sequence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;

namespace OpenNefia.Core.Prototypes
{
    public interface ITheme
    {
        string ID { get; }

        List<MappingDataNode> Overrides { get; set; }
    }

    /// <summary>
    /// Defines a set of YAML overrides for modifying a set of prototypes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This doesn't go through the prototypes system since the themes need to be loaded
    /// before any prototypes are scanned.
    /// </para>
    /// <para>
    /// TODO support more operations like removing nodes
    /// </para>
    /// </remarks>
    [DataDefinition]
    public class Theme : ITheme
    {
        /// <inheritdoc />
        [DataField("id", required: true)]
        public string ID { get; } = default!;

        /// <summary>
        /// List of overrides.
        /// </summary>
        [DataField(required: true)]
        public List<MappingDataNode> Overrides { get; set; } = default!;
    }
}
