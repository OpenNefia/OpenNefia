using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
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

        public float MapLevel { get; set; } = 1;
        public bool HasMonsterHouses { get; set; } = false;
        public int MaxGenerationAttempts { get; set; } = 2000;
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public class NefiaLayoutStandardComponent : Component
    {
        public override string Name => "NefiaLayoutStandard";
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public class NefiaCrowdModifierComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "MapNefiaCrowdModifier";

        public float MobDensityModifier { get; set; } = 1.0f;
        public float CrowedDensityModifier { get; set; } = 1.0f;
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public class NefiaRoomsComponent : Component
    {
        /// <inheritdoc />
        public override string Name => "MapNefiaCrowdModifier";

        public List<Room> Rooms { get; } = new();
    }

    public struct Room
    {
        public UIBox2i Bounds { get; set; }
        public Direction Alignment { get; set; }
    }
}
