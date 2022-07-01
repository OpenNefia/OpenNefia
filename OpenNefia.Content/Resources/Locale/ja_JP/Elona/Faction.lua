Elona.Faction = {
    HostileAction = {
        AnimalsGetExcited = "家畜は興奮した！",
        GetsFurious = function(us, them)
            return ("%sは激怒した。"):format(_.name(them))
        end,
        GlaresAt = function(us, them)
            return ("%sは嫌な顔をした。"):format(_.name(them))
        end,
    },
}
