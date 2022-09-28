Elona.Oracle = {
    WasCreatedAt = function(item, map, day, month, year)
        return ("%sは%s年%s月に%sで生成された。"):format(_.basename(item), year, month, _.name(map))
    end,
    WasHeldBy = function(item, owner, map, day, month, year)
        return ("%sは%s年%s月に%sの%sの手に渡った。"):format(
            _.basename(item),
            year,
            month,
            _.name(item),
            _.basename(owner)
        )
    end,

    Effect = {
        NoArtifacts = "まだ特殊なアイテムは生成されていない。",
        Cursed = "何かがあなたの耳元でささやいたが、あなたは聞き取ることができなかった。",
    },

    ConvertArtifact = function(oldItemName, item)
        return ("%sは%sに形を変えた。"):format(oldItemName, _.name(item, true))
    end,
}
