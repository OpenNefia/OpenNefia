local VisualAI = _.VisualAI

OpenNefia.Prototypes.VisualAI.Block.VisualAI = {
    ActionMoveCloseAsPossible = {
        Name = "Move as close to target as possible",
    },
    ActionMoveWithinDistance = {
        Name = function(vars)
            local threshold = vars.action.threshold
            return ("Move to within %d tile%s of target"):format(threshold, _.plural(threshold))
        end,
    },
    ActionRetreatFromTarget = {
        Name = "Move away from target as far as possible",
    },
    ActionRetreatUntilDistance = {
        Name = function(vars)
            local threshold = vars.action.threshold
            return ("Move back until %d tile%s away from target"):format(threshold, _.plural(threshold))
        end,
    },
    ActionMeleeAttack = {
        Name = "Melee attack",
    },
    ActionRangedAttack = {
        Name = "Ranged attack",
    },
    ActionCastSpell = {
        Name = function(vars)
            local skillName = VisualAI.formatSpell(vars.action.spellID) -- Magic Missile
            return ("Cast spell '%s'"):format(skillName)
        end,
    },
    ActionInvokeAction = {
        Name = function(vars)
            local skillName = VisualAI.formatAction(vars.action.actionID) -- Swarm
            return ("Invoke action '%s'"):format(skillName)
        end,
    },
    ActionChangeAmmo = {
        Name = "Change ammo to",
    },
    ActionPickUp = {
        Name = "Pick up",
    },
    ActionDrop = {
        Name = "Drop",
    },
    ActionEquip = {
        Name = "Equip",
    },
    ActionThrowPotion = {
        Name = "Throw a potion",
    },
    ActionThrowMonsterBall = {
        Name = "Throw a monster ball",
    },
    ActionStoreTarget = {
        Name = "Preserve current target to next turn",
    },
    ActionWander = {
        Name = "Wander",
    },
    ActionDoNothing = {
        Name = "Do nothing",
    },
}
