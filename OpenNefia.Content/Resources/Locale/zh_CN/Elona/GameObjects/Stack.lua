Elona.GameObjects.Stack = {
    HasBeenStacked = function(entity, totalCount)
        return ("%s已叠加(共%s个) "):format(_.name(entity), totalCount)
    end,
}