Elona.Combat = {
    PhysicalAttack = {
        CriticalHit = "会心の一撃！ ",
        Vorpal = {
            Melee = " *シャキーン* ",
            Ranged = " *ズバシュッ* ",
        },
        Miss = {
            Ally = function(attacker, target)
                return ("%s%sの攻撃を華麗に避けた。"):format(_.kare_wa(target), _.name(attacker))
            end,
            Other = function(attacker, target)
                return ("%s攻撃を華麗にかわされた。"):format(_.kare_wa(attacker))
            end,
        },
        Evade = {
            Ally = function(attacker, target)
                return ("%s%sの攻撃を避けた。"):format(_.kare_wa(target), _.name(attacker))
            end,
            Other = function(attacker, target)
                return ("%s攻撃をかわされた。"):format(_.kare_wa(attacker))
            end,
        },
        furthermore = "さらに",
    },
}
