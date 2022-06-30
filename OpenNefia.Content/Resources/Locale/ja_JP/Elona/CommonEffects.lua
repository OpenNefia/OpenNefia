Elona.CommonEffects = {
    Wet = {
        GetsWet = function(entity)
            return ("%sは濡れた。"):format(_.name(entity))
        end,
        IsRevealed = function(entity)
            return ("%sの姿があらわになった。"):format(_.name(entity))
        end,
    },
}
