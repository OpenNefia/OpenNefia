Elona.Faction = {
    HostileAction = {
        AnimalsGetExcited = "家畜兴奋了！",
        GetsFurious = function(us, them)
            return ("%s愤怒了。"):format(_.name(them))
        end,
        GlaresAt = function(us, them)
            return ("%s瞪了一眼。"):format(_.name(them))
        end,
    },
}
