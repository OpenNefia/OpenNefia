Elona.Rank = {
    Changed = function(rankName, oldPlace, newPlace, title)
        return ("ランク変動(%s %s位 → %s位) 《%s》"):format(rankName, oldPlace, newPlace, title)
    end,
    CloserToNextRank = "着実に次のランクに近づいている。",
}
