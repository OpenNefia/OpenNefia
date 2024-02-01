OpenNefia.Prototypes.VisualAI.Block.VisualAI = {
    ConditionTargetInSight = {
        Name = "Target is in sight",
    },
    ConditionHpMpSpThreshold = {
        Name = function(kind, comparator, threshold)
            return ("My %s is %s %s%%"):format(kind, comparator, threshold)
        end,
    },
    ConditionCanDoMeleeAttack = {
        Name = "Can do melee attack",
    },
    ConditionCanDoRangedAttack = {
        Name = "Can do ranged attack",
    },
    ConditionTargetTileDist = {
        Name = function(comparator, threshold)
            return ("Target is %s %s tile%s away"):format(comparator, threshold, _.plural(threshold))
        end,
    },
    ConditionSkillInRange = {
        Name = function(skill_name)
            return ("Skill '%s' is in range"):format(skill_name)
        end,
    },
    ConditionRandomChance = {
        Name = function(chance)
            return ("True %d%% of the time"):format(chance)
        end,
    },
}
