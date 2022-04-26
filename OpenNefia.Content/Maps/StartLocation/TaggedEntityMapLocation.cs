using OpenNefia.Content.GameObjects;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;
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
        [Dependency] private readonly ITagSystem _tags = default!;

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

            var found = _tags.EntityWithTagInMap<SpatialComponent>(map.Id, Tag);

            if (found != null)
            {
                return found.WorldPosition;
            }

            Logger.ErrorS("area.mapIds", $"Failed to find entity with tag {Tag} in the destination map!");
            return new CenterMapLocation().GetStartPosition(ent, map);
        }
    }
}
