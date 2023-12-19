Elona.Oracle = {
    Message = {
        WasCreatedAt = function(itemName, mapName, areaName, date)
            return ("%sは%s年%s月に%sで生成された。"):format(itemName, date.Year, date.Month, mapName)
        end,
        WasHeldBy = function(itemName, mapName, areaName, ownerName, date)
            return ("%sは%s年%s月に%sの%sの手に渡った。"):format(
                itemName,
                date.Year,
                date.Month,
                areaName,
                ownerName
            )
        end,
    },

    Effect = {
        NoArtifacts = "まだ特殊なアイテムは生成されていない。",
        Cursed = "何かがあなたの耳元でささやいたが、あなたは聞き取ることができなかった。",
    },

    ConvertArtifact = function(oldItemName, item)
        return ("%sは%sに形を変えた。"):format(oldItemName, _.name(item, true))
    end,
}
