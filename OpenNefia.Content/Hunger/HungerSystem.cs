using OpenNefia.Content.EntityGen;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.Game;
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
        [Dependency] private readonly IGameSessionManager _gameSession = default!;

        public override void Initialize()
        {
            SubscribeComponent<HungerComponent, EntityBeingGeneratedEvent>(InitializeNutrition, priority: EventPriorities.High);
        }

        private void InitializeNutrition(EntityUid uid, HungerComponent component, ref EntityBeingGeneratedEvent args)
        {
            // >>>>>>>> shade2/chara.hsp:516 	if rc=pc:cHunger(rc)=9000:else:cHunger(rc)=defAll ..
            if (_gameSession.IsPlayer(uid))
                component.Nutrition = 9000;
            else
                component.Nutrition = HungerLevels.Ally - 1000 + _rand.Next(4000);
            // <<<<<<<< shade2/chara.hsp:516 	if rc=pc:cHunger(rc)=9000:else:cHunger(rc)=defAll ..
        }

        public void Vomit(EntityUid entity, HungerComponent? hunger = null)
        {
            // TODO: implement
        }
    }
}