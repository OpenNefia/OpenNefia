Elona.Encounter = {
    Types = {
        Enemy = {
            Message = function(distNearestTown, rank)
                return ("Ambush! (Distance from nearest town:%s Enemy strength:%s)"):format(distNearestTown, rank)
            end,
            Rank = {
                Putit = "Putit Rank",
                Orc = "Orc Rank",
                GrizzlyBear = "Grizzly Bear Rank",
                Drake = "Drake Rank",
                Lich = "Lich Rank",
                Dragon = "Dragon Rank",
            },
        },
    },
}
