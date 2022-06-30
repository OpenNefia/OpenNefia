Elona.Rank = {
    Changed = function(rankName, oldPlace, newPlace, title)
        return ("Ranking Change (%s %s -> %s) <%s>"):format(rankName, oldPlace, newPlace, title)
    end,
    CloserToNextRank = "You are one step closer to the next rank.",
}
