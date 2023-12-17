Elona.Spells = {
    Prompt = {
        WhichDirection = "どの方向に唱える？",
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

        Stats = {
            Power = function(power)
                return ("ﾊﾟﾜｰ%s"):format(power)
            end,
            TurnCounter = function(turns)
                return ("%sﾀｰﾝ"):format(turns)
            end,
        },
    },
}
