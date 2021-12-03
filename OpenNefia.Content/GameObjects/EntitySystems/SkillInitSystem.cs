using System;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Prototypes;

namespace OpenNefia.Content.GameObjects
{
    public class SkillInitSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<CharaComponent, CharaInitEvent>(OnCharaInit);
        }

        private void OnCharaInit(EntityUid uid, CharaComponent component, CharaInitEvent args)
        {
            InitClassSkills(uid, component);
        }

        private void InitClassSkills(EntityUid uid, CharaComponent? chara = null,
            SkillsComponent? skills = null)
        {
            if (!Resolve(uid, ref chara, ref skills))
                return;

            foreach (var pair in chara.Class.ResolvePrototype().BaseSkills)
            {
                skills.Skills[pair.Key] = new LevelAndPotential(level: pair.Value);
            }
        }
    }

    public class CharaInitEvent : EntityEventArgs
    {
    }
}
