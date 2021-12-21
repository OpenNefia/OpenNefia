using OpenNefia.Content.Logic;
using OpenNefia.Content.Prototypes;
using OpenNefia.Core.Audio;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects
{
    public class CombatSystem : EntitySystem
    {
        [Dependency] private readonly IAudioSystem _sounds = default!;

        public override void Initialize()
        {
            SubscribeLocalEvent<MoveableComponent, CollideWithEventArgs>(HandleCollideWith, nameof(HandleCollideWith));
            SubscribeLocalEvent<StatusFearComponent, PhysicalAttackEventArgs>(HandlePhysicalAttackFear, nameof(HandlePhysicalAttackFear), 
                before: new[] { new SubId(typeof(CombatSystem), nameof(HandlePhysicalAttackMain))});
            SubscribeLocalEvent<SkillsComponent, PhysicalAttackEventArgs>(HandlePhysicalAttackMain, nameof(HandlePhysicalAttackMain));
        }

        private void HandlePhysicalAttackFear(EntityUid uid, StatusFearComponent component, PhysicalAttackEventArgs args)
        {
            if (args.Handled)
                return;

            Mes.Display(DisplayNameSystem.GetDisplayName(uid) + " runs away in fear.");
            args.Handled = true;
        }

        private void HandlePhysicalAttackMain(EntityUid uid, SkillsComponent component, PhysicalAttackEventArgs args)
        {
            if (args.Handled)
                return;

            Mes.Display($"{DisplayNameSystem.GetDisplayName(uid)} punches {DisplayNameSystem.GetDisplayName(args.Target)}");
            _sounds.Play(Protos.Sound.Atk2, args.Target);
            EntityManager.DeleteEntity(args.Target);
            args.Handled = true;
        }

        private void HandleCollideWith(EntityUid uid, MoveableComponent _, CollideWithEventArgs args)
        {
            if (!EntityManager.HasComponent<MoveableComponent>(args.Target))
                return;

            var turnResult = MeleeAttack(uid, args.Target);

            if (turnResult != null)
            {
                args.Handled = true;
                args.TurnResult = turnResult.Value;
                return;
            }
        }

        public TurnResult? MeleeAttack(EntityUid attacker, EntityUid target)
        {
            var ev = new GetMeleeWeaponsEvent();
            RaiseLocalEvent(attacker, ev);

            if (ev.Weapons.Count > 0)
            {
                foreach (var (weapon, attackCount) in ev.Weapons.WithIndex())
                {
                    PhysicalAttack(attacker, target, weapon, attackCount);
                }
                return TurnResult.Succeeded;
            }
            else
            {
                PhysicalAttack(attacker, target, null);
                return TurnResult.Succeeded;
            }
        }

        public PrototypeId<SkillPrototype> GetPhysicalAttackSkill(EntityUid? weapon, WeaponComponent? weaponComp = null)
        {
            if (weapon != null && Resolve(weapon.Value, ref weaponComp))
            {
                return weaponComp.UsedSkill;
            }
            return SkillPrototypeOf.MartialArts;
        }

        /// <summary>
        /// Causes a physical attack.
        /// </summary>
        /// <param name="attacker">Attacking entity.</param>
        /// <param name="target">Target being attacked.</param>
        /// <param name="weapon">Weapon used in the attack, if any.</param>
        /// <param name="attackCount">
        /// Number of attacks so far.
        /// 
        /// The first weapon gets count 0, the second count 1, and so on. 
        /// </param>
        public void PhysicalAttack(EntityUid attacker, EntityUid target, EntityUid? weapon = null,
            int attackCount = 0)
        {
            if (!EntityManager.IsAlive(attacker) || !EntityManager.IsAlive(target))
                return;

            var ev = new PhysicalAttackEventArgs()
            {
                Attacker = attacker,
                Target = target,
                Weapon = weapon,
                AttackCount = attackCount
            };
            RaiseLocalEvent(attacker, ev);
        }
    }

    public class PhysicalAttackEventArgs : HandledEntityEventArgs
    {
        public EntityUid Attacker;
        public EntityUid Target;
        public EntityUid? Weapon;
        public int AttackCount;
    }

    public class GetMeleeWeaponsEvent : EntityEventArgs
    {
        public List<EntityUid> Weapons = new();
    }
}
