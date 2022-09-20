using OpenNefia.Content.Maps;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
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
    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public class NefiaCrowdDensityModifierComponent : Component
    {
        [DataField(required: true)]
        public INefiaCrowdDensityModifier Modifier = new SimpleCrowdDensityModifier();
    }

    public sealed record NefiaCrowdDensity(int MobCount, int ItemCount);

    [ImplicitDataDefinitionForInheritors]
    public interface INefiaCrowdDensityModifier
    {
        public NefiaCrowdDensity Calculate(IMap map);
    }

    public class SimpleCrowdDensityModifier : INefiaCrowdDensityModifier
    {
        [DataField]
        public int MobModifier { get; set; }

        [DataField]
        public int ItemModifier { get; set; }

        public SimpleCrowdDensityModifier() : this(4, 4) {}

        public SimpleCrowdDensityModifier(int mob, int item)
        {
            MobModifier = mob;
            ItemModifier = item;
        }

        public NefiaCrowdDensity Calculate(IMap map)
        {
            var entityMan = IoCManager.Resolve<IEntityManager>();
            var crowdDensity = entityMan.EnsureComponent<MapCharaGenComponent>(map.MapEntityUid).MaxCharaCount;
            return new(crowdDensity / MobModifier, crowdDensity / ItemModifier);
        }
    }

    [RegisterComponent]
    [ComponentUsage(ComponentTarget.Map)]
    public class NefiaRoomsComponent : Component
    {
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
