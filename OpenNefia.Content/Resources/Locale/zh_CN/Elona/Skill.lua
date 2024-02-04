Elona.Skill = {
    Gained = function(skillName)
        return ("你获得了【%s】的能力。"):format(skillName)
    end,

    Default = {
        OnDecrease = function(entity, skillName)
            return ("%s感到自己的%s技术在减退。"):format(_.name(entity), skillName)
        end,
        OnIncrease = function(entity, skillName)
            return ("%s感到自己的%s技术在提高。"):format(_.name(entity), skillName)
        end,
    },

    Leveling = {
        GainNewBodyPart = function(entity, bodyPartName)
            return ("%s的身体长出了新的%s！"):format(_.name(entity), bodyPartName)
        end,
    },

    Fatigue = {
        Indicator = {
            Light = "轻度疲劳",
            Moderate = "中度疲劳",
            Heavy = "过度劳累",
        },
    },
}