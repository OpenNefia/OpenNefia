Elona.CharaInfo.SkillsList = {
    Topic = {
        Name = "Name",
        Level = "Lv(Potential)",
        Detail = "Detail",
    },

    Category = {
        Skill = "Skill",
        WeaponProficiency = "Weapon Proficiency",
        Resistance = "Resistance",
    },

    Resist = {
        Name = function(elementName)
            return ("Resist %s"):format(elementName)
        end,
        Grade = {
            ["0"] = "Critically Weak",
            ["1"] = "Weak",
            ["2"] = "No Resist",
            ["3"] = "Little",
            ["4"] = "Normal",
            ["5"] = "Strong",
            ["6"] = "Superb",
        },
    },

    BonusPointsRemaining = function(bonusPoints)
        return ("You can spend %s bonus points."):format(bonusPoints)
    end,
}
