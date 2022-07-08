using OpenNefia.Content.Damage;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Content.StatusEffects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Random;

namespace OpenNefia.Content.Combat
{
    public sealed class ShieldBashSystem : EntitySystem
    {
        [Dependency] private readonly IRandom _rand = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IStatusEffectSystem _effects = default!;
        [Dependency] private readonly ISkillsSystem _skills = default!;
        [Dependency] private readonly IDamageSystem _damage = default!;
        [Dependency] private readonly ICombatSystem _combat = default!;

        public override void Initialize()
        {
            SubscribeComponent<ShieldBashComponent, BeforeMeleeAttackEvent>(ProcShieldBash);
        }

        private void ProcShieldBash(EntityUid attacker, ShieldBashComponent component, BeforeMeleeAttackEvent args)
        {
            var equipState = _combat.GetEquipState(attacker);
            if (equipState.IsWieldingShield)
                ShieldBash(attacker, args.Target);
        }

        public void ShieldBash(EntityUid attacker, EntityUid target)
        {
            // ﻿>>>>>>>> shade2/action.hsp:1213         if sync(cc):txt lang(name(cc)+"は盾で"+name(t ..
            var shield = _skills.Level(attacker, Protos.Skill.Shield);
            var bashChance = (Math.Clamp(MathF.Sqrt(shield) - 3, 1, 5) / 100f) + ((CompOrNull<ShieldBashComponent>(attacker)?.HasShieldBash ?? false) ? 0.05f : 0f);

            if (_rand.Prob(bashChance))
            {
                _mes.Display(Loc.GetString("Elona.Combat.Melee.ShieldBash", ("attacker", attacker), ("target", target)));
                _damage.DamageHP(target, _rand.Next(shield) + 1, attacker);
                _effects.Apply(target, Protos.StatusEffect.Dimming, (int)(50 + Math.Sqrt(shield) * 15));
                _effects.AddTurns(target, Protos.StatusEffect.Paralysis, _rand.Next(3));
            }
            // <<<<<<<< shade2/action.hsp:1216         cParalyze(tc)+=rnd(3) ..
        }
    }
}