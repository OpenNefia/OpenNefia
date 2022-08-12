using OpenNefia.Content.Damage;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Content.TurnOrder;
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

namespace OpenNefia.Content.EtherDisease
{
    public interface IEtherDiseaseSystem : IEntitySystem
    {
        void ModifyCorruption(EntityUid player, int v, EtherDiseaseComponent? etherDisease = null);
    }

    public sealed class EtherDiseaseSystem : EntitySystem, IEtherDiseaseSystem
    {
        [Dependency] private readonly IDamageSystem _damage = default!;

        public const int EtherDiseaseDeathThreshold = 20000;

        public override void Initialize()
        {
            SubscribeComponent<EtherDiseaseComponent, BeforeTurnBeginEventArgs>(ProcEtherDiseaseDeath);
        }

        private void ProcEtherDiseaseDeath(EntityUid uid, EtherDiseaseComponent etherDisease, BeforeTurnBeginEventArgs args)
        {
            if (!EntityManager.IsAlive(uid))
                return;

            if (etherDisease.Corruption >= EtherDiseaseDeathThreshold)
            {
                if (TryComp<SkillsComponent>(uid, out var skills))
                    _damage.DamageHP(uid, Math.Max(999999, skills.MaxHP), damageType: new GenericDamageType("Elona.DamageType.EtherDisease"));
            }
        }

        public void ModifyCorruption(EntityUid player, int v, EtherDiseaseComponent? etherDisease = null)
        {
            // TODO
        }
    }
}