Elona.Combat = {
    PhysicalAttack = {
        CriticalHit = "会心の一撃！ ",
        Vorpal = " *シャキーン* ",
        Miss = {
            Ally = function(attacker, target)
                return ("%s%sの攻撃を華麗に避けた。"):format(_.sore_wa(target), _.name(attacker))
            end,
            Other = function(attacker, target)
                return ("%s攻撃を華麗にかわされた。"):format(_.sore_wa(attacker))
            end,
        },
        Evade = {
            Ally = function(attacker, target)
                return ("%s%sの攻撃を避けた。"):format(_.sore_wa(target), _.name(attacker))
            end,
            Other = function(attacker, target)
                return ("%s攻撃をかわされた。"):format(_.sore_wa(attacker))
            end,
        },
        WieldsProudly = function(wielder, itemName)
            return ("%sは%sを誇らしげに構えた。"):format(_.name(wielder), itemName)
        end,
    },
    RangedAttack = {
        Vorpal = " *ズバシュッ* ",
        Errors = {
            Elona = {
                NoRangedWeapon = "射撃用の道具を装備していない。",
                NoAmmo = "矢/弾丸を装備する必要がある。",
                WrongAmmoType = "矢/弾丸の種類が適していない。",
            },
        },
        LoadNormalAmmo = function(attacker)
            return ("%s通常弾を装填した。"):format(_.sore_wa(attacker))
        end,
    },
}
