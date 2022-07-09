using OpenNefia.Content.Logic;
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

namespace OpenNefia.Content.Sleep
{
    public interface ISleepSystem : IEntitySystem
    {
        bool IsPlayerSleeping { get; }
    }

    public sealed class SleepSystem : EntitySystem, ISleepSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public const int SleepThresholdHoursLight = 15;
        public const int SleepThresholdHoursModerate = 30;
        public const int SleepThresholdHoursHeavy = 50;

        public bool IsPlayerSleeping { get; private set; } = false;

        public override void Initialize()
        {
        }
    }
}