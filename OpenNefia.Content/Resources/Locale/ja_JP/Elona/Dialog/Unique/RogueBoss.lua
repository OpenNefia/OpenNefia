Elona.Dialog.Unique.RogueBoss = {
    TooPoor = {
        Text = function(speaker, player)
            return ("チッ、一文無しの乞食%sとっとといっちまい%s"):format(
                _.ka(speaker),
                _.na(speaker)
            )
        end,
    },

    Ambush = {
        dialog = function(speaker, rogueGroupName, surrenderCost)
            return (
                "おまえさん、ついてない%s%s達は泣く子も黙る冷血な盗賊団、その名も%s%s命が惜しければ、おとなしく荷車の積荷と金貨%s枚を渡すがいい%s"
            ):format(
                _.na(speaker),
                _.ore(speaker, 3),
                rogueGroupName,
                _.da(speaker),
                surrenderCost,
                _.yo(speaker)
            )
        end,
        Choices = {
            Surrender = "I surrender.",
            TryMe = "Try me.",
        },
    },

    TryMe = {
        Text = function(speaker)
            return ("いい度胸%s…しかし、賢い選択とは言えない%sここがおまえさんの墓場%s"):format(
                _.da(speaker),
                _.na(speaker),
                _.da(speaker)
            )
        end,
    },

    Surrender = {
        Text = function(speaker)
            return ("なかなか賢明な判断%s"):format(_.da(speaker))
        end,
    },
}
