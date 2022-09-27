Elona.Dialog = {
    Common = {
        Choices = {
            More = "(More)",
            Bye = "Bye bye.",
        },

        Thanks = "Thanks!",
        YouKidding = "You kidding?",

        WillNotListen = function(entity)
            return ("%s won't listen."):format(_.name(entity))
        end,
        IgnoresYou = function(speaker)
            return ("(%s ignores you...)"):format(_.he(speaker))
        end,
        IsBusy = function(speaker)
            return ("(%s is in the middle of something.)"):format(_.name(speaker))
        end,
        IsSleeping = function(speaker)
            return ("(%s is sleeping.)"):format(_.name(speaker))
        end,
        YouHandOver = function(player, item)
            return ("%s hand%s over %s."):format(_.name(player), _.s(player), _.name(item, nil, 1))
        end,
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
