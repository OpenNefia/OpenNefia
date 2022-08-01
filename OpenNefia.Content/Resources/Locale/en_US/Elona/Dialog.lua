Elona.Dialog = {
    Common = {
        Choices = {
            More = "(More)",
            Bye = "Bye bye.",
        },
    },

    Impression = {
        Modify = {
            Gain = function(chara, newLevel)
                return ("Your relation with %s becomes <%s>!"):format(_.basename(chara), newLevel)
            end,
            Lose = function(chara, newLevel)
                return ("Your relation with %s becomes <%s>..."):format(_.basename(chara), newLevel)
            end,
        },
        Levels = {
            ["0"] = "Foe",
            ["1"] = "Hate",
            ["2"] = "Annoying",
            ["3"] = "Normal",
            ["4"] = "Amiable",
            ["5"] = "Friend",
            ["6"] = "Fellow",
            ["7"] = "Soul Mate",
            ["8"] = "*Love*",
        },
    },

    SpeakerName = {
        Fame = function(fame)
            return ("Fame: %s"):format(fame)
        end,
        ShopRank = function(shopRank)
            return ("Shop Rank: %s"):format(shopRank)
        end,
    },

    Layer = {
        Topic = {
            Impress = "Impress",
            Attract = "Attract",
        },
    },
}
