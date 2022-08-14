using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Maps
{
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public class MapTypeTownComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "MapTypeTown";

        [DataField]
        public int? TownId { get; set; }
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public class MapTypeDungeonComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "MapTypeDungeon";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public class MapTypeGuildComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "MapTypeGuild";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public class MapTypeShelterComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "MapTypeShelter";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public class MapTypeFieldComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "MapTypeField";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public class MapTypeQuestComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "MapTypeQuest";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public class MapTypePlayerOwnedComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "MapTypePlayerOwned";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public class MapTypeWorldMapComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "MapTypeWorldMap";
    }
}
