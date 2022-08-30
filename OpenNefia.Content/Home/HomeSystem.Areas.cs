using OpenNefia.Content.Areas;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Nefia;
using OpenNefia.Content.RandomAreas;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Maps;
using OpenNefia.Content.EntityGen;
using OpenNefia.Core.Locale;

namespace OpenNefia.Content.Home
{
    public sealed partial class HomeSystem
    {
        private void Initialize_Areas()
        {
            SubscribeEntity<GetAreaEntranceMessageEvent>(GetHomeEntranceMessage);
            SubscribeComponent<AreaHomeTutorialComponent, AreaFloorGenerateEvent>(AreaHomeTutorial_FloorGenerate);
        }

        private void GetHomeEntranceMessage(EntityUid areaEntity, GetAreaEntranceMessageEvent args)
        {
            if (!TryArea(areaEntity, out var area))
                return;

            foreach (var floor in area.ContainedMaps.Values)
            {
                if (floor.MapId == ActiveHomeID)
                {
                    args.OutMessage = Loc.GetString("Elona.Home.Map.Description");
                    return;
                }
            }
        }

        private void AreaHomeTutorial_FloorGenerate(EntityUid uid, AreaHomeTutorialComponent component, AreaFloorGenerateEvent args)
        {
            if (args.OutMap == null)
                return;

            if (args.FloorId.FloorNumber != 0)
                return;

            // TODO tutorial
        }
    }
}
