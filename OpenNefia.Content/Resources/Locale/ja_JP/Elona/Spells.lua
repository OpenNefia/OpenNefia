Elona.Spells = {
    Prompt = {
        WhichDirection = "どの方向に唱える？",
        OvercastWarning = "マナが足りないが、それでも詠唱を試みる？",
    },

    Cast = {
        Confused = function(caster)
            return ("%sは混乱しながらも魔法の詠唱を試みた。"):format(_.name(caster))
        end,
        Silenced = "沈黙の霧が詠唱を阻止した。",
        Fail = function(caster)
            return ("%sは詠唱に失敗した。"):format(_.name(caster))
        end,
    },

    CastingStyle = {
        Default = {
            Generic = function(caster)
                return ("%s魔法を詠唱した。"):format(_.sore_wa(caster))
            end,
            WithSkillName = function(caster, target, skillName)
                return ("%sは%sの魔法を詠唱した。"):format(_.name(caster), skillName)
            end,
        },
        Elona = {
            Spider = {
                Generic = function(caster)
                    return ("%s糸を吐いた。"):format(_.sore_wa(caster))
                end,
                WithSkillName = function(caster, target, skillName)
                    return ("%sは%sの糸を吐いた。"):format(_.name(caster), skillName)
                end,
            },
            Spill = {
                Generic = function(caster)
                    return ("%s体液をまき散らした。"):format(_.sore_wa(caster))
                end,
                WithSkillName = function(caster, target, skillName)
                    return ("%sは%sの体液をまき散らした。"):format(_.name(caster), skillName)
                end,
            },
            Tentacle = {
                Generic = function(caster)
                    return ("%s触手を伸ばした。"):format(_.sore_wa(caster))
                end,
                WithSkillName = function(caster, target, skillName)
                    return ("%sは%sの触手を伸ばした。"):format(_.name(caster), skillName)
                end,
            },
            Gaze = {
                Generic = function(caster)
                    return ("%s鋭く睨んだ。"):format(_.sore_wa(caster))
                end,
                WithSkillName = function(caster, target, skillName)
                    return ("%sは%sの魔法で鋭く睨んだ。"):format(_.name(caster), skillName)
                end,
            },
            Spore = {
                Generic = function(caster)
                    return ("%s胞子を飛ばした。"):format(_.sore_wa(caster))
                end,
                WithSkillName = function(caster, target, skillName)
                    return ("%sは%sの胞子を飛ばした。"):format(_.name(caster), skillName)
                end,
            },
            Machine = {
                Generic = function(caster)
                    return ("%s細かく振動した。"):format(_.sore_wa(caster))
                end,
                WithSkillName = function(caster, target, skillName)
                    return ("%sは%sで細かく振動した。"):format(_.name(caster), skillName)
                end,
            },
        },
    },

    Description = {
        Power = function(power)
            return ("ﾊﾟﾜｰ%s"):format(power)
        end,
        TurnCounter = function(turns)
            return ("%sﾀｰﾝ"):format(turns)
        end,
    },

    Layer = {
        Window = {
            Title = "魔法の詠唱",
        },

        Topic = {
            Name = "魔法の名称",
            Cost = "消費MP",
            Stock = "ｽﾄｯｸ",
            Lv = "Lv",
            Chance = "成功",
            Effect = "効果",
        },
    },
}
