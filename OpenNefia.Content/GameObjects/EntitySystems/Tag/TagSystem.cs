using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects.EntitySystems.Tag
{
    public interface ITagSystem : IEntitySystem
    {
        bool HasTag(EntityUid entity, PrototypeId<TagPrototype> tag, TagComponent? tagComp = null);

        IEnumerable<TagComponent> EntitiesWithTagInMap(MapId mapId, PrototypeId<TagPrototype> tag);
        IEnumerable<T> EntitiesWithTagInMap<T>(MapId mapId, PrototypeId<TagPrototype> tag)
            where T : IComponent;

        TagComponent? EntityWithTagInMap(MapId mapId, PrototypeId<TagPrototype> tag);
        T? EntityWithTagInMap<T>(MapId mapId, PrototypeId<TagPrototype> tag)
            where T : IComponent;
    }

    public sealed class TagSystem : EntitySystem, ITagSystem
    {
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public bool HasTag(EntityUid entity, PrototypeId<TagPrototype> tag, TagComponent? tagComp = null)
        {
            if (!Resolve(entity, ref tagComp))
                return false;
            return tagComp.HasTag(tag);
        }

        public IEnumerable<TagComponent> EntitiesWithTagInMap(MapId mapId, PrototypeId<TagPrototype> tag)
        {
            return _lookup.EntityQueryInMap<TagComponent>(mapId)
                .Where(comp => comp.HasTag(tag));
        }

        public IEnumerable<T> EntitiesWithTagInMap<T>(MapId mapId, PrototypeId<TagPrototype> tag)
            where T : IComponent
        {
            return _lookup.EntityQueryInMap<TagComponent, T>(mapId)
                .Where(pair => pair.Item1.HasTag(tag))
                .Select(pair => pair.Item2);
        }

        public TagComponent? EntityWithTagInMap(MapId mapId, PrototypeId<TagPrototype> tag)
            => EntitiesWithTagInMap(mapId, tag).FirstOrDefault();

        public T? EntityWithTagInMap<T>(MapId mapId, PrototypeId<TagPrototype> tag)
            where T : IComponent
            => EntitiesWithTagInMap<T>(mapId, tag).FirstOrDefault();
    }
}
