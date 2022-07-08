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

namespace OpenNefia.Content.HealthBar
{
    public interface IHealthBarSystem : IEntitySystem
    {
        bool ShouldShowHealthBarFor(EntityUid uid);
    }

    public sealed class HealthBarSystem : EntitySystem, IHealthBarSystem
    {
        [Dependency] private readonly IPartySystem _parties = default!;

        public override void Initialize()
        {
            SubscribeEntity<EntityKilledEvent>(RemoveStethoscopeTarget);
        }

        [RegisterSaveData("Elona.HealthBarSystem.StethoscopeTarget")]
        public EntityUid? StethoscopeTarget { get; set; }

        public bool ShouldShowHealthBarFor(EntityUid uid)
{
            return _parties.IsInPlayerParty(uid) || uid == StethoscopeTarget;
        }

        private void RemoveStethoscopeTarget(EntityUid uid, ref EntityKilledEvent args)
        {
            if (StethoscopeTarget == uid)
                StethoscopeTarget = null;
        }
    }
}