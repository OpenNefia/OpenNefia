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
    public sealed class AreaMapBlueprintSystem
        : EntitySystem
    {
        [Dependency] private readonly IMapLoader _mapLoader = default!;

        public override void Initialize()
        {
            SubscribeComponent<AreaMapBlueprintComponent, AreaFloorGenerateEvent>(OnAreaFloorGenerate, priority: EventPriorities.High);
        }

        private void OnAreaFloorGenerate(EntityUid areaEntity, AreaMapBlueprintComponent areaMapBlueprint, AreaFloorGenerateEvent args)
        {
            if (args.Handled)
                return;

            var map = _mapLoader.LoadBlueprint(areaMapBlueprint.BlueprintPath);
            args.Handle(map);
        }
    }
}
