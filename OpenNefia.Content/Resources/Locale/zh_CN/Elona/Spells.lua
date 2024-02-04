Elona.Spells = {
    Prompt = {
        WhichDirection = "在哪个方向施放？",
        OvercastWarning = "魔力不足，是否尝试施放？",
    },

    Cast = {
        Confused = function(caster)
            return ("%s虽然混乱，但尝试着施放魔法。"):format(_.name(caster))
        end,
        Silenced = "沉默之雾阻止了施放魔法。",
        Fail = function(caster)
            return ("%s施放失败。"):format(_.name(caster))
        end,
    },

    CastingStyle = {
        Default = {
            Generic = function(caster)
                return ("%s开始咏唱魔法。"):format(_.sore_wa(caster))
            end,
            WithSkillName = function(caster, target, skillName)
                return ("%s咏唱%s的魔法。"):format(_.name(caster), skillName)
            end,
        },
        Elona = {
            Spider = {
                Generic = function(caster)
                    return ("%s吐出丝线。"):format(_.sore_wa(caster))
                end,
                WithSkillName = function(caster, target, skillName)
                    return ("%s吐出%s的丝线。"):format(_.name(caster), skillName)
                end,
            },
            Spill = {
                Generic = function(caster)
                    return ("%s喷洒体液。"):format(_.sore_wa(caster))
                end,
                WithSkillName = function(caster, target, skillName)
                    return ("%s喷洒%s的体液。"):format(_.name(caster), skillName)
                end,
            },
            Tentacle = {
                Generic = function(caster)
                    return ("%s伸出触手。"):format(_.sore_wa(caster))
                end,
                WithSkillName = function(caster, target, skillName)
                    return ("%s伸出%s的触手。"):format(_.name(caster), skillName)
                end,
            },
            Gaze = {
                Generic = function(caster)
                    return ("%s锐利地注视。"):format(_.sore_wa(caster))
                end,
                WithSkillName = function(caster, target, skillName)
                    return ("%s用%s的魔法锐利地注视。"):format(_.name(caster), skillName)
                end,
            },
            Spore = {
                Generic = function(caster)
                    return ("%s飞散孢子。"):format(_.sore_wa(caster))
                end,
                WithSkillName = function(caster, target, skillName)
                    return ("%s飞散%s的孢子。"):format(_.name(caster), skillName)
                end,
            },
            Machine = {
                Generic = function(caster)
                    return ("%s微微振动。"):format(_.sore_wa(caster))
                end,
                WithSkillName = function(caster, target, skillName)
                    return ("%s用%s微微振动。"):format(_.name(caster), skillName)
                end,
            },
        },
    },

    Description = {
        Power = function(power)
            return ("威力%s"):format(power)
        end,
        TurnCounter = function(turns)
            return ("%s回合"):format(turns)
        end,
    },

    Layer = {
        Window = {
            Title = "魔法咏唱",
        },

        Topic = {
            Name = "魔法名称",
            Cost = "消耗MP",
            Stock = "库存",
            Lv = "Lv",
            Chance = "成功率",
            Effect = "效果",
        },
    },
}