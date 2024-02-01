OpenNefia.Prototypes.VisualAI.Block.VisualAI = {
    ActionMoveCloseAsPossible = {
        Name = "Move as close to target as possible",
    },
    ActionMoveWithinDistance = {
        Name = function(threshold)
            return ("Move to within %d tile%s of target"):format(threshold, _.plural(threshold))
        end,
    },
    ActionRetreatFromTarget = {
        Name = "Move away from target as far as possible",
    },
    ActionRetreatUntilDistance = {
        Name = function(threshold)
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
        Name = function(skill_name)
            return ("Cast spell '%s'"):format(skill_name)
        end,
    },
    ActionInvokeAction = {
        Name = function(skill_name)
            return ("Invoke action '%s'"):format(skill_name)
        end,
    },
    ActionChangeAmmo = {
        Name = "Change ammo to",
    },
    ActionPickUp = {
        Name = "Pick up",
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
