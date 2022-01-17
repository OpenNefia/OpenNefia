using OpenNefia.Content.World;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.UI.Hud
{
    public sealed class HudUpdateSystem : EntitySystem
    {
        [Dependency] private readonly IHudLayer _hud = default!;
        [Dependency] private readonly IMapManager _mapManager = default!;
        public override void Initialize()
        {
            SubscribeLocalEvent<MapOnHoursPassedEvent>(HandleMapHoursPassed, nameof(HandleMapHoursPassed));
            SubscribeLocalEvent<ActiveMapChangedDelegate>(HandleMapChanged, nameof(HandleMapChanged));
        }

        private void HandleMapChanged(ref ActiveMapChangedDelegate ev)
        {
            _hud.UpdateMinimap();
        }

        private void HandleMapHoursPassed(ref MapOnHoursPassedEvent ev)
        {
            _hud.UpdateTime();
        }
    }
}
