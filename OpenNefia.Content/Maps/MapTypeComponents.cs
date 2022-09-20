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
        [DataField]
        public int? TownId { get; set; }
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public class MapTypeDungeonComponent : Component
    {    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public class MapTypeGuildComponent : Component
    {    }
    
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public class MapTypeShelterComponent : Component
    {    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public class MapTypeFieldComponent : Component
    {    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public class MapTypeQuestComponent : Component
    {    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public class MapTypePlayerOwnedComponent : Component
    {    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public class MapTypeWorldMapComponent : Component
    {    }
}
