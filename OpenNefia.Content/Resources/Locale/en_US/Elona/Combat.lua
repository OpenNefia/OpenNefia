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
        furthermore = "Futhermore,",
    },
}
