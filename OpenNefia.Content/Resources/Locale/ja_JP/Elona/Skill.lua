Elona.Skill = {
    Gained = function(skillName)
        return ("あなたは「%s」の能力を得た。"):format(skillName)
    end,

    Default = {
        OnDecrease = function(entity, skillName)
            return ("%sは%sの技術の衰えを感じた。"):format(_.name(entity), skillName)
        end,
        OnIncrease = function(entity, skillName)
            return ("%sは%sの技術の向上を感じた。"):format(_.name(entity), skillName)
        end,
    },

    Leveling = {
        GainNewBodyPart = function(entity, bodyPartName)
            return ("%sの身体から新たな%sが生えてきた！"):format(_.name(entity), bodyPartName)
        end,
    },
}
