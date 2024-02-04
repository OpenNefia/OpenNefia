Elona.Enchantment = {
    PowerUnit = "*",

    Item = {
        ModifyAttribute = {
            Equipment = {
                Increases = function(item, wielder, skillName, power)
                    return ("%s的%s提升了%s"):format(_.he(item), skillName, power)
                end,
                Decreases = function(item, wielder, skillName, power)
                    return ("%s的%s降低了%s"):format(_.he(item), skillName, power)
                end,
            },
            Food = {
                Increases = function(item, wielder, skillName, power)
                    return ("%s具有增强%s的营养"):format(_.he(item), skillName)
                end,
                Decreases = function(item, wielder, skillName, power)
                    return ("%s含有减弱%s的毒素"):format(_.he(item), skillName)
                end,
            },
            Eaten = {
                Increases = function(chara, skillName)
                    return ("%s的%s已经发展得很好了。"):format(_.name(chara), skillName)
                end,
                Decreases = function(chara, skillName)
                    return ("%s的%s已经变得衰弱了。"):format(_.name(chara), skillName)
                end,
            },
        },

        ModifyResistance = {
            Increases = function(item, wielder, elementName)
                return ("%s具有对%s的耐性"):format(_.he(item), elementName)
            end,
            Decreases = function(item, wielder, elementName)
                return ("%s削弱了对%s的耐性"):format(_.he(item), elementName)
            end,
        },

        ModifySkill = {
            Increases = function(item, wielder, skillName, power)
                return ("%s提升了%s的技能"):format(_.he(item), skillName)
            end,
            Decreases = function(item, wielder, skillName, power)
                return ("%s降低了%s的技能"):format(_.he(item), skillName)
            end,
        },

        SustainAttribute = {
            Equipment = function(item, wielder, skillName, power)
                return ("%s保持了%s"):format(_.he(item), skillName)
            end,
            Food = function(item, wielder, skillName, power)
                return ("%s具有帮助%s成长的营养"):format(_.he(item), skillName)
            end,
            Eaten = function(chara, skillName)
                return ("%s进入了成长期的阶段。"):format(_.name(chara), skillName)
            end,
        },

        ElementalDamage = {
            Description = function(item, wielder, elementName, power)
                return ("%s造成额外的%s属性伤害"):format(_.he(item), elementName)
            end,
        },

        InvokeSpell = {
            Description = function(item, wielder, spellName, power)
                return ("%s释放了%s"):format(_.he(item), spellName)
            end,
        },

        Ammo = {
            Description = function(item, wielder, ammoName, maxAmmo)
                return ("%s可以装填%s [最大%s发]"):format(_.he(item), ammoName, maxAmmo)
            end,
        },

        SuckBlood = {
            BloodSucked = function(entity)
                return ("某物吸取了%s的血液。"):format(_.name(entity))
            end,
        },

        SuckExperience = {
            ExperienceReduced = function(entity)
                return ("%s变得不熟练了。"):format(_.name(entity))
            end,
        },

        SummonCreature = {
            CreatureSummoned = "魔力的漩涡召唤出了某物！",
        },
    },
}