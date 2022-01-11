Elona.TargetText = {
    ItemOnCell = {
        And = "と",

        MoreThanThree = function(itemCount)
            return ("ここには%s種類のアイテムがある。"):format(itemCount)
        end,

        Item = function(itemNames)
            return ("%sが落ちている。"):format(itemNames)
        end,
        Construct = function(itemNames)
            return ("%sが設置されている。"):format(itemNames)
        end,
        NotOwned = function(itemNames)
            return ("%sがある。"):format(itemNames)
        end,
    },
}
