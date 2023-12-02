using OpenNefia.Content.Logic;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.LecchoTorte.QuickStart
{
    public sealed class MapLoadGlobalAreasSystem : EntitySystem
    {
        [Dependency] private readonly IGlobalAreaSystem _globalAreas = default!;

        public override void Initialize()
        {
            SubscribeComponent<MapLoadGlobalAreasComponent, AfterMapEnterEventArgs>(LoadGlobalAreasOnEnter, priority: EventPriorities.VeryLow);
        }

        private void LoadGlobalAreasOnEnter(EntityUid uid, MapLoadGlobalAreasComponent component, AfterMapEnterEventArgs args)
        {
            _globalAreas.InitializeGlobalAreas(component.InitGlobalAreas);
        }
    }
}