using OpenNefia.Content.Activity;
using OpenNefia.Content.EntityGen;
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
using OpenNefia.Content.Food;
using OpenNefia.Content.Feats;
using OpenNefia.Content.Sanity;
using OpenNefia.Content.StatusEffects;

namespace OpenNefia.Content.Food
{
    public interface ICorpseSystem : IEntitySystem
    {
    }

    public sealed class CorpseSystem : EntitySystem, ICorpseSystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IFoodSystem _food = default!;
        [Dependency] private readonly IFeatsSystem _feats = default!;
        [Dependency] private readonly ISanitySystem _sanity = default!;
        [Dependency] private readonly IStatusEffectSystem _statusEffects = default!;

        public override void Initialize()
        {
            SubscribeComponent<CorpseComponent, BeforeApplyFoodEffectsEvent>(Corpse_BeforeItemEaten);
        }

        private void Corpse_BeforeItemEaten(EntityUid uid, CorpseComponent component, BeforeApplyFoodEffectsEvent args)
        {
            if (_food.IsHumanFlesh(uid))
            {
                if (_feats.HasFeat(args.Eater, Protos.Feat.EatHuman))
                {
                    _mes.Display(Loc.GetString("Elona.Food.Message.Human.Like"));
                }
                else
                {
                    _mes.Display(Loc.GetString("Elona.Food.Message.Human.Dislike"));
                    _sanity.DamageSanity(args.Eater, 15);
                    _statusEffects.Apply(args.Eater, Protos.StatusEffect.Insanity, 150);
                    if (!_feats.HasFeat(args.Eater, Protos.Feat.EatHuman) && _rand.OneIn(5))
                    {
                        _feats.AddLevel(args.Eater, Protos.Feat.EatHuman, 1);
                    }
                }
            }
            else if (_feats.HasFeat(args.Eater, Protos.Feat.EatHuman))
            {
                _mes.Display(Loc.GetString("Elona.Food.Message.Human.WouldHaveRatherEaten"));
                args.OutNutrition /= 2;
            }

            // TODO corpse effects
        }
    }
}