using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Log;
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
        [Dependency] private readonly IPrototypeManager _protos = default!;
        [Dependency] private readonly IMapLoader _mapLoader = default!;

        public override void Initialize()
        {
            SubscribeComponent<AreaStaticFloorsComponent, AreaFloorGenerateEvent>(OnAreaFloorGenerate, priority: EventPriorities.VeryHigh);
        }

        private void OnAreaFloorGenerate(EntityUid areaEntity, AreaStaticFloorsComponent areaStaticFloors, AreaFloorGenerateEvent args)
        {
            if (args.Handled)
                return;

            if (!areaStaticFloors.Floors.TryGetValue(args.FloorId, out var mapProtoId))
            {
                Logger.WarningS("area.gen", $"No static floor in area {args.Area} for floor {args.FloorId}.");
                return;
            }

            var proto = _protos.Index(mapProtoId);
            var map = _mapLoader.LoadBlueprint(proto.BlueprintPath);

            args.Handle(map);
        }
    }
}
