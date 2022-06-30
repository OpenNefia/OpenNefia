using OpenNefia.Content.TurnOrder;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Activity;
using OpenNefia.Core.IoC;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Inventory;

namespace OpenNefia.Content.Skills
{
    public sealed partial class SkillsSystem
    {
        [Dependency] private readonly IActivitySystem _activities = default!;

        private void HandleTurnStarting(EntityUid uid, SkillsComponent skills, EntityTurnStartingEventArgs args)
        {
            if (args.Handled)
                return;

            skills.CanRegenerateThisTurn = true;

            if (_gameSession.IsPlayer(uid))
                GainExperienceAtTurnStart(uid, skills);
        }

        private void GainExperienceAtTurnStart(EntityUid uid, SkillsComponent skills)
        {
            if (!TryComp<TurnOrderComponent>(uid, out var turnOrder))
                return;

            var turn = turnOrder.TotalTurnsTaken % 10;
            if (turn == 1)
            {
                foreach (var member in _parties.EnumerateMembers(uid).Where(e => EntityManager.IsAlive(e)))
                {
                    GainHealingAndMeditationExperience(member);
                }
            }
            else if (turn == 2)
            {
                GainStealthExperience(uid, skills);
            }
            else if (turn == 3)
            {
                GainWeightLiftingExperience(uid, skills);
            }
            else if (turn == 4)
            {
                if (!_activities.HasActivity(uid))
                {
                    HealStamina(uid, 2, showMessage: false, skills);
                }
            }
        }

        private void GainHealingAndMeditationExperience(EntityUid uid, SkillsComponent? skills = null)
        {
            if (!Resolve(uid, ref skills))
                return;

            var exp = 0;
            if (skills.HP != skills.MaxHP)
            {
                var healing = Level(uid, Protos.Skill.Healing);
                if (healing < Level(uid, Protos.Skill.AttrConstitution))
                    exp = 5 + healing / 5;
            }
            GainSkillExp(uid, Protos.Skill.Healing, exp, 1000, skills: skills);

            exp = 0;
            if (skills.MP != skills.MaxMP)
            {
                var meditation = Level(uid, Protos.Skill.Meditation);
                if (meditation < Level(uid, Protos.Skill.AttrMagic))
                    exp = 5 + meditation / 5;
            }
            GainSkillExp(uid, Protos.Skill.Meditation, exp, 1000, skills: skills);
        }

        private void GainStealthExperience(EntityUid uid, SkillsComponent skills)
        {
            var exp = 2;

            if (TryMap(uid, out var map) && HasComp<MapTypeWorldMapComponent>(map.MapEntityUid) && _rand.OneIn(20))
                exp = 0;

            GainSkillExp(uid, Protos.Skill.Stealth, exp, 0, 1000, skills);
        }

        private void GainWeightLiftingExperience(EntityUid uid, SkillsComponent skills)
        {
            if (!TryComp<InventoryComponent>(uid, out var inv))
                return;

            var exp = 0;

            if (inv.BurdenType > BurdenType.None)
            {
                exp = 4;

                if (TryMap(uid, out var map) && HasComp<MapTypeWorldMapComponent>(map.MapEntityUid) && _rand.OneIn(20))
                    exp = 0;
            }

            GainSkillExp(uid, Protos.Skill.WeightLifting, exp, 0, 1000, skills: skills);
        }

        private void HandleTurnEnding(EntityUid uid, SkillsComponent skills, EntityTurnEndingEventArgs args)
        {
            if (skills.CanRegenerateThisTurn)
                Regenerate(uid, skills);
        }

        public void Regenerate(EntityUid uid, SkillsComponent? skills = null)
        {
            if (!Resolve(uid, ref skills))
                return;

            if (_rand.OneIn(6))
            {
                var hpDelta = _rand.Next(Level(uid, Protos.Skill.Healing) / 3 + 1) + 1;
                HealHP(uid, hpDelta, showMessage: false, skills);
            }
            if (_rand.OneIn(5))
            {
                var mpDelta = _rand.Next(Level(uid, Protos.Skill.Meditation) / 2 + 1) + 1;
                HealMP(uid, mpDelta, showMessage: false, skills);
            }
        }
    }
}
