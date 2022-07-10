using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.World;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Adventurer
{
    public interface IAdventurerSystem : IEntitySystem
    {
        bool TryGetArea(EntityUid adventurer, [NotNullWhen(true)] out IArea? area);
    }

    public sealed class AdventurerSystem : EntitySystem, IAdventurerSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public override void Initialize()
        {
            SubscribeEntity<MapOnTimePassedEvent>(ProcUpdateAdventurers);
        }

        public bool TryGetArea(EntityUid value, [NotNullWhen(true)] out IArea? area)
        {
            // TODO
            return TryArea(value, out area);
        }

        private void ProcUpdateAdventurers(EntityUid uid, ref MapOnTimePassedEvent args)
        {
            if (args.HoursPassed <= 0)
                return;

            // TODO
        }
    }
}