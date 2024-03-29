Elona.Dialog = {
    Common = {
        Choices = {
            More = "(続く)",
            Bye = "さようなら",
        },

        Thanks = function(speaker)
            return _.thanks(speaker, 2)
        end,
        YouKidding = function(speaker)
            return ("冷やかし%s"):format(_.ka(speaker, 1))
        end,

        WillNotListen = function(entity)
            return ("%sは耳を貸さない。"):format(_.name(entity))
        end,
        IgnoresYou = "…(あなたを無視している)",
        IsBusy = function(speaker)
            return ("(%sはお取り込み中だ…)"):format(_.name(speaker))
        end,
        IsSleeping = function(speaker)
            return ("(%sはぐっすり眠っている…)"):format(_.name(speaker))
        end,
        YouHandOver = function(player, item)
            return ("%s%sを手渡した。"):format(_.sore_wa(player), _.name(item, nil, 1))
        end,
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
