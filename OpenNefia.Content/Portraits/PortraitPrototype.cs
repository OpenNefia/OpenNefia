﻿using OpenNefia.Content.Charas;
using OpenNefia.Content.Rendering;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Portraits
{
    [Prototype("Elona.Portrait")]
    public class PortraitPrototype : IPrototype, IHspIds<int>, IAtlasRegionProvider
    {
        [IdDataField]
        public string ID { get; } = default!;

        /// <inheritdoc/>
        [DataField]
        [NeverPushInheritance]
        public HspIds<int>? HspIds { get; }

        [DataField(required: true)]
        public TileSpecifier Image { get; } = null!;

        [DataField]
        public Gender? Gender { get; }

        [DataField]
        public bool RandomlyGenerate { get; } = false;

        public IEnumerable<AtlasRegion> GetAtlasRegions()
        {
            yield return new(ContentAtlasNames.Portrait, $"{ID}:Default", Image);
        }
    }
}
