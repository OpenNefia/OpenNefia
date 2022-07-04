Elona.Combat = {
    PhysicalAttack = {
        CriticalHit = "Critical Hit!",
        Vorpal = {
            Melee = "*vopal*",
            Ranged = "*vopal*",
        },
        Miss = {
            Ally = function(attacker, target)
                return ("%s evade%s %s."):format(_.name(target), _.s(target), _.name(attacker))
            end,
            Other = function(attacker, target)
                return ("%s miss%s %s."):format(_.name(attacker), _.s(attacker, true), _.name(target))
            end,
        },
        Evade = {
            Ally = function(attacker, target)
                return ("%s skillfully evade%s %s."):format(_.name(target), _.s(target), _.name(attacker))
            end,
            Other = function(attacker, target)
                return ("%s skillfully evade%s %s."):format(_.name(target), _.s(target), _.name(attacker))
            end,
        },
        Furthermore = "Futhermore,",
        WieldsProudly = function(wielder, itemName)
            return ("%s wield%s %s proudly."):format(_.name(wielder), _.s(wielder), itemName)
        end,
    },
    Damage = {
        Levels = {
            Scratch = function(entity, attacker)
                return ("make%s a scratch."):format(_.s(attacker))
            end,
            Slightly = function(entity, attacker)
                return ("slightly wound%s %s."):format(_.s(attacker), _.him(entity))
            end,
            Moderately = function(entity, attacker)
                return ("moderately wound%s %s."):format(_.s(attacker), _.him(entity))
            end,
            Severely = function(entity, attacker)
                return ("severely wound%s %s."):format(_.s(attacker), _.him(entity))
            end,
            Critically = function(entity, attacker)
                return ("critically wound%s %s!"):format(_.s(attacker), _.him(entity))
            end,
        },
        Reactions = {
            Screams = function(entity)
                return ("%s scream%s."):format(_.name(entity), _.s(entity))
            end,
            WrithesInPain = function(entity)
                return ("%s writhe%s in pain."):format(_.name(entity), _.s(entity))
            end,
            IsSeverelyHurt = function(entity)
                return ("%s %s severely hurt!"):format(_.name(entity), _.is(entity))
            end,
            IsHealed = function(entity)
                return ("%s %s healed."):format(_.name(entity), _.is(entity))
            end,
        },
        RunsAway = function(entity)
            return ("%s run%s away in terror."):format(_.name(entity), _.s(entity))
        end,
        SleepIsDisturbed = function(entity)
            return ("%s%s sleep %s disturbed."):format(_.name(entity), _.his_owned(entity), _.is(entity))
        end,
    },
}
