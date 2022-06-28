using Love;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Maps;
using OpenNefia.Content.World;
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
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.Arena
{
    public interface IAreaArenaSystem : IEntitySystem
    {
    }

    public sealed class AreaArenaSystem : EntitySystem, IAreaArenaSystem
    {
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IWorldSystem _world = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<AreaArenaComponent, AreaMapInitializeEvent>(CheckArenaRenew);
        }

        private void CheckArenaRenew(EntityUid uid, AreaArenaComponent component, AreaMapInitializeEvent args)
        {
            if (!_areaManager.TryGetAreaOfMap(args.Map.Id, out var area)
                || !TryComp<AreaArenaComponent>(area.AreaEntityUid, out var arena))
                return;

            if (_world.State.GameDate > arena.SeedRenewDate)
            {
                arena.Seed = _rand.Next(10000);
                arena.SeedRenewDate = _world.State.GameDate + GameTimeSpan.FromHours(MapTransferSystem.MapRenewMinorIntervalHours);
            }
        }
    }
}