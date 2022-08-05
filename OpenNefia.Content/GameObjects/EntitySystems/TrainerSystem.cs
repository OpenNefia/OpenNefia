using Love;
using OpenNefia.Content.Combat;
using OpenNefia.Content.Logic;
using OpenNefia.Content.Maps;
using OpenNefia.Content.Prototypes;
using OpenNefia.Content.Skills;
using OpenNefia.Content.World;
using OpenNefia.Core;
using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Locale;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Random;
using OpenNefia.Core.SaveGames;
using OpenNefia.Core.Serialization.Manager.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.GameObjects.EntitySystems
{
    public interface ITrainerSystem : IEntitySystem
    {
        int TotalSkillsLearned { get; set; }

        IList<PrototypeId<SkillPrototype>> CalcLearnableSkills(EntityUid trainer);
        int CalcLearnSkillCost(EntityUid player, PrototypeId<SkillPrototype> skillID);
        int CalcTrainSkillCost(EntityUid player, PrototypeId<SkillPrototype> skillID);
        int CalcTrainPotentialAmount(EntityUid player, PrototypeId<SkillPrototype> skillID);
    }

    public sealed class TrainerSystem : EntitySystem, ITrainerSystem
    {
        [Dependency] private readonly ISkillsSystem _skills = default!;

        [RegisterSaveData("Elona.TrainerSystem.TotalSkillsLearned")]
        public int TotalSkillsLearned { get; set; } = 0;

        public int CalcTrainSkillCost(EntityUid player, PrototypeId<SkillPrototype> skillID)
        {
            return _skills.Level(player, skillID) / 5 + 2;
        }

        public int CalcLearnSkillCost(EntityUid player, PrototypeId<SkillPrototype> skillID)
        {
            return 15 + 3 * TotalSkillsLearned;
        }

        public int CalcTrainPotentialAmount(EntityUid player, PrototypeId<SkillPrototype> skillID)
        {
            return Math.Clamp(15 - _skills.Potential(player, skillID) / 15, 2, 15);
        }

        private PrototypeId<SkillPrototype>[] DefaultTrainableSkills = new[]
        {
            Protos.Skill.Detection,
            Protos.Skill.Evasion
        };

        public IList<PrototypeId<SkillPrototype>> CalcLearnableSkills(EntityUid trainer)
        {
            var skills = new List<PrototypeId<SkillPrototype>>();
            skills.AddRange(DefaultTrainableSkills);

            if (TryMap(trainer, out var map) && TryComp<MapTrainersComponent>(map.MapEntityUid, out var mapTrainers))
            {
                skills.AddRange(mapTrainers.LearnableSkills);
            }

            return skills;
        }
    }
}