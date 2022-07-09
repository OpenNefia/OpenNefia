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

namespace OpenNefia.Content.Weight
{
    public interface IWeightSystem : IEntitySystem
    {
        void ModifyWeight(EntityUid ent, int delta, bool force = false, WeightComponent? weight = null);
    }

    public sealed class WeightSystem : EntitySystem, IWeightSystem
    {
        [Dependency] private readonly IMapManager _mapManager = default!;
        [Dependency] private readonly IAreaManager _areaManager = default!;
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IEntityLookup _lookup = default!;

        public void ModifyWeight(EntityUid ent, int delta, bool force = false, WeightComponent? weight = null)
        {
            if (!Resolve(ent, ref weight))
                return;

            var height = weight.Height;
            var min = height * height * 18 / 25000;
            var max = height * height * 24 / 10000;

            if (weight.Weight < min)
            {
                weight.Weight = min;
                return;
            }
            if (delta > 0 && weight.Weight > max && !force)
                return;

            weight.Weight = Math.Max(1, weight.Weight * (100 + delta) / 100 + (delta > 0 ? 1 : 0) - (delta < 0 ? 1 : 0));

            if (delta > 2 )
            {
                _mes.Display(Loc.GetString("Elona.Weight.Weight.Gain", ("entity", ent)));
            }
            else if (delta < -2)
            {
                _mes.Display(Loc.GetString("Elona.Weight.Weight.Lose", ("entity", ent)));
            }
        }
    }
}