using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.Areas
{
    public sealed class AreaStaticFloorsSystem : EntitySystem
    {
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IMapLoader _mapLoader = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<AreaStaticFloorsComponent, AreaFloorGenerateEvent>(OnAreaFloorGenerate, nameof(OnAreaFloorGenerate));
        }

        private void OnAreaFloorGenerate(EntityUid areaEntity, AreaStaticFloorsComponent areaStaticFloors, AreaFloorGenerateEvent args)
        {
            if (args.Handled)
                return;

            if (!areaStaticFloors.Floors.TryGetValue(args.FloorId, out var mapProtoId))
                return;

            var proto = _protos.Index(mapProtoId);
            var mapId = _mapLoader.LoadBlueprint(proto.BlueprintPath).Id;

            args.Handle(mapId);
        }
    }
}
