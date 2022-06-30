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

namespace OpenNefia.Content.Hunger
{
    public interface IHungerSystem : IEntitySystem
    {
        void Vomit(EntityUid entity, HungerComponent? hunger = null);
    }

    public sealed class HungerSystem : EntitySystem, IHungerSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public override void Initialize()
        {
        }

        public const int NutritionThresholdVomit = 35000;

        public void Vomit(EntityUid entity, HungerComponent? hunger = null)
        {
            // TODO: implement
        }
    }
}