local VisualAI = _.VisualAI

OpenNefia.Prototypes.VisualAI.Block.VisualAI = {
    ConditionTargetInSight = {
        Name = "Target is in sight",
    },
    ConditionHpMpSpThreshold = {
        Name = function(vars)
            local kind = VisualAI.formatEnum(vars.condition.type) -- HP
            local comparison = VisualAI.formatEnum(vars.condition.comparison) -- ==
            local threshold = math.floor(vars.condition.threshold * 100) -- 50
            return ("My %s is %s %s%%"):format(kind, comparison, threshold)
        end,
    },
    ConditionCanDoMeleeAttack = {
        Name = "Can do melee attack",
    },
    ConditionCanDoRangedAttack = {
        Name = "Can do ranged attack",
    },
    ConditionTargetTileDist = {
        Name = function(vars)
            local comparison = VisualAI.formatEnum(vars.condition.comparison) -- ==
            local threshold = vars.condition.threshold -- 3
            return ("Target is %s %s tile%s away"):format(comparison, threshold, _.plural(threshold))
        end,
    },
    ConditionSpellInRange = {
        Name = function(vars)
            local skillName = VisualAI.formatSpell(vars.condition.spellID) -- Magic Missile
            return ("Spell '%s' is in range"):format(skillName)
        end,
    },
    ConditionActionInRange = {
        Name = function(vars)
            local skillName = VisualAI.formatAction(vars.condition.actionID) -- Swarm
            return ("Action '%s' is in range"):format(skillName)
        end,
    },
    ConditionRandomChance = {
        Name = function(vars)
            local probability = math.floor(vars.condition.probability * 100)
            return ("True %d%% of the time"):format(probability)
        end,
    },
}
