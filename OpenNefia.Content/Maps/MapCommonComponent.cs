using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Maps
{
    [RegisterComponent]
    public class MapCommonComponent : Component, IHspIds<int>
    {
        public override string Name => "MapCommon";

        /// <inheritdoc/>
        [DataField]
        public HspIds<int>? HspIds { get; }

        [DataField]
        public PrototypeId<MusicPrototype>? Music { get; set; }

        [DataField]
        public bool IsIndoors { get; set; } = false;
    }
}
