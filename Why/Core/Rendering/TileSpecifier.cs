using System;
using System.Collections.Generic;
using System.Xml.Linq;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Rendering
{
    [DataDefinition]
    public class TileSpecifier
    {
        [DataField(required: true)]
        public string TileId = "Default";

        /// <summary>
        /// Path to tile or atlas image.
        /// </summary>
        [DataField(required: true)]
        public ResourcePath? ImagePath;

        /// <summary>
        /// Number of tiles in the X direction.
        /// </summary>
        [DataField]
        public int CountX = 1;

        /// <summary>
        /// Region of a texture atlas to use.
        /// If null, the image path should point to a single tile image/strip.
        /// If non-null, indicates this specifier operates on a texture atlas.
        /// </summary>
        [DataField]
        public ImageRegion? ImageRegion;

        public string TileIndex => TileId;
        public bool HasOverhang => false;
    }
}
