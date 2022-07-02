using OpenNefia.Content.Charas;
using OpenNefia.Content.DisplayName;
using OpenNefia.Content.Equipment;
using OpenNefia.Content.EquipSlots;
using OpenNefia.Content.Factions;
using OpenNefia.Content.GameObjects;
using OpenNefia.Content.GameObjects.EntitySystems.Tag;
using OpenNefia.Content.Input;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Core.Audio;
using OpenNefia.Core.Configuration;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenNefia.Content.Prototypes.Protos;

namespace OpenNefia.Content.Combat
{
    public sealed record EquipState(int AttackCount, bool IsWieldingShield, bool IsWieldingTwoHanded, bool IsDualWielding);

    public interface ICombatSystem
    {
        EquipState GetEquipState(EntityUid ent);
        TurnResult? MeleeAttack(EntityUid attacker, EntityUid target);
    }

    public class CombatSystem : EntitySystem, ICombatSystem
    {
        [Dependency] private readonly IAudioManager _sounds = default!;
        [Dependency] private readonly IFactionSystem _factions = default!;
        [Dependency] private readonly IRandom _random = default!;
        [Dependency] private readonly IDisplayNameSystem _displayNames = default!;
        [Dependency] private readonly IMessagesManager _mes = default!;
        [Dependency] private readonly IMapDebrisSystem _mapDebris = default!;
        [Dependency] private readonly IConfigurationManager _config = default!;
        [Dependency] private readonly IInputSystem _input = default!;
        [Dependency] private readonly IEquipSlotsSystem _equipSlots = default!;
        [Dependency] private readonly ITagSystem _tags = default!;

        public override void Initialize()
        {
            SubscribeComponent<SkillsComponent, PhysicalAttackEventArgs>(HandlePhysicalAttackMain);
        }

        private void HandlePhysicalAttackMain(EntityUid uid, SkillsComponent skills, PhysicalAttackEventArgs args)
        {
            if (args.Handled)
                return;

            if (!EntityManager.TryGetComponent(args.Target, out SkillsComponent targetSkills))
                return;

            _mes.Display($"{_displayNames.GetDisplayName(uid)} punches {_displayNames.GetDisplayName(args.Target)}");

            _sounds.Play(Sound.Atk1, args.Target);
            targetSkills.HP--;
            if (targetSkills.HP < 0)
            {
                _mes.Display($"{_displayNames.GetDisplayName(uid)} kills {_displayNames.GetDisplayName(args.Target)}!");
                KillEntity(args.Target);
            }

            args.Handled = true;
        }

        public EquipState GetEquipState(EntityUid ent)
        {
            var attackCount = 0;
            var isWieldingShield = false;
            var isWieldingTwoHanded = false;
            var isDualWielding = false;

            foreach (var equipSlot in _equipSlots.GetEquipSlots(ent))
            {
                if (!_equipSlots.TryGetContainerForEquipSlot(ent, equipSlot, out var container))
                    continue;

                if (!EntityManager.IsAlive(container.ContainedEntity))
                    continue;

                if (TryComp<EquipmentComponent>(container.ContainedEntity.Value, out var equip))
                {
                    // TODO generalize
                    if (equip.EquipSlots.Contains(Protos.EquipSlot.Hand))
                        attackCount++;

                    if (_tags.HasTag(equip.Owner, Protos.Tag.ItemCatEquipShield))
                        isWieldingShield = true;
                }
            }

            if (isWieldingShield)
            {
                if (TryComp<SkillsComponent>(ent, out var skills))
                {
                    if (skills.PV.Buffed > 0)
                    {
                        skills.PV.Buffed *= (int)((120 + Math.Sqrt(skills.Level(Protos.Skill.Shield)) * 2) / 100);
                    }
                }
                else
                {
                    if (attackCount == 1)
                        isWieldingTwoHanded = true;
                    else if (attackCount > 0)
                        isDualWielding = true;
                }
            }

            return new(attackCount, isWieldingShield, isWieldingTwoHanded, isDualWielding);
        }

        private readonly PrototypeId<SoundPrototype>[] KillSounds = new[]
        {
            Sound.Kill1,
            Sound.Kill2,
        };

        private void KillEntity(EntityUid target, MetaDataComponent? metaData = null, SpatialComponent? spatial = null)
        {
            if (!Resolve(target, ref metaData))
                return;

            _sounds.Play(_random.Pick(KillSounds), target);

            // TODO
            if (Resolve(target, ref spatial))
                _mapDebris.SpillBlood(spatial.MapPosition, 5);

            // TODO
            if (TryComp<CharaComponent>(target, out var chara))
            {
                chara.Liveness = CharaLivenessState.Dead;
            }
            else
            {
                metaData.Liveness = EntityGameLiveness.DeadAndBuried;
            }
        }

        public TurnResult? MeleeAttack(EntityUid attacker, EntityUid target)
        {
            var ev = new GetMeleeWeaponsEvent();
            RaiseEvent(attacker, ev);

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
            return Skill.MartialArts;
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
            RaiseEvent(attacker, ev);
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
