Elona.GameObjects.Stack = {
    HasBeenStacked = function(entity, totalCount)
        return ("%sをまとめた(計%s個) "):format(_.name(entity), totalCount)
    end,
}
