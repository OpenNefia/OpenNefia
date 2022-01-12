using OpenNefia.Content.GameObjects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Maps
{
    /// <summary>
    /// Places the player on top the entity with the given <see cref="TagPrototype"/>.
    /// </summary>
    public class TaggedEntityMapLocation : IMapStartLocation
    {
        [Dependency] private readonly IEntityLookup _lookup = default!;

        [DataField]
        public PrototypeId<TagPrototype> Tag { get; }

        public TaggedEntityMapLocation() { }

        public TaggedEntityMapLocation(PrototypeId<TagPrototype> tag)
        {
            Tag = tag;
        }

        public Vector2i GetStartPosition(EntityUid ent, IMap map)
        {
            EntitySystem.InjectDependencies(this);

            var found = _lookup.EntityQueryInMap<TagComponent, SpatialComponent>(map.Id)
                .Where(pair => pair.Item1.HasTag(Tag))
                .FirstOrDefault();

            if (found != default)
            {
                var (_, foundSpatial) = found;
                return foundSpatial.WorldPosition;
            }

            Logger.ErrorS("area.mapIds", $"Failed to find entity with tag {Tag} in the destination map!");
            return new CenterMapLocation().GetStartPosition(ent, map);
        }
    }
}
