Elona.Combat = {
    PhysicalAttack = {
        CriticalHit = "会心一击!",
        Vorpal = "*锋利的一声*",
        Miss = {
            Ally = function(attacker, target)
                return ("%s%s 闪避了 %s."):format(_.name(target), _.s(target), _.name(attacker))
            end,
            Other = function(attacker, target)
                return ("%s%s 未命中 %s."):format(_.name(attacker), _.s(attacker, false), _.name(target))
            end,
        },
        Evade = {
            Ally = function(attacker, target)
                return ("%s%s 灵巧地闪避了 %s."):format(_.name(target), _.s(target), _.name(attacker))
            end,
            Other = function(attacker, target)
                return ("%s%s 灵巧地闪避了 %s."):format(_.name(target), _.s(target), _.name(attacker))
            end,
        },
        WieldsProudly = function(wielder, itemName)
            return ("%s%s 自豪地挥舞着 %s."):format(_.name(wielder), _.s(wielder), itemName)
        end,
    },

    RangedAttack = {
        Vorpal = " *嗖嗖*",
        Errors = {
            Elona = {
                NoRangedWeapon = "没有装备远程武器。",
                NoAmmo = "需要装备箭/子弹。",
                WrongAmmoType = "箭/子弹的类型不符合要求。",
            },
        },
        LoadNormalAmmo = function(attacker)
            return ("%s 为%s 装填普通弹药."):format(_.name(attacker), _.s(attacker))
        end,
    },
}
