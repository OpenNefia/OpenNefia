Elona.Dialog = {
    Common = {
        Choices = {
            More = "(继续)",
            Bye = "再见",
        },

        Thanks = function(speaker)
            return _.thanks(speaker, 2)
        end,
        YouKidding = function(speaker)
            return ("戏弄%s中"):format(_.ka(speaker, 1))
        end,

        WillNotListen = function(entity)
            return ("%s不愿倾听。"):format(_.name(entity))
        end,
        IgnoresYou = "...(忽略你)",
        IsBusy = function(speaker)
            return ("(%s正在忙碌中...)"):format(_.name(speaker))
        end,
        IsSleeping = function(speaker)
            return ("(%s正在熟睡中...)"):format(_.name(speaker))
        end,
        YouHandOver = function(player, item)
            return ("%s递给了%s。"):format(_.sore_wa(player), _.name(item, nil, 1))
        end,
    },

    Impression = {
        Modify = {
            Gain = function(chara, newLevel)
                return ("与%s的关系变为<%s>！"):format(_.basename(chara), newLevel)
            end,
            Lose = function(chara, newLevel)
                return ("与%s的关系变为<%s>..."):format(_.basename(chara), newLevel)
            end,
        },
        Levels = {
            ["0"] = "天敌",
            ["1"] = "讨厌",
            ["2"] = "烦人",
            ["3"] = "普通",
            ["4"] = "友善",
            ["5"] = "朋友",
            ["6"] = "密友",
            ["7"] = "灵魂之友",
            ["8"] = "*爱*",
        },
    },

    SpeakerName = {
        Fame = function(fame)
            return ("名声：%s"):format(fame)
        end,
        ShopRank = function(shopRank)
            return ("商店规模：%s"):format(shopRank)
        end,
    },

    Layer = {
        Topic = {
            Impress = "友好",
            Attract = "兴趣",
        },
    },
}