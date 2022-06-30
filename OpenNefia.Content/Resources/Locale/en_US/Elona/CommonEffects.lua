Elona.CommonEffects = {
    Wet = {
        GetsWet = function(entity)
            return ("%s get%s wet."):format(_.name(entity), _.s(entity))
        end,
        IsRevealed = function(entity)
            return ("%s %s revealed %s shape."):format(_.name(entity), _.is(entity), _.his(entity))
        end,
    },
}
