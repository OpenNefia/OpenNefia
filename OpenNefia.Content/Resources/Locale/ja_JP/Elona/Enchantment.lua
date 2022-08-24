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

        Ammo = {
            Description = function(item, wielder, ammoName, maxAmmo)
                return ("%s%sを装填できる [最大%s発]"):format(_.kare_wa(item), ammoName, maxAmmo)
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
}
