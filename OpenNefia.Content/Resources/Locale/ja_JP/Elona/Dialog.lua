Elona.Dialog = {
    Common = {
        Choices = {
            More = "(続く)",
            Bye = "さようなら",
        },
        IgnoresYou = "…(あなたを無視している)",
    },

    Impression = {
        Modify = {
            Gain = function(chara, newLevel)
                return ("%sとの関係が<%s>になった！"):format(_.basename(chara), newLevel)
            end,
            Lose = function(chara, newLevel)
                return ("%sとの関係が<%s>になった…"):format(_.basename(chara), newLevel)
            end,
        },
        Levels = {
            ["0"] = "天敵",
            ["1"] = "嫌い",
            ["2"] = "うざい",
            ["3"] = "普通",
            ["4"] = "好意的",
            ["5"] = "友達",
            ["6"] = "親友",
            ["7"] = "魂の友",
            ["8"] = "*Love*",
        },
    },

    SpeakerName = {
        Fame = function(fame)
            return ("名声 %s"):format(fame)
        end,
        ShopRank = function(shopRank)
            return ("店の規模:%s"):format(shopRank)
        end,
    },

    Layer = {
        Topic = {
            Impress = "友好",
            Attract = "興味",
        },
    },
}
