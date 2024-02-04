Elona.CharaInfo.SkillsList = {
    Topic = {
        Name = "能力名称",
        Level = "Lv(潜在)",
        Detail = "效果",
    },

    Category = {
        Skill = "◆ 技能与特殊能力",
        WeaponProficiency = "◆ 武器专精",
        Resistance = "◆ 耐性与抵抗力",
    },

    Resist = {
        Name = function(elementName)
            return ("%s耐性"):format(elementName)
        end,
        Grade = {
            ["0"] = "致命的弱点",
            ["1"] = "弱点",
            ["2"] = "无耐性",
            ["3"] = "弱耐性",
            ["4"] = "普通耐性",
            ["5"] = "强耐性",
            ["6"] = "极佳耐性",
        },
    },

    BonusPointsRemaining = function(bonusPoints)
        return ("剩余 %s 点奖励可分配到技能上"):format(bonusPoints)
    end,
}