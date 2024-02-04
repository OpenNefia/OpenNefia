OpenNefia.Prototypes.Entity.Elona = {
    EncRandomTeleport = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s触发随机传送"):format(_.kare_wa(item))
            end,
        },
    },
    EncSuckBlood = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s吸取使用者的生命之血"):format(_.kare_wa(item))
            end,
        },
    },
    EncSuckExperience = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s阻碍%s成长"):format(_.kare_wa(item), _.name(wielder, true))
            end,
        },
    },
    EncSummonCreature = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s召唤魔物"):format(_.kare_wa(item))
            end,
        },
    },
    EncPreventTeleport = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s阻碍传送"):format(_.kare_wa(item))
            end,
        },
    },
    EncResistBlindness = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s无效盲目状态"):format(_.kare_wa(item))
            end,
        },
    },
    EncResistParalysis = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s无效麻痹状态"):format(_.kare_wa(item))
            end,
        },
    },
    EncResistConfusion = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s无效混乱状态"):format(_.kare_wa(item))
            end,
        },
    },
    EncResistFear = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s无效恐惧状态"):format(_.kare_wa(item))
            end,
        },
    },
    EncResistSleep = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s无效睡眠状态"):format(_.kare_wa(item))
            end,
        },
    },
    EncResistPoison = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s无效中毒状态"):format(_.kare_wa(item))
            end,
        },
    },
    EncResistTheft = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s防止道具被偷窃"):format(_.kare_wa(item))
            end,
        },
    },
    EncResistRottenFood = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s轻松消化腐烂物品"):format(_.kare_wa(item))
            end,
        },
    },
    EncFastTravel = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s提高速度, 缩短世界地图移动时间"):format(
                    _.kare_wa(item)
                )
            end,
        },
    },
    EncResistEtherwind = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s保护%s不受以太风侵扰"):format(_.kare_wa(item), _.name(wielder, true))
            end,
        },
    },
    EncResistBadWeather = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s无效雷雨和雪的阻碍"):format(_.kare_wa(item))
            end,
        },
    },
    EncResistPregnancy = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s防止异物入侵体内"):format(_.kare_wa(item))
            end,
        },
    },
    EncFloat = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s使%s悬浮"):format(_.kare_wa(item), _.name(wielder, true))
            end,
        },
    },
    EncResistMutation = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s保护%s免受变异"):format(_.kare_wa(item), _.name(wielder, true))
            end,
        },
    },
    EncEnhanceSpells = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s提升魔法威力"):format(_.kare_wa(item))
            end,
        },
    },
    EncSeeInvisible = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s能够看见隐形存在"):format(_.kare_wa(item))
            end,
        },
    },
    EncAbsorbStamina = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s吸取攻击目标的耐力"):format(_.kare_wa(item))
            end,
        },
    },
    EncRagnarok = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s结束一切"):format(_.kare_wa(item))
            end,
        },
    },
    EncAbsorbMana = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s吸取攻击目标的法力"):format(_.kare_wa(item))
            end,
        },
    },
    EncPierceChance = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s增加完全贯穿攻击机会"):format(_.kare_wa(item))
            end,
        },
    },
    EncCriticalChance = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s增加暴击攻击机会"):format(_.kare_wa(item))
            end,
        },
    },
    EncExtraMeleeAttackChance = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s增加额外近战攻击机会"):format(_.kare_wa(item))
            end,
        },
    },
    EncExtraRangedAttackChance = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s增加额外远程攻击机会"):format(_.kare_wa(item))
            end,
        },
    },
    EncTimeStop = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s偶尔停止时间"):format(_.kare_wa(item))
            end,
        },
    },
    EncResistCurse = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s保护免受咒语影响"):format(_.kare_wa(item))
            end,
        },
    },
    EncStradivarius = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s提高演奏报酬的质量"):format(_.kare_wa(item))
            end,
        },
    },
    EncDamageResistance = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s减轻承受的物理伤害"):format(_.kare_wa(item))
            end,
        },
    },
    EncDamageImmunity = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s偶尔使受到的伤害无效"):format(_.kare_wa(item))
            end,
        },
    },
    EncDamageReflection = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s受到攻击时反弹伤害给对手"):format(_.kare_wa(item))
            end,
        },
    },
    EncCuresBleedingQuickly = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s止血"):format(_.kare_wa(item))
            end,
        },
    },
    EncCatchesGodSignals = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s接收神的信号"):format(_.kare_wa(item))
            end,
        },
    },
    EncDragonBane = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s对龙族有强大的效果"):format(_.kare_wa(item))
            end,
        },
    },
    EncUndeadBane = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s对不死族有强大的效果"):format(_.kare_wa(item))
            end,
        },
    },
    EncDetectReligion = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s揭示他人的信仰"):format(_.kare_wa(item))
            end,
        },
    },
    EncGould = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s使听众陶醉于深沉的音色中"):format(_.kare_wa(item))
            end,
        },
    },
    EncGodBane = {
        Enchantment = {
            Description = function(item, wielder, power)
                return ("%s对神有强大的效果"):format(_.kare_wa(item))
            end,
        },
    },
}