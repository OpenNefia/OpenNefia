using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Maps
{
    public class MapCommonSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<MapCreatedEvent>(AddRequiredComponents, nameof(AddRequiredComponents));
        }

        private void AddRequiredComponents(MapCreatedEvent args)
        {
            EntityManager.EnsureComponent<MapCommonComponent>(args.Map.MapEntityUid);
        }
    }
}
