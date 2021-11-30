using System;
using System.Collections.Generic;
using System.Xml.Linq;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Rendering
{
    [DataDefinition]
    public class TileSpecifier
    {
        /// <summary>
        /// Path to tile or atlas image.
        /// </summary>
        [DataField(required: true)]
        public ResourcePath Filepath { get; } = default!;

        /// <summary>
        /// Number of tiles in the X direction.
        /// </summary>
        [DataField]
        public int CountX { get; } = 1;

        /// <summary>
        /// Region of a texture atlas to use.
        /// If null, the image path should point to a single tile image/strip.
        /// If non-null, indicates this specifier operates on a texture atlas.
        /// </summary>
        [DataField]
        public Box2i? Region { get; }

        /// <summary>
        /// Internal string to use for graphics purposes in the tile atlases.
        /// </summary>
        public string AtlasIndex { get; internal set; } = string.Empty;

        public bool HasOverhang { get; internal set; } = false;
    }
}
