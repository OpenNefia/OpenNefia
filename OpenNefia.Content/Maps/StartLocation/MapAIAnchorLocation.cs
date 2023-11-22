using OpenNefia.Content.VanillaAI;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization.Manager.Attributes;

namespace OpenNefia.Content.Maps
{
    /// <summary>
    /// Places the entity at its AI anchor position
    /// TODO: Merge with <see cref="MapStartLocationComponent"/>?
    /// </summary>
    public class MapAIAnchorLocation : IMapStartLocation
    {
        public MapAIAnchorLocation() { }

        public Vector2i GetStartPosition(EntityUid ent, IMap map)
        {
            if (IoCManager.Resolve<IEntityManager>().TryGetComponent<AIAnchorComponent>(ent, out var anchor))
                return anchor.Anchor;
            return new CenterMapLocation().GetStartPosition(ent, map);
        }
    }
}
