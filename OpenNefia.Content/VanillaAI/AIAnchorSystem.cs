using OpenNefia.Content.EntityGen;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.VanillaAI
{
    public class AIAnchorSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;

        public override void Initialize()
        {
            SubscribeComponent<VanillaAIComponent, EntityGeneratedEvent>(HandleGenerated);
            SubscribeComponent<AIAnchorComponent, ComponentStartup>(HandleStartup);
        }

        private void HandleGenerated(EntityUid uid, VanillaAIComponent ai, ref EntityGeneratedEvent args)
        {
            if (!EntityManager.TryGetComponent(uid, out SpatialComponent spatial))
                return;

            if (!_mapManager.TryGetMap(spatial.MapID, out var map))
                return;

            if (!EntityManager.TryGetComponent(map.MapEntityUid, out MapVanillaAIComponent mapAi))
                return;

            if (mapAi.AnchorCitizens)
            {
                EntityManager.EnsureComponent<AIAnchorComponent>(uid);
            }
        }

        private void HandleStartup(EntityUid uid, AIAnchorComponent component, ComponentStartup args)
        {
            if (!EntityManager.TryGetComponent(uid, out SpatialComponent spatial))
                return;

            component.InitialPosition = spatial.WorldPosition;
            component.Anchor = component.InitialPosition;
        }
    }
}
