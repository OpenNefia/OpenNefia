Elona.CommonEffects = {
    Wet = {
        GetsWet = function(entity)
            return ("[%s]被弄湿了。"):format(_.name(entity))
        end,
        IsRevealed = function(entity)
            return ("[%s]的身影显现了出来。"):format(_.name(entity))
        end,
    },
}