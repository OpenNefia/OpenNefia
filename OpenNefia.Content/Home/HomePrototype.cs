using OpenNefia.Content.Logic;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Rendering;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenNefia.Content.Home
{
    [Prototype("Elona.Home")]
    public class HomePrototype : IPrototype, IHspIds<int>
    {
        [IdDataField]
        public string ID { get; } = default!;

        /// <inheritdoc/>
        [DataField]
        [NeverPushInheritance]
        public HspIds<int>? HspIds { get; }

        [DataField(required: true)]
        public ResourcePath MapBlueprint { get; set; } = default!;

        [DataField]
        public PrototypeId<ChipPrototype>? AreaEntranceChip { get; set; }

        [DataField]
        public int Value { get; set; }

        [DataField]
        public int HomeScale { get; set; }

        [DataField]
        public int ItemOnFloorLimit { get; set; }

        [DataField]
        public int HomeRankPoints { get; set; }

        [DataField]
        public PrototypeId<MapTilesetPrototype>? TilesetID { get; set; }
    }
}