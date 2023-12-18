Elona.Spells = {
    Prompt = {
        WhichDirection = "Which direction do you want to cast the spell?",
        OvercastWarning = "You are going to over-cast the spell. Are you sure?",
    },

    Cast = {
        Confused = function(caster)
            return ("%s try%s to cast a spell in confusion."):format(_.name(caster), _.s(caster))
        end,
        Silenced = "The mist of silence interrupts a spell.",
        Fail = function(caster)
            return ("%s fail%s to cast a spell."):format(_.name(caster), _.s(caster))
        end,
    },

    CastingStyle = {
        Default = {
            Generic = function(caster)
                return ("%s cast%s a spell."):format(_.name(caster), _.s(caster))
            end,
            WithSkillName = function(caster, target, skillName)
                return ("%s cast%s %s."):format(_.name(caster), _.s(caster), skillName)
            end,
        },
        Elona = {
            Spider = {
                Generic = function(caster)
                    return ("%s split%s cobweb."):format(_.name(caster), _.s(caster))
                end,
                WithSkillName = function(caster, target, skillName)
                    return ("%s split%s a cobweb of %s."):format(_.name(caster), _.s(caster), skillName)
                end,
            },
            Spill = {
                Generic = function(caster)
                    return ("%s spread%s body fluid."):format(_.name(caster), _.s(caster))
                end,
                WithSkillName = function(caster, target, skillName)
                    return ("%s spread%s body fluid of %s."):format(_.name(caster), _.s(caster), skillName)
                end,
            },
            Tentacle = {
                Generic = function(caster)
                    return ("%s put%s out a tentacle."):format(_.name(caster), _.s(caster))
                end,
                WithSkillName = function(caster, target, skillName)
                    return ("%s put%s out a tentacle of %s."):format(_.name(caster), _.s(caster), skillName)
                end,
            },
            Gaze = {
                Generic = function(caster)
                    return ("%s gaze%s."):format(_.name(caster), _.s(caster))
                end,
                WithSkillName = function(caster, target, skillName)
                    return ("%s gaze%s at %s with %s."):format(
                        _.name(caster),
                        _.s(caster),
                        _.theTarget(caster, target),
                        skillName
                    )
                end,
            },
            Spore = {
                Generic = function(caster)
                    return ("%s scatter%s spores."):format(_.name(caster), _.s(caster))
                end,
                WithSkillName = function(caster, target, skillName)
                    return ("%s scatter%s %s spores."):format(_.name(caster), _.s(caster), skillName)
                end,
            },
            Machine = {
                Generic = function(caster)
                    return ("%s vibrate%s."):format(_.name(caster), _.s(caster))
                end,
                WithSkillName = function(caster, target, skillName)
                    return ("%s vibrate%s using %s."):format(_.name(caster), _.s(caster), skillName)
                end,
            },
        },
    },

    Layer = {
        Window = {
            Title = "Spell",
        },

        Topic = {
            Name = "Name",
            Cost = "Cost",
            Stock = "Stock",
            Lv = "Lv",
            Chance = "Chance",
            Effect = "Effect",
        },

        Stats = {
            Power = function(power)
                return ("Power:%s"):format(power)
            end,
            TurnCounter = function(turns)
                return ("%st"):format(turns)
            end,
        },
    },
}
