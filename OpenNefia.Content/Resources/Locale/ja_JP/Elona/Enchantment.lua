Elona.Enchantment = {
    PowerUnit = "*",

    Item = {
        ModifyAttribute = {
            Equipment = {
                Increases = function(item, wielder, skillName, power)
                    return ("%s%sを%s上げる"):format(_.kare_wa(item), skillName, power)
                end,
                Decreases = function(item, wielder, skillName, power)
                    return ("%s%sを%s下げる"):format(_.kare_wa(item), skillName, power)
                end,
            },
            Food = {
                Increases = function(item, wielder, skillName, power)
                    return ("%s%sを増強させる栄養をもっている"):format(_.kare_wa(item), skillName)
                end,
                Decreases = function(item, wielder, skillName, power)
                    return ("%s%sを減衰させる毒素を含んでいる"):format(_.kare_wa(item), skillName)
                end,
            },
            Eaten = {
                Increases = function(chara, skillName)
                    return ("%sの%sは発達した。"):format(_.name(chara), skillName)
                end,
                Decreases = function(chara, skillName)
                    return ("%sの%sは衰えた。"):format(_.name(chara), skillName)
                end,
            },
        },

        ModifyResistance = {
            Increases = function(item, wielder, elementName)
                return ("%s%sへの耐性を授ける"):format(_.kare_wa(item), elementName)
            end,
            Decreases = function(item, wielder, elementName)
                return ("%s%sへの耐性を弱化する"):format(_.kare_wa(item), elementName)
            end,
        },

        ModifySkill = {
            Increases = function(item, wielder, skillName, power)
                return ("%s%sの技能を下げる"):format(_.kare_wa(item), skillName)
            end,
            Decreases = function(item, wielder, skillName, power)
                return ("%s%sの技能を上げる"):format(_.kare_wa(item), skillName)
            end,
        },

        SustainAttribute = {
            Equipment = function(item, wielder, skillName, power)
                return ("%s%sを維持する"):format(_.kare_wa(item), skillName)
            end,
            Food = function(item, wielder, skillName, power)
                return ("%s%sの成長を助ける栄養をもっている"):format(_.kare_wa(item), skillName)
            end,
            Eaten = function(chara, skillName)
                return ("%sの%sは成長期に突入した。"):format(_.name(chara), skillName)
            end,
        },

        ElementalDamage = {
            Description = function(item, wielder, elementName, power)
                return ("%s%s属性の追加ダメージを与える"):format(_.kare_wa(item), elementName)
            end,
        },

        InvokeSpell = {
            Description = function(item, wielder, spellName, power)
                return ("%s%sを発動する"):format(_.kare_wa(item), spellName)
            end,
        },

        SuckBlood = {
            BloodSucked = function(entity)
                return ("何かが%sの血を吸った。"):format(_.name(entity))
            end,
        },

        SuckExperience = {
            ExperienceReduced = function(entity)
                return ("%sは未熟になった。"):format(_.name(entity))
            end,
        },

        SummonCreature = {
            CreatureSummoned = "魔力の渦が何かを召喚した！",
        },
    },

    Ego = {
        Major = {
            Elona = {
                Silence = function(name)
                    return ("静寂の%s"):format(name)
                end,
                ResBlind = function(name)
                    return ("耐盲目の%s"):format(name)
                end,
                ResConfuse = function(name)
                    return ("耐混乱の%s"):format(name)
                end,
                Fire = function(name)
                    return ("烈火の%s"):format(name)
                end,
                Cold = function(name)
                    return ("氷結の%s"):format(name)
                end,
                Lightning = function(name)
                    return ("稲妻の%s"):format(name)
                end,
                Healer = function(name)
                    return ("癒し手の%s"):format(name)
                end,
                ResParalyze = function(name)
                    return ("耐麻痺の%s"):format(name)
                end,
                ResFear = function(name)
                    return ("耐恐怖の%s"):format(name)
                end,
                ResSleep = function(name)
                    return ("睡眠防止の%s"):format(name)
                end,
                Defender = function(name)
                    return ("防衛者の%s"):format(name)
                end,
            },
        },
        Minor = {
            Elona = {
                Singing = function(name)
                    return ("唄う%s"):format(name)
                end,
                Servants = function(name)
                    return ("召使の%s"):format(name)
                end,
                Followers = function(name)
                    return ("従者の%s"):format(name)
                end,
                Howling = function(name)
                    return ("呻く%s"):format(name)
                end,
                Glowing = function(name)
                    return ("輝く%s"):format(name)
                end,
                Conspicuous = function(name)
                    return ("異彩の%s"):format(name)
                end,
                Magical = function(name)
                    return ("魔力を帯びた%s"):format(name)
                end,
                Enchanted = function(name)
                    return ("闇を砕く%s"):format(name)
                end,
                Mighty = function(name)
                    return ("強力な%s"):format(name)
                end,
                Trustworthy = function(name)
                    return ("頼れる%s"):format(name)
                end,
            },
        },
    },
}
