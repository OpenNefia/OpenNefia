using Love;
using OpenNefia.Content.Damage;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Parties;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Random;
using OpenNefia.Core.SaveGames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Core.Game;
using OpenNefia.Content.Factions;

namespace OpenNefia.Content.HealthBar
{
    public interface IHealthBarSystem : IEntitySystem
    {
        bool ShouldShowHealthBarFor(EntityUid uid);
    }

    public sealed class HealthBarSystem : EntitySystem, IHealthBarSystem
    {
        [Dependency] private readonly IPartySystem _parties = default!;
        [Dependency] private readonly IGameSessionManager _gameSession = default!;

        public override void Initialize()
        {
            SubscribeEntity<EntityKilledEvent>(RemoveStethoscopeTarget);
            SubscribeEntity<EntityWoundedEvent>(SetStethoscopeTarget);
        }

        [RegisterSaveData("Elona.HealthBarSystem.LastAttackedTarget")]
        // TODO: save data requires non-nullable references...
        public EntityUid LastAttackedTarget { get; set; } = EntityUid.Invalid;

        public bool ShouldShowHealthBarFor(EntityUid uid)
        {
            return _parties.IsInPlayerParty(uid) || uid == LastAttackedTarget;
        }

        private void SetStethoscopeTarget(EntityUid uid, ref EntityWoundedEvent args)
        {
            if (args.Attacker != null && _gameSession.IsPlayer(args.Attacker.Value))
                LastAttackedTarget = uid;
        }

        private void RemoveStethoscopeTarget(EntityUid uid, ref EntityKilledEvent args)
        {
            if (LastAttackedTarget == uid)
                LastAttackedTarget = EntityUid.Invalid;
        }
    }
}