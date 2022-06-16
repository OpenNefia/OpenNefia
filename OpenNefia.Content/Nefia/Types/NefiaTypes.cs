using OpenNefia.Content.Nefia.Generator;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Nefia
{
    public interface INefiaFloorTemplate
    {
        public void Setup(IMap map, NefiaGenParamsComponent nefiaParams);
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public class NefiaGenParamsComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "NefiaGenParams";

        [DataField]
        public float MapLevel { get; set; } = 1;

        [DataField]
        public bool HasMonsterHouses { get; set; } = false;

        [DataField]
        public int MaxGenerationAttempts { get; set; } = 2000;
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public class NefiaGenTypeComponent : Component
    {
        public override string Name => "NefiaGenType";

        [DataField]
        public INefiaFloorType Generator { get; set; } = new NefiaFloorStandard();
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public class NefiaCrowdModifierComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "MapNefiaCrowdModifier";

        [DataField]
        public float MobDensityModifier { get; set; } = 1.0f;

        [DataField]
        public float CrowedDensityModifier { get; set; } = 1.0f;
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public class NefiaRoomsComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "MapNefiaRooms";

        [DataField]
        public List<Room> Rooms { get; } = new();
    }

    [DataDefinition]
    public struct Room
    {
        public Room(UIBox2i bounds, Direction alignment)
        {
            Bounds = bounds;
            Alignment = alignment;
        }

        [DataField]
        public UIBox2i Bounds { get; set; }
        
        [DataField]
        public Direction Alignment { get; set; }
    }
}
