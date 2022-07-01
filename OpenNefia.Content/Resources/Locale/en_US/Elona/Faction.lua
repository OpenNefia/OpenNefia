Elona.Faction = {
    HostileAction = {
        AnimalsGetExcited = "Animals get excited!",
        GetsFurious = function(us, them)
            return ("%s gets furious!"):format(_.name(them))
        end,
        GlaresAt = function(us, them)
            return ("%s glares at %s."):format(_.name(them), _.name(us))
        end,
    },
}
