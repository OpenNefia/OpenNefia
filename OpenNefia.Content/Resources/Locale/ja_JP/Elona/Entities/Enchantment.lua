OpenNefia.Prototypes.Entity.Elona = {
    EncRandomTeleport = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%sランダムなテレポートを引き起こす"):format(_.kare_wa(item))
            end,
        },
    },

    EncSuckBlood = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s使用者の生き血を吸う"):format(_.kare_wa(item))
            end,
        },
    },

    EncSuckExperience = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s%sの成長を妨げる"):format(_.kare_wa(item), _.name(wielder, true))
            end,
        },
    },

    EncSummonCreature = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s魔物を呼び寄せる"):format(_.kare_wa(item))
            end,
        },
    },

    EncPreventTeleport = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%sテレポートを妨害する"):format(_.kare_wa(item))
            end,
        },
    },

    EncResistBlindness = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s盲目を無効にする"):format(_.kare_wa(item))
            end,
        },
    },

    EncResistParalysis = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s麻痺を無効にする"):format(_.kare_wa(item))
            end,
        },
    },

    EncResistConfusion = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s混乱を無効にする"):format(_.kare_wa(item))
            end,
        },
    },

    EncResistFear = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s恐怖を無効にする"):format(_.kare_wa(item))
            end,
        },
    },

    EncResistSleep = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s睡眠を無効にする"):format(_.kare_wa(item))
            end,
        },
    },

    EncResistPoison = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s毒を無効にする"):format(_.kare_wa(item))
            end,
        },
    },

    EncResistTheft = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%sアイテムを盗まれなくする"):format(_.kare_wa(item))
            end,
        },
    },

    EncResistRottenFood = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s腐ったものを難なく消化させる"):format(_.kare_wa(item))
            end,
        },
    },

    EncFastTravel = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s速度を上げ、ワールドマップでの移動時間を短くする"):format(
                    _.kare_wa(item)
                )
            end,
        },
    },

    EncResistEtherwind = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%sエーテルの風から%sを保護する"):format(_.kare_wa(item), _.name(wielder, true))
            end,
        },
    },

    EncResistBadWeather = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s雷雨と雪による足止めを無効にする"):format(_.kare_wa(item))
            end,
        },
    },

    EncResistPregnancy = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s異物の体内への侵入を防ぐ"):format(_.kare_wa(item))
            end,
        },
    },

    EncFloat = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s%sを浮遊させる"):format(_.kare_wa(item), _.name(wielder, true))
            end,
        },
    },

    EncResistMutation = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s%sを変異から保護する"):format(_.kare_wa(item), _.name(wielder, true))
            end,
        },
    },

    EncEnhanceSpells = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s魔法の威力を高める"):format(_.kare_wa(item))
            end,
        },
    },

    EncSeeInvisible = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s透明な存在を見ることを可能にする"):format(_.kare_wa(item))
            end,
        },
    },

    EncAbsorbStamina = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s攻撃対象からスタミナを吸収する"):format(_.kare_wa(item))
            end,
        },
    },

    EncRagnarok = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s全てを終結させる"):format(_.kare_wa(item))
            end,
        },
    },

    EncAbsorbMana = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s攻撃対象からマナを吸収する"):format(_.kare_wa(item))
            end,
        },
    },

    EncPierceChance = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s完全貫通攻撃発動の機会を増やす"):format(_.kare_wa(item))
            end,
        },
    },

    EncCriticalChance = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%sクリティカルヒットの機会を増やす"):format(_.kare_wa(item))
            end,
        },
    },

    EncExtraMeleeAttackChance = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s追加打撃の機会を増やす"):format(_.kare_wa(item))
            end,
        },
    },

    EncExtraRangedAttackChance = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s追加射撃の機会を増やす"):format(_.kare_wa(item))
            end,
        },
    },

    EncTimeStop = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s稀に時を止める"):format(_.kare_wa(item))
            end,
        },
    },

    EncResistCurse = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s呪いの言葉から保護する"):format(_.kare_wa(item))
            end,
        },
    },

    EncStradivarius = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s演奏報酬の品質を上げる"):format(_.kare_wa(item))
            end,
        },
    },

    EncDamageResistance = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s被る物理ダメージを軽減する"):format(_.kare_wa(item))
            end,
        },
    },

    EncDamageImmunity = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s被るダメージを稀に無効にする"):format(_.kare_wa(item))
            end,
        },
    },

    EncDamageReflection = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s攻撃された時、相手に切り傷のダメージを与える"):format(_.kare_wa(item))
            end,
        },
    },

    EncCuresBleedingQuickly = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s出血を抑える"):format(_.kare_wa(item))
            end,
        },
    },

    EncCatchesGodSignals = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s神が発する電波をキャッチする"):format(_.kare_wa(item))
            end,
        },
    },

    EncDragonBane = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s竜族に対して強力な威力を発揮する"):format(_.kare_wa(item))
            end,
        },
    },

    EncUndeadBane = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s不死者に対して強力な威力を発揮する"):format(_.kare_wa(item))
            end,
        },
    },

    EncDetectReligion = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s他者の信仰を明らかにする"):format(_.kare_wa(item))
            end,
        },
    },

    EncGould = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s深い音色で聴衆を酔わす"):format(_.kare_wa(item))
            end,
        },
    },

    EncGodBane = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s神に対して強力な威力を発揮する"):format(_.kare_wa(item))
            end,
        },
    },
}
