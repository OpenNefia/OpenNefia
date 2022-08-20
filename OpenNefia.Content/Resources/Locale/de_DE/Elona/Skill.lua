Elona.Skill = {
    Gained = function(skillName)
        return ("You have learned a new ability, %s."):format(skillName)
    end,

    Default = {
        OnDecrease = function(entity, skillName)
            return ("%s %s skill falls off."):format(_.possessive(entity), skillName)
        end,
        OnIncrease = function(entity, skillName)
            return ("%s %s skill increases."):format(_.possessive(entity), skillName)
        end,
    },
}
