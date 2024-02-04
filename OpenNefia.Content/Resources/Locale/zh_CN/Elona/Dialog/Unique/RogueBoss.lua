Elona.Dialog.Unique.RogueBoss = {
    TooPoor = {
        Text = function(speaker, player)
            return ("哼, 一无所有的乞丐%s赶快滚开%s"):format(
                _.ka(speaker),
                _.na(speaker)
            )
        end,
    },

    Ambush = {
        Text = function(speaker, rogueGroupName, surrenderCost)
            return (
                "你这家伙, 运气真差%s%s们是哭泣也不会有人理的冷血盗\n贼团, 名叫%s%s如果想保命的话, 就老老实实地交出货\n车的货物和金币%s枚%s"
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
            Surrender = "投降",
            TryMe = "试试看我",
        },
    },

    TryMe = {
        Text = function(speaker)
            return ("不错的胆量%s…但是, 并不是明智的选择%s这里就是你的\n坟墓%s"):format(
                _.da(speaker),
                _.na(speaker),
                _.da(speaker)
            )
        end,
    },

    Surrender = {
        Text = function(speaker)
            return ("相当明智的判断%s"):format(_.da(speaker))
        end,
    },
}