Elona.CharaInfo.SkillsList = {
    Topic = {
        Name = "能力の名称",
        Level = "Lv(潜在)",
        Detail = "効果",
    },

    Category = {
        Skill = "◆ スキルと特殊能力",
        WeaponProficiency = "◆ 武器の専門",
        Resistance = "◆ 耐性と抵抗力",
    },

    Resist = {
        Name = function(elementName)
            return ("%s耐性"):format(elementName)
        end,
        Grade = {
            ["0"] = "致命的な弱点",
            ["1"] = "弱点",
            ["2"] = "耐性なし",
            ["3"] = "弱い耐性",
            ["4"] = "普通の耐性",
            ["5"] = "強い耐性",
            ["6"] = "素晴らしい耐性",
        },
    },

    BonusPointsRemaining = function(bonusPoints)
        return ("残り %s のボーナスをスキルに分配できる"):format(bonusPoints)
    end,
}
