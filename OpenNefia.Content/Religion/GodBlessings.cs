using OpenNefia.Content.Effects;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Maps;
using OpenNefia.Content.Prototypes;

namespace OpenNefia.Content.Religion
{
    public sealed class GodBlessingManiEffect : IEffect
    {
        [Dependency] private readonly IReligionSystem _religion = default!;

        public EffectResult Apply(EntityUid source, MapCoordinates coords, EntityUid target, EffectArgs args)
        {
            _religion.ApplySkillBlessing(target, Protos.Skill.AttrDexterity, 400, 8);
            _religion.ApplySkillBlessing(target, Protos.Skill.AttrPerception, 300, 14);
            _religion.ApplySkillBlessing(target, Protos.Skill.Healing, 500, 8);
            _religion.ApplySkillBlessing(target, Protos.Skill.Firearm, 250, 18);
            _religion.ApplySkillBlessing(target, Protos.Skill.Detection, 350, 8);
            _religion.ApplySkillBlessing(target, Protos.Skill.LockPicking, 250, 16);
            _religion.ApplySkillBlessing(target, Protos.Skill.Carpentry, 300, 10);
            _religion.ApplySkillBlessing(target, Protos.Skill.Jeweler, 350, 12);
            return EffectResult.Succeeded;
        }
    }

    public sealed class GodBlessingLulwyEffect : IEffect
    {
        [Dependency] private readonly IReligionSystem _religion = default!;

        public EffectResult Apply(EntityUid source, MapCoordinates coords, EntityUid target, EffectArgs args)
        {
            _religion.ApplySkillBlessing(target, Protos.Skill.AttrPerception, 450, 10);
            _religion.ApplySkillBlessing(target, Protos.Skill.AttrSpeed, 350, 30);
            _religion.ApplySkillBlessing(target, Protos.Skill.Bow, 350, 16);
            _religion.ApplySkillBlessing(target, Protos.Skill.Crossbow, 450, 12);
            _religion.ApplySkillBlessing(target, Protos.Skill.Stealth, 450, 12);
            _religion.ApplySkillBlessing(target, Protos.Skill.MagicDevice, 550, 8);
            return EffectResult.Succeeded;
        }
    }

    public sealed class GodBlessingItzpaltEffect : IEffect
    {
        [Dependency] private readonly IReligionSystem _religion = default!;

        public EffectResult Apply(EntityUid source, MapCoordinates coords, EntityUid target, EffectArgs args)
        {
            _religion.ApplySkillBlessing(target, Protos.Skill.AttrMagic, 300, 18);
            _religion.ApplySkillBlessing(target, Protos.Skill.Meditation, 350, 15);
            _religion.ApplyResistBlessing(target, Protos.Element.Fire, 50, 200);
            _religion.ApplyResistBlessing(target, Protos.Element.Cold, 50, 200);
            _religion.ApplyResistBlessing(target, Protos.Element.Lightning, 50, 200);
            return EffectResult.Succeeded;
        }
    }

    public sealed class GodBlessingEhekatlEffect : IEffect
    {
        [Dependency] private readonly IReligionSystem _religion = default!;

        public EffectResult Apply(EntityUid source, MapCoordinates coords, EntityUid target, EffectArgs args)
        {
            _religion.ApplySkillBlessing(target, Protos.Skill.AttrCharisma, 250, 20);
            _religion.ApplySkillBlessing(target, Protos.Skill.AttrLuck, 100, 50);
            _religion.ApplySkillBlessing(target, Protos.Skill.Evasion, 300, 15);
            _religion.ApplySkillBlessing(target, Protos.Skill.MagicCapacity, 350, 17);
            _religion.ApplySkillBlessing(target, Protos.Skill.Fishing, 300, 12);
            _religion.ApplySkillBlessing(target, Protos.Skill.LockPicking, 450, 8);
            return EffectResult.Succeeded;
        }
    }

    public sealed class GodBlessingOpatosEffect : IEffect
    {
        [Dependency] private readonly IReligionSystem _religion = default!;

        public EffectResult Apply(EntityUid source, MapCoordinates coords, EntityUid target, EffectArgs args)
        {
            _religion.ApplySkillBlessing(target, Protos.Skill.AttrStrength, 450, 11);
            _religion.ApplySkillBlessing(target, Protos.Skill.AttrConstitution, 350, 16);
            _religion.ApplySkillBlessing(target, Protos.Skill.Shield, 350, 15);
            _religion.ApplySkillBlessing(target, Protos.Skill.WeightLifting, 300, 16);
            _religion.ApplySkillBlessing(target, Protos.Skill.Mining, 350, 12);
            _religion.ApplySkillBlessing(target, Protos.Skill.MagicDevice, 450, 8);
            return EffectResult.Succeeded;
        }
    }

    public sealed class GodBlessingJureEffect : IEffect
    {
        [Dependency] private readonly IReligionSystem _religion = default!;

        public EffectResult Apply(EntityUid source, MapCoordinates coords, EntityUid target, EffectArgs args)
        {
            _religion.ApplySkillBlessing(target, Protos.Skill.AttrWill, 300, 16);
            _religion.ApplySkillBlessing(target, Protos.Skill.Healing, 250, 18);
            _religion.ApplySkillBlessing(target, Protos.Skill.Meditation, 400, 10);
            _religion.ApplySkillBlessing(target, Protos.Skill.Anatomy, 400, 9);
            _religion.ApplySkillBlessing(target, Protos.Skill.Cooking, 450, 8);
            _religion.ApplySkillBlessing(target, Protos.Skill.MagicDevice, 400, 10);
            _religion.ApplySkillBlessing(target, Protos.Skill.MagicCapacity, 400, 12);
            return EffectResult.Succeeded;
        }
    }

    public sealed class GodBlessingKumiromiEffect : IEffect
    {
        [Dependency] private readonly IReligionSystem _religion = default!;

        public EffectResult Apply(EntityUid source, MapCoordinates coords, EntityUid target, EffectArgs args)
        {
            _religion.ApplySkillBlessing(target, Protos.Skill.AttrPerception, 400, 8);
            _religion.ApplySkillBlessing(target, Protos.Skill.AttrDexterity, 350, 12);
            _religion.ApplySkillBlessing(target, Protos.Skill.AttrLearning, 250, 16);
            _religion.ApplySkillBlessing(target, Protos.Skill.Gardening, 300, 12);
            _religion.ApplySkillBlessing(target, Protos.Skill.Alchemy, 350, 10);
            _religion.ApplySkillBlessing(target, Protos.Skill.Tailoring, 350, 9);
            _religion.ApplySkillBlessing(target, Protos.Skill.Literacy, 350, 8);
            return EffectResult.Succeeded;
        }
    }
}
