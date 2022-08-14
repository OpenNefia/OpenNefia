using OpenNefia.Content.GameObjects;
using OpenNefia.Content.Qualities;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.Skills
{
    public interface ISkillAdjustsSystem : IEntitySystem
    {
        void RemoveAllSkillAdjusts(EntityUid entity, SkillAdjustsComponent? skillAdj = null);
    }

    public sealed class SkillAdjustsSystem : EntitySystem, ISkillAdjustsSystem
    {
        public override void Initialize()
        {
            SubscribeComponent<SkillAdjustsComponent, EntityRefreshEvent>(OnRefresh);
        }

        private void OnRefresh(EntityUid entity, SkillAdjustsComponent skillAdj, ref EntityRefreshEvent args)
        {
            if (!EntityManager.TryGetComponent<SkillsComponent>(entity, out var skills))
                return;

            foreach (var (skillId, adjust) in skillAdj.SkillAdjusts)
            {
                if (skills.TryGetKnown(skillId, out var skill))
                {
                    var amount = adjust;

                    if (EntityManager.TryGetComponent<QualityComponent>(entity, out var quality)
                        && quality.Quality >= Quality.Good)
                    {
                        amount = Math.Max(adjust, skill.Level.Base / 5);
                    }

                    skill.Level.Buffed += amount;
                    if (skill.Level.Buffed < 1)
                        skill.Level.Buffed = 1;
                }
            }
        }

        public void RemoveAllSkillAdjusts(EntityUid entity, SkillAdjustsComponent? skillAdj = null)
        {
            if (!Resolve(entity, ref skillAdj))
                return;

            skillAdj.SkillAdjusts.Clear();
        }
    }
}
