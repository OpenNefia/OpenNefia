using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;

namespace OpenNefia.Core.Rendering
{
    [Prototype("Asset", -1)]
    public class AssetPrototype : IPrototype
    {
        [DataField("id", required: true)]
        public string ID { get; private set; } = default!;

        /// <summary>
        /// Image to use for this asset.
        /// </summary>
        [DataField(required: true)]
        public AssetSpecifier Image = default!;

        /// <summary>
        /// Number of tiled images in the X direction. Each will get its own quad when the asset is instantiated.
        /// </summary>
        [DataField]
        public uint CountX = 1;

        /// <summary>
        /// Number of tiled images in the Y direction. Each will get its own quad when the asset is instantiated.
        /// </summary>
        [DataField]
        public uint CountY = 1;

        /// <summary>
        /// List of regions available when making an asset batch from this <see cref="AssetDef"/>.
        /// </summary>
        [DataField]
        public AssetRegions Regions = new();

        /// <summary>
        /// Specifier for custom texture region generation logic. Used by things like the message window.
        /// </summary>
        [DataField]
        public IRegionSpecifier? RegionSpecifier;

        public bool RequiresSizeArgument => RegionSpecifier != null;
    }

    public class AssetRegions : Dictionary<string, Box2i>
    {
    }

    [DataDefinition]
    public class AssetSpecifier
    {
        /// <summary>
        /// Path of the image to use.
        /// </summary>
        [DataField(required: true)]
        public ResourcePath Filepath = default!;

        /// <summary>
        /// Information for a region of an image to cut out and use for this <see cref="AssetDef"/>.
        /// </summary>
        [DataField]
        public Box2i? Region;

        /// <summary>
        /// Color in the image to replace with transparency.
        /// </summary>
        /// <remarks>
        /// This is for the convenience of being able to use existing .BMP images.
        /// </remarks>
        [DataField]
        public Color? KeyColor { get; set; } = null;

        /// <summary>
        /// Filter to apply to the image.
        /// </summary>
        [DataField]
        public ImageFilter? Filter;
    }
}
