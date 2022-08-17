Elona.GameObjects.Stack = {
    HasBeenStacked = function(entity, totalCount)
        return ("%s has been stacked. (Total:%s)"):format(_.name(entity), totalCount)
    end,
}
