using OpenNefia.Content.RandomGen;
using OpenNefia.Core.Maps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.IoC;
using OpenNefia.Content.Qualities;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;

namespace OpenNefia.Content.Maps
{
    public sealed class TownCharaFilterGen : IMapCharaFilterGen
    {
        [Dependency] private readonly IRandomGenSystem _randomGen = default!;
        [Dependency] private readonly IRandom _rand = default!;

        [DataField("extraChoices")]
        private List<TownCharaFilterExtraChoice> _extraChoices = new();
        public IReadOnlyList<TownCharaFilterExtraChoice> ExtraChoices => _extraChoices;

        public CharaFilter GenerateFilter(IMap map)
        {
            // >>>>>>>> shade2/map.hsp:9 	if (mType=mTypeTown)or(mType=mTypeVillage){ ..
            var filter = new CharaFilter()
            {
                MinLevel = _randomGen.CalcObjectLevel(10),
                Quality = _randomGen.CalcObjectQuality(Quality.Normal),
                FltSelect = FltSelects.Town
            };

            foreach (var choice in _extraChoices)
            {
                if (choice.OneIn != null && !_rand.OneIn(choice.OneIn.Value))
                    continue;
                else if (choice.Prob != null && !_rand.Prob(choice.Prob.Value))
                    continue;

                filter.Id = choice.ID;
                break;
            }

            return filter;
            // <<<<<<<< shade2/map.hsp:29 		} ..
        }
    }

    [DataDefinition]
    public sealed class TownCharaFilterExtraChoice
    {
        [DataField(required: true)]
        public PrototypeId<EntityPrototype> ID { get; }

        [DataField]
        public int? OneIn { get; }

        [DataField]
        public float? Prob { get; }
    }
}
