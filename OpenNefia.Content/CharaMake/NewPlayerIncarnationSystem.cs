using OpenNefia.Content.Currency;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Factions;
using OpenNefia.Content.Feats;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Identify;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Qualities;
using OpenNefia.Content.RandomGen;
using OpenNefia.Content.Skills;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.CharaMake
{
    public sealed class NewPlayerIncarnationSystem : EntitySystem
    {
        [Dependency] private readonly IRandom _random = default!;
        [Dependency] private readonly IItemGen _itemGen = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IFeatsSystem _feats = default!;
        [Dependency] private readonly IRefreshSystem _refreshSys = default!;

        public const int DefaultSkillBonusPoints = 5;

        public override void Initialize()
        {
            SubscribeComponent<PlayerComponent, NewPlayerIncarnatedEvent>(HandleNewPlayerIncarnated);
        }

        private void HandleNewPlayerIncarnated(EntityUid uid, PlayerComponent player, NewPlayerIncarnatedEvent args)
        {
            var currency = EntityManager.GetComponent<MoneyComponent>(uid);
            currency.Gold = 400 + _random.Next(200);

            // >>>>>>>> shade2/chara.hsp:539 *cm_finishPC ..

            var quality = EntityManager.GetComponent<QualityComponent>(uid);
            quality.Quality.Base = Quality.Normal;

            var inventory = EntityManager.GetComponent<InventoryComponent>(uid);
            _itemGen.GenerateItem(inventory.Container, Item.CargoTravelersFood, amount: 8);
            _itemGen.GenerateItem(inventory.Container, Item.Ration, amount: 8);
            _itemGen.GenerateItem(inventory.Container, Item.BottleOfCrimAle, amount: 2);

            var skills = EntityManager.EnsureComponent<SkillsComponent>(uid);
            if (_skills.Level(uid, Skill.Literacy, skills) == 0)
                _itemGen.GenerateItem(inventory.Container, Item.PotionOfCureMinorWound, amount: 3);

            // TODO class inits

            var skillBonusPoints = DefaultSkillBonusPoints + _feats.Level(uid, Feat.PermSkillPoint);
            _skills.GainBonusPoints(uid, skillBonusPoints, skills);

            foreach (var item in inventory.Container.ContainedEntities)
            {
                if (EntityManager.TryGetComponent(item, out IdentifyComponent identify))
                    identify.IdentifyState = IdentifyState.Full;
            }

            var faction = EntityManager.EnsureComponent<FactionComponent>(uid);
            faction.RelationToPlayer = Relation.Ally;

            _refreshSys.Refresh(uid);

            // <<<<<<<< shade2/chara.hsp:579 	return ..
        }
    }
}
