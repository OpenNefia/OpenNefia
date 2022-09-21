using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;

namespace OpenNefia.Content.Areas
{
    public sealed class AreaBlankMapSystem : EntitySystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;

        public override void Initialize()
        {
            SubscribeComponent<AreaBlankMapComponent, AreaFloorGenerateEvent>(OnAreaFloorGenerate, priority: EventPriorities.High);
        }

        private void OnAreaFloorGenerate(EntityUid areaEntity, AreaBlankMapComponent areaBlankMap, AreaFloorGenerateEvent args)
        {
            if (args.Handled)
                return;

            var map = _mapManager.CreateMap(areaBlankMap.Size.X, areaBlankMap.Size.Y);
            map.Clear(areaBlankMap.Tile);
            args.Handle(map);
        }
    }
}