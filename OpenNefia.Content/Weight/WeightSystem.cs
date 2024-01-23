using OpenNefia.Content.EntityGen;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Weight
{
    public interface IWeightSystem : IEntitySystem
    {
        /// <summary>
        /// Gets the total weight of this entity *and* all contained entities recursively.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        int GetTotalWeight(EntityUid entity, bool excludeSelf = false, int? stackCount = null, WeightComponent? weight = null);

        void ModifyWeight(EntityUid ent, int delta, bool force = false, WeightComponent? weight = null);
    }

    public sealed class WeightSystem : EntitySystem, IWeightSystem
    {
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IStackSystem _stacks = default!;

        public override void Initialize()
        {
        }

        /// <inheritdoc/>
        public int GetTotalWeight(EntityUid entity, bool excludeSelf = false, int? stackCount = null, WeightComponent? weight = null)
        {
            int total = 0;

            if (Resolve(entity, ref weight, logMissing: false))
                total = weight.Weight.Buffed;

            if (excludeSelf)
                total = 0;

            // XXX: Should some contained entities not count towards
            // total weight? There may be some child entities not inside
            // containers, for example.
            foreach (var child in Spatial(entity).ChildEntities)
            {
                total += GetTotalWeight(child);
            }

            return total * (stackCount ?? _stacks.GetCount(entity));
        }

        public void ModifyWeight(EntityUid ent, int delta, bool force = false, WeightComponent? weight = null)
        {
            if (!Resolve(ent, ref weight))
                return;

            var height = weight.Height;
            var min = height * height * 18 / 25000;
            var max = height * height * 24 / 10000;

            if (weight.Weight.Base < min)
            {
                weight.Weight.Base = min;
                return;
            }
            if (delta > 0 && weight.Weight.Base > max && !force)
                return;

            weight.Weight.Base = Math.Max(1, weight.Weight.Base * (100 + delta) / 100 + (delta > 0 ? 1 : 0) - (delta < 0 ? 1 : 0));

            if (delta > 2)
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