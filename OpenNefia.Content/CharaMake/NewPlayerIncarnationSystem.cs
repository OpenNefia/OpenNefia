using OpenNefia.Content.Currency;
using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Feats;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Inventory;
using OpenNefia.Content.Qualities;
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
        [Dependency] private readonly IEntityGen _entityGen = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IFeatsSystem _feats = default!;
        [Dependency] private readonly IRefreshSystem _refreshSys = default!;

        public const int DefaultSkillBonusPoints = 5;

        public override void Initialize()
        {
            SubscribeLocalEvent<PlayerComponent, NewPlayerIncarnatedEvent>(HandleNewPlayerIncarnated, nameof(HandleNewPlayerIncarnated));
        }

        private void HandleNewPlayerIncarnated(EntityUid uid, PlayerComponent player, NewPlayerIncarnatedEvent args)
        {
            var currency = EntityManager.GetComponent<WalletComponent>(uid);
            currency.Gold = 400 + _random.Next(200);

            // >>>>>>>> shade2/chara.hsp:539 *cm_finishPC ..

            var quality = EntityManager.GetComponent<QualityComponent>(uid);
            quality.Quality.Base = Quality.Normal;

            var inventory = EntityManager.GetComponent<InventoryComponent>(uid);
            _entityGen.SpawnEntity(Item.CargoTravelersFood, inventory.Container, count: 8);
            _entityGen.SpawnEntity(Item.Ration, inventory.Container, count: 8);
            _entityGen.SpawnEntity(Item.BottleOfCrimAle, inventory.Container, count: 2);

            var skills = EntityManager.EnsureComponent<SkillsComponent>(uid);
            if (_skills.Level(skills, Skill.Literacy) == 0)
                _entityGen.SpawnEntity(Item.PotionOfCureMinorWound, inventory.Container, count: 3);

            // TODO class inits

            var feats = EntityManager.EnsureComponent<FeatsComponent>(uid);

            var skillBonusPoints = DefaultSkillBonusPoints + _feats.Level(feats, CharaFeat.PermSkillPoint);
            _skills.GainBonusPoints(uid, skillBonusPoints, skills);

            foreach (var item in inventory.Container.ContainedEntities)
            {
                if (EntityManager.TryGetComponent(item, out IdentifyComponent identify))
                    identify.IdentifyState = IdentifyState.Full;
            }

            _refreshSys.Refresh(uid);

            // <<<<<<<< shade2/chara.hsp:579 	return ..
        }
    }
}
